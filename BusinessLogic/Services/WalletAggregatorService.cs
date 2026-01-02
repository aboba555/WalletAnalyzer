using DataManagement.Models;

namespace BusinessLogic.Services;

public class WalletAggregatorService(ISolanaTrackerService solanaTrackerService, IBirdeyeService birdeyeService) : IWalletAggregatorService
{
    public async Task<Wallet> GetWalletInfoAsync(string walletAddress)
    {
        var solanaTrackerData = await solanaTrackerService.FetchWalletDataAsync(walletAddress);
        var birdeyeData = await birdeyeService.FetchBirdeyeTransactionDataAsync(walletAddress);

        Wallet result = new Wallet
        {
            WalletAddress = walletAddress,
            Tokens = solanaTrackerData.Tokens.Select(t => new Token
            {
                Address = t.Address,
                Balance = t.Balance,
                ValueUsd = t.Value,
                PriceUsd = t.Price.Usd,
                MarketCapUsd = t.MarketCap.Usd,
                LiquidityUsd = t.Liquidity.Usd
            }).ToList(),
            Transactions = birdeyeData.Data.Solana.Select(t => new CryptoTransaction
            {
                TxHash = t.TxHash,
                FromAddress = t.From,
                ToAddress = t.To,
                BlockTime = DateTime.Parse(t.BlockTime),
                Status = t.Status,
                Fee = t.Fee,
                Action = t.MainAction,
                BalanceChange = t.BalanceChange.Select(b => new BalanceChange
                {
                    Amount = b.Amount,
                    Symbol = b.Symbol,
                    Name = b.Name,
                    Decimals = b.Decimals,
                    Address = b.Address,
                    LogoUri = b.LogoUri
                }).ToList()
            }),
            TotalValueUsd = solanaTrackerData.Total
        };
        
        return result;
    }
}