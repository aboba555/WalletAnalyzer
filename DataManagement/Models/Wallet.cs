
namespace DataManagement.Models;

public class Wallet
{
    public string WalletAddress {  get; set; }
    public IEnumerable<Token> Tokens {  get; set; }
    public IEnumerable<CryptoTransaction> Transactions {  get; set; }
    public decimal TotalValueUsd { get; set; }
}