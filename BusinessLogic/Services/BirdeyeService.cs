using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogic.DTO.Birdeye;

namespace BusinessLogic.Services;

public class BirdeyeService(HttpClient httpClient) : IBirdeyeService
{
    public async Task<BirdeyeResponse> FetchBirdeyeTransactionDataAsync(string walletAddress)
    {
        string url = $"/v1/wallet/tx_list?wallet={walletAddress}&limit=100";

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<BirdeyeResponse>(options);
        return result;
    }
}