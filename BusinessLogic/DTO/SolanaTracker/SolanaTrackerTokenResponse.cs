namespace BusinessLogic.DTO.SolanaTracker;

public class SolanaTrackerTokenResponse
{
    public string Address { get; set; }
    public decimal Balance { get; set; }
    public decimal Value { get; set; }
    public QuoteUsdValue Price { get; set; }
    public QuoteUsdValue MarketCap { get; set; }
    public QuoteUsdValue Liquidity { get; set; }
}

public class QuoteUsdValue
{
    public decimal Quote { get; set; }
    public decimal Usd { get; set; }
}