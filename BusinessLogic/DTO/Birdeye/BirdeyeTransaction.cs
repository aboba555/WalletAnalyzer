namespace BusinessLogic.DTO.Birdeye;

public class BirdeyeTransaction
{
    public string TxHash { get; set; }
    public int BlockNumber { get; set; }
    public string BlockTime { get; set; }
    public bool Status { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public decimal Fee { get; set; }
    public string MainAction { get; set; }
    public List<BirdeyeBalanceChangeResponse> BalanceChange { get; set; }
    public BirdeyeContractLabel ContractLabel { get; set; }
    public List<BirdeyeTokenTransfer> TokenTransfers { get; set; }
}