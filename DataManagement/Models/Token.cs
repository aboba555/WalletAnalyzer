namespace DataManagement.Models;

public class Token
{
    public string Name { get; set; }
    public string Symbol {  get; set; }
    public string Address { get; set; }
    public decimal Balance { get; set; }
    public decimal ValueUsd { get; set; }
    public decimal PriceUsd { get; set; }
    public decimal MarketCapUsd { get; set; }
    public decimal LiquidityUsd  { get; set; }
}