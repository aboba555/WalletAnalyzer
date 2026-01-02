using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessLogic.DTO.OpenAI;
using DataManagement.Models;

namespace BusinessLogic.Services;

public class OpenAiService(HttpClient httpClient) : IOpenAiService
{
    public async Task<GptResponse> AnalyzeWalletAsync(Wallet wallet)
    {
        var prompt = BuildPrompt(wallet);

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are an elite blockchain analyst. You MUST respond with ONLY valid JSON, no markdown, no code blocks, no additional text." },
                new { role = "user", content = prompt },
            },
            temperature = 0.7,
            max_tokens = 1500
        };
        
        var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", requestBody);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"OpenAI Error: {errorContent}");
            throw new Exception($"OpenAI API error: {response.StatusCode}");
        }
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var openAiResult = await response.Content.ReadFromJsonAsync<OpenAIResponse>(options);
        var content = openAiResult?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("OpenAI returned empty response");
        }
        
        
        var cleanedJson = CleanJsonResponse(content);
        var result = JsonSerializer.Deserialize<GptResponse>(cleanedJson, options);
        
        if (result == null)
        {
            throw new Exception("Failed to parse GPT response");
        }
        
        result.RiskScore = Math.Clamp(result.RiskScore, 0, 100);
        result.Warnings ??= new List<string>();
        result.Recommendations ??= new List<string>();
        
        return result;
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

    private string BuildPrompt(Wallet wallet)
    {
        var tokensSummary = wallet.Tokens?.Select(t => new
        {
            address = t.Address?[..Math.Min(12, t.Address?.Length ?? 0)] + "...",
            symbol = t.Symbol ?? "UNKNOWN",
            name = t.Name ?? "Unknown Token",
            balance = t.Balance,
            valueUsd = Math.Round(t.ValueUsd, 4),
            priceUsd = t.PriceUsd,
            marketCapUsd = t.MarketCapUsd,
            liquidityUsd = t.LiquidityUsd,
            portfolioPercent = wallet.TotalValueUsd > 0 
                ? Math.Round((t.ValueUsd / wallet.TotalValueUsd) * 100, 2) 
                : 0
        }).ToList();
        
        var txStats = AnalyzeTransactions(wallet.Transactions);

        var analysisData = new
        {
            walletAddress = wallet.WalletAddress,
            totalValueUsd = Math.Round(wallet.TotalValueUsd, 4),
            tokenCount = wallet.Tokens?.Count() ?? 0,
            tokens = tokensSummary,
            transactionStats = txStats
        };

        var dataJson = JsonSerializer.Serialize(analysisData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return $@"You are an elite Solana blockchain analyst specializing in DeFi risk assessment and portfolio analysis.

        WALLET DATA:
        {dataJson}

        ANALYZE AND EVALUATE:

        1. PORTFOLIO COMPOSITION
           - Total value and token distribution
           - Concentration risk (single token > 30% = high risk)
           - Diversification quality

        2. TOKEN RISK ASSESSMENT  
           - Identify risky tokens: marketCap < $100K or liquidity < $50K
           - Flag potential rugs/honeypots (extremely low liquidity)
           - SOL derivatives vs memecoins ratio

        3. TRADING BEHAVIOR
           - Transaction frequency and patterns
           - Send vs receive ratio
           - Average transaction size
           - Protocol interactions (swaps, staking, etc.)

        4. RED FLAGS
           - Tokens with liquidity < $10K (extreme rug risk)
           - Over 50% in single token
           - High fee transactions
           - Suspicious transaction patterns

        5. RISK SCORE CALCULATION
           - 0-20: Very Safe (diversified, blue chips only)
           - 21-40: Low Risk (mostly stable, minor exposure to risk)
           - 41-60: Moderate (some risky holdings, manageable)
           - 61-80: High Risk (significant memecoin/low-cap exposure)
           - 81-100: Critical (concentrated in high-risk assets)

        RESPOND WITH ONLY THIS JSON STRUCTURE (no markdown, no code blocks):
        {{
          ""summary"": ""Comprehensive 2-3 sentence assessment including total value, main holdings, and overall risk profile. Be specific with numbers and percentages."",
          ""riskScore"": <integer 0-100>,
          ""warnings"": [
            ""Specific warning with token names and exact percentages/values"",
            ""Another specific warning if applicable""
          ],
          ""recommendations"": [
            ""Specific actionable recommendation with concrete steps"",
            ""Another recommendation if applicable""
          ]
        }}

        IMPORTANT RULES:
        - Be SPECIFIC: use actual values, percentages, and token names from the data
        - Keep warnings and recommendations concise (under 200 chars each)
        - Maximum 3 warnings and 3 recommendations
        - If wallet is healthy, warnings can be empty array []
        - NO generic advice like ""do your own research""
        - NO markdown formatting in the response
        - ONLY output the JSON object, nothing else";
    }

    private static object AnalyzeTransactions(IEnumerable<CryptoTransaction>? transactions)
    {
        if (transactions == null || transactions.Count() == 0)
        {
            return new { count = 0, message = "No transaction history" };
        }

        var sends = transactions.Count(t => t.Action == "send");
        var receives = transactions.Count(t => t.Action == "received");
        var swaps = transactions.Count(t => t.Action == "unknown" || t.Action == "swap");
        var totalFees = transactions.Sum(t => t.Fee);

        var recentTxs = transactions
            .OrderByDescending(t => t.BlockTime)
            .Take(10)
            .Select(t => new
            {
                action = t.Action,
                time = t.BlockTime.ToString("yyyy-MM-dd"),
                tokens = t.BalanceChange?.Take(2).Select(bc => new 
                { 
                    symbol = bc.Symbol, 
                    amount = bc.Amount 
                })
            })
            .ToList();

        DateTime? firstTx = transactions.Min(t => t.BlockTime);
        DateTime? lastTx = transactions.Max(t => t.BlockTime);

        return new
        {
            totalCount = transactions.Count(),
            sends,
            receives,
            swapsOrOther = swaps,
            totalFeesLamports = totalFees,
            totalFeesSOL = Math.Round((double)totalFees / 1_000_000_000.0, 6),
            firstTransaction = firstTx?.ToString("yyyy-MM-dd"),
            lastTransaction = lastTx?.ToString("yyyy-MM-dd"),
            recentActivity = recentTxs
        };
    }
}