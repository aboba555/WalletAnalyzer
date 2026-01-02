namespace DataManagement.Models;

public class CryptoTransaction
{
    public string TxHash { get; set; }
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
    public DateTime BlockTime { get; set; }
    public bool Status { get; set; }
    public decimal Fee { get; set; }
    public string Action { get; set; }
    public IEnumerable<BalanceChange> BalanceChange { get; set; }
}