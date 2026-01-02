namespace BusinessLogic.DTO.Birdeye;

public class BirdeyeTokenTransfer
{
    public string FromTokenAccount  { get; set; }
    public string ToTokenAccount { get; set; }
    public string FromUserAccount { get; set; }
    public string ToUserAccount { get; set; }
    public decimal TokenAmount { get; set; }
    public string Mint { get; set; }
    public bool TransferNative { get; set; }
    public bool IsScaledUiToken { get; set; }
    public int? Multiplier { get; set; }
}