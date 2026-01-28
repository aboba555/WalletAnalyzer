namespace BusinessLogic.DTO.RiskAnalysis;

public class RiskAnalysis
{
    public int TotalScore { get; set; }
    
    public string RiskLevel { get; set; } = string.Empty;
    
    public List<RiskFactor> Factors { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
    
}

public class RiskFactor
{
    public string Category { get; set; }
    
    public int Points { get; set; }
    
    public string Description { get; set; }

    public RiskFactor(string category, int points, string description)
    {
        Category = category;
        Points = points;
        Description = description;
    }
}

public static class RiskCategory
{
    public const string Concentration = "concentration";
    public const string Liquidity = "liquidity";
    public const string MarketCap = "market_cap";
    public const string Diversification = "diversification";
    public const string ScamTokens = "scam_tokens";
    public const string WalletAge = "wallet_age";
    public const string StablecoinAllocation = "stablecoin_allocation";
    public const string BlueChipAllocation = "blue_chip_allocation";
}