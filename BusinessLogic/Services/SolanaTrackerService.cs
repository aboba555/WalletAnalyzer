using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogic.DTO.SolanaTracker;
using BusinessLogic.Settings;
using Microsoft.Extensions.Options;
namespace BusinessLogic.Services;

public class SolanaTrackerService(HttpClient httpClient) : ISolanaTrackerService
{
    public async Task<SolanaTrackerResponse> FetchWalletDataAsync(string walletAddress)
    {
        string url = "/wallet/" + walletAddress + "/basic";
        
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<SolanaTrackerResponse>(options);
        
        return result;
    }
}