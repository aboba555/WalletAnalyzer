using BusinessLogic.DTO.OpenAI;
using DataManagement.Models;

namespace BusinessLogic.Services;

public interface IOpenAiService
{
    Task<GptResponse> AnalyzeWalletAsync(Wallet wallet);
}