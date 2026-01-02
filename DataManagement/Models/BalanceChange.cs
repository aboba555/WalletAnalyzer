namespace DataManagement.Models;

public class BalanceChange
{
    public decimal Amount { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public int Decimals { get; set; }
    public string Address { get; set; }
    public string LogoUri { get; set; }
}