using BusinessLogic.Services;
using BusinessLogic.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BusinessLogic;

public static class Extension
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ISolanaTrackerService, SolanaTrackerService>(client =>
        {
            client.BaseAddress = new Uri(configuration["SolanaTracker:BaseUrl"]);
            var apiKey = configuration["SolanaTracker:ApiKey"];
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        });
        
        services.AddHttpClient<IBirdeyeService, BirdeyeService>(client =>
        {
            client.BaseAddress = new Uri(configuration["Birdeye:BaseUrl"]);
            var apiKey = configuration["Birdeye:ApiKey"];
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("x-chain","solana");
        });
        services.AddHttpClient<IOpenAiService, OpenAiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com");
            var apiKey = configuration["Gpt:ApiKey"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.Timeout = TimeSpan.FromSeconds(90);
        });
        
        services.AddScoped<IWalletAggregatorService, WalletAggregatorService>();
        
        return services;
    }
}