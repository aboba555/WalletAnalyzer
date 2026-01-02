using DataManagement.Models;

namespace BusinessLogic.Services;

public interface IWalletAggregatorService
{
    Task<Wallet> GetWalletInfoAsync(string walletAddress);
}