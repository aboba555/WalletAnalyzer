using DataManagement.Models;

namespace BusinessLogic.DTO.SolanaTracker;

public class SolanaTrackerResponse
{
    public List<SolanaTrackerTokenResponse> Tokens { get; set; }
    public decimal Total { get; set; }
    public decimal TotalSol { get; set; }
}