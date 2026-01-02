using BusinessLogic.DTO.Birdeye;

namespace BusinessLogic.Services;

public interface IBirdeyeService
{
    Task<BirdeyeResponse> FetchBirdeyeTransactionDataAsync(string walletAddress);
}