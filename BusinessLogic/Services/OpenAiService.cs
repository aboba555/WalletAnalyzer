using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessLogic.DTO.OpenAI;
using BusinessLogic.DTO.RiskAnalysis;
using DataManagement.Models;

namespace BusinessLogic.Services;

public class OpenAiService(HttpClient httpClient, IRiskCalculatorService riskCalculatorService) : IOpenAiService
{
    public async Task<GptResponse> AnalyzeWalletAsync(Wallet wallet)
    {
        var riskAnalysis = riskCalculatorService.CalculateRisk(wallet);
        
        var (summary, recommendations) = await GenerateAiContentAsync(wallet, riskAnalysis);
        
        
        return new GptResponse
        {
            RiskScore = riskAnalysis.TotalScore,
            Summary = summary,
            Warnings = riskAnalysis.Warnings,
            Recommendations = recommendations
        };
    }

    private async Task<(string summary, List<string> recommendations)> GenerateAiContentAsync(
        Wallet wallet, 
        RiskAnalysis riskAnalysis)
    {
        var prompt = BuildPrompt(wallet, riskAnalysis);

        var requestBody = new
        {
            model = "gpt-5",
            messages = new[]
            {
                new { role = "system", content = "You are a blockchain analyst. Respond with ONLY valid JSON." },
                new { role = "user", content = prompt }
            },
        };

        var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", requestBody);
        
        if (!response.IsSuccessStatusCode)
        {
            return (
                GenerateFallbackSummary(wallet, riskAnalysis),
                GenerateFallbackRecommendations(riskAnalysis)
            );
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var openAiResult = await response.Content.ReadFromJsonAsync<OpenAIResponse>(options);
        var content = openAiResult?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrEmpty(content))
        {
            return (
                GenerateFallbackSummary(wallet, riskAnalysis),
                GenerateFallbackRecommendations(riskAnalysis)
            );
        }

        try
        {
            var cleanedJson = CleanJsonResponse(content);
            var parsed = JsonSerializer.Deserialize<AiContentResponse>(cleanedJson, options);
            
            return (
                parsed?.Summary ?? GenerateFallbackSummary(wallet, riskAnalysis),
                parsed?.Recommendations ?? GenerateFallbackRecommendations(riskAnalysis)
            );
        }
        catch
        {
            return (
                GenerateFallbackSummary(wallet, riskAnalysis),
                GenerateFallbackRecommendations(riskAnalysis)
            );
        }
    }

    private string BuildPrompt(Wallet wallet, RiskAnalysis riskAnalysis)
    {
        var topTokens = wallet.Tokens?
            .OrderByDescending(t => t.ValueUsd)
            .Take(3)
            .Select(t => $"{t.Symbol}: ${t.ValueUsd:F2} ({(t.ValueUsd / wallet.TotalValueUsd * 100):F1}%)")
            .ToList() ?? new List<string>();

        return $@"Analyze this Solana wallet and provide summary + recommendations.
        WALLET DATA:
        - Address: {wallet.WalletAddress}
        - Total Value: ${wallet.TotalValueUsd:F2}
        - Token Count: {wallet.Tokens?.Count() ?? 0}
        - Top Holdings: {string.Join(", ", topTokens)}

        RISK ANALYSIS (already calculated):
        - Risk Score: {riskAnalysis.TotalScore}/100
        - Risk Level: {riskAnalysis.RiskLevel}
        - Warnings: {string.Join("; ", riskAnalysis.Warnings)}

        Generate:
        1. Summary: 2-3 sentences describing portfolio composition and risk profile. Use specific numbers.
        2. Recommendations: 2-3 actionable tips based on the warnings. Be specific.

        Respond with ONLY this JSON:
        {{
          ""summary"": ""..."",
          ""recommendations"": [""..."", ""...""]
        }}";
    }

    private static string GenerateFallbackSummary(Wallet wallet, RiskAnalysis riskAnalysis)
    {
        var topToken = wallet.Tokens?.OrderByDescending(t => t.ValueUsd).FirstOrDefault();
        var topPercent = topToken != null && wallet.TotalValueUsd > 0
            ? (topToken.ValueUsd / wallet.TotalValueUsd * 100).ToString("F1")
            : "0";

        return $"Portfolio worth ${wallet.TotalValueUsd:F2} across {wallet.Tokens?.Count() ?? 0} tokens. " +
               $"Risk level: {riskAnalysis.RiskLevel} ({riskAnalysis.TotalScore}/100). " +
               $"Largest holding: {topToken?.Symbol ?? "N/A"} at {topPercent}%.";
    }

    private static List<string> GenerateFallbackRecommendations(RiskAnalysis riskAnalysis)
    {
        var recommendations = new List<string>();

        foreach (var factor in riskAnalysis.Factors.Where(f => f.Points >= 10))
        {
            var rec = factor.Category switch
            {
                RiskCategory.Concentration => "Consider reducing your largest position to below 30%",
                RiskCategory.Liquidity => "Avoid tokens with liquidity below $50K - high rug risk",
                RiskCategory.MarketCap => "Be cautious with micro-cap tokens (<$100K market cap)",
                RiskCategory.Diversification => "Diversify your portfolio across 5+ different tokens",
                _ => null
            };
            
            if (rec != null) recommendations.Add(rec);
        }

        return recommendations.Take(3).ToList();
    }

    private static string CleanJsonResponse(string content)
    {
        var cleaned = content.Trim();
        cleaned = Regex.Replace(cleaned, @"^```json\s*\n?", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"^```\s*\n?", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\n?```\s*$", "", RegexOptions.Multiline);

        var startIdx = cleaned.IndexOf('{');
        var endIdx = cleaned.LastIndexOf('}');

        if (startIdx >= 0 && endIdx > startIdx)
        {
            cleaned = cleaned.Substring(startIdx, endIdx - startIdx + 1);
        }

        return cleaned.Trim();
    }
}

internal class AiContentResponse
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}