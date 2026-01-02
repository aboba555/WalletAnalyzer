using BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController(IWalletAggregatorService walletAggregatorService, IOpenAiService openAiService) : ControllerBase
{
    [HttpGet("{walletAddress}")]
    public async Task<IActionResult> GetRawWalletAnalysis([FromRoute] string walletAddress)
    {
        try
        {
            var wallet = await walletAggregatorService.GetWalletInfoAsync(walletAddress);
            return Ok(wallet);
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpGet("{walletAddress}/analysis")]
    public async Task<IActionResult> GetFullWalletAnalysis([FromRoute] string walletAddress)
    {
        try
        {
            var wallet = await walletAggregatorService.GetWalletInfoAsync(walletAddress);
            var analysis = await openAiService.AnalyzeWalletAsync(wallet);
            
            return Ok(new {wallet, analysis});
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}