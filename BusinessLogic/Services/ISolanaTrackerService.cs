using BusinessLogic.DTO.SolanaTracker;

namespace BusinessLogic.Services;

public interface ISolanaTrackerService
{
    Task<SolanaTrackerResponse> FetchWalletDataAsync(string walletAddress);
}