using BusinessLogic.DTO.RiskAnalysis;
using DataManagement.Models;

namespace BusinessLogic.Services;

public class RiskCalculatorService : IRiskCalculatorService
{
    public RiskAnalysis CalculateRisk(Wallet wallet)
    {
        var factors = new List<RiskFactor>
        {
            CalculateConcentration(wallet),
            CalculateLiquidity(wallet),
            CalculateMarketCap(wallet),
            CalculateDiversification(wallet),
            CalculateWalletAge(wallet)
        };
        var totalScore = Math.Clamp(factors.Sum(f => f.Points), 0, 100);
        
        return new RiskAnalysis
        {
            TotalScore = totalScore,
            RiskLevel = GetRiskLevel(totalScore),
            Factors = factors,
            Warnings = factors
                .Where(f => f.Points >= 10)
                .Select(f => f.Description)
                .ToList(),
        };
    }

    private RiskFactor CalculateConcentration(Wallet wallet)
    {
        if (wallet.Tokens == null || !wallet.Tokens.Any() || wallet.TotalValueUsd <= 0)
        {
            return new RiskFactor(RiskCategory.Concentration, 0, "No tokens to analyze");
        }
        
        var maxValueToken = wallet.Tokens.OrderByDescending(t => t.ValueUsd).FirstOrDefault();
        var maxConcentration = (maxValueToken.ValueUsd / wallet.TotalValueUsd) * 100;
        
        int points = 0;
        string description;
        if (maxConcentration > 80)
        {
            points = 25;
            description = $"Critical concentration: {maxConcentration:F1}% in {maxValueToken.Address}";
        }
        else if (maxConcentration > 50)
        {
            points = 15;
            description = $"High concentration: {maxConcentration:F1}% in {maxValueToken.Address}";
        }
        else if (maxConcentration > 30)
        {
            points = 8;
            description = $"Moderate concentration: {maxConcentration:F1}% in {maxValueToken.Address}";
        }
        else
        {
            points = 0;
            description = "Well diversified portfolio";
        }
        return new RiskFactor(RiskCategory.Concentration, points, description);
    }
    
    private RiskFactor CalculateLiquidity(Wallet wallet)
    {
        if (wallet.Tokens == null || !wallet.Tokens.Any())
        {
            return new RiskFactor(RiskCategory.Liquidity, 0, "No tokens to analyze");
        }

        var criticalTokens = wallet.Tokens.Where(t => t.LiquidityUsd < 10_000).ToList();
        var lowTokens = wallet.Tokens.Where(t => t.LiquidityUsd >= 10_000 && t.LiquidityUsd < 50_000).ToList();

        if (criticalTokens.Any())
        {
            return new RiskFactor(
                RiskCategory.Liquidity,
                20,
                $"{criticalTokens.Count} token(s) with critical liquidity (<$10K)"
            );
        }
    
        if (lowTokens.Any())
        {
            return new RiskFactor(
                RiskCategory.Liquidity,
                10,
                $"{lowTokens.Count} token(s) with low liquidity (<$50K)"
            );
        }

        return new RiskFactor(RiskCategory.Liquidity, 0, "All tokens have adequate liquidity");
    }

    private RiskFactor CalculateMarketCap(Wallet wallet)
    {
        if (wallet.Tokens == null || !wallet.Tokens.Any())
        {
            return new RiskFactor(RiskCategory.MarketCap, 0, "No tokens to analyze");
        }
        
        var lowMCapTokens = wallet.Tokens.Where(t => t.MarketCapUsd < 100_000).ToList();

        int points;
        string description;
        if (lowMCapTokens.Any())
        {
            return new RiskFactor(RiskCategory.MarketCap, 15, $"Low market cap: {lowMCapTokens.Count} tokens");
        }
        return new RiskFactor(RiskCategory.MarketCap, 0, "All tokens have good market cap");
    }

    private RiskFactor CalculateDiversification(Wallet wallet)
    {
        if (wallet.Tokens == null || !wallet.Tokens.Any())
        {
            return new RiskFactor(RiskCategory.Diversification, 0, "No tokens to analyze");
        }
        
        var tokensCount = wallet.Tokens.Count();
        if (tokensCount <= 1)
        {
            return new RiskFactor(RiskCategory.Diversification, 10, "Wallet contains only one or less tokens");
        }
        if(tokensCount > 1 && tokensCount <= 4)
        {
            return new RiskFactor(RiskCategory.Diversification, 5, "Wallet contains more than 1 but less than 5 tokens");
        }
        
        return new RiskFactor(RiskCategory.Diversification, 0, "Wallet contains more than 4 tokens ( which is good )");
    }

    private RiskFactor CalculateWalletAge(Wallet wallet)
    {
        if (wallet.Transactions == null || !wallet.Transactions.Any())
        {
            return new RiskFactor(RiskCategory.WalletAge, 5, "No transaction history");
        }
        
        var firstTransaction = wallet.Transactions.Min(t => t.BlockTime);
        var walletAge = DateTime.UtcNow - firstTransaction;
        if (walletAge.TotalDays < 7)
        {
            return new RiskFactor(
                RiskCategory.WalletAge,
                5,
                $"New wallet: {walletAge.TotalDays:F0} days old"
            );
        }
    
        if (walletAge.TotalDays < 30)
        {
            return new RiskFactor(
                RiskCategory.WalletAge,
                3,
                $"Young wallet: {walletAge.TotalDays:F0} days old"
            );
        }

        return new RiskFactor(
            RiskCategory.WalletAge,
            0,
            $"Established wallet: {walletAge.TotalDays:F0} days old"
        );
    }
    
    private static string GetRiskLevel(int score) => score switch
    {
        <= 20 => "Very Safe",
        <= 40 => "Low Risk",
        <= 60 => "Moderate",
        <= 80 => "High Risk",
        _ => "Critical"
    };
}