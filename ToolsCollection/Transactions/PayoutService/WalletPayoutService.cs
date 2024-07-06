namespace Transactions.PayoutService
{
    using Cysharp.Threading.Tasks;
    using Transactions.Model;
    using Wallet.Manager;

    public class WalletPayoutService : IPayoutService
    {
        readonly IWalletManager walletManager;
        public   string         AssetType => Blueprint.AssetDefaultType.Currency;

        public WalletPayoutService(IWalletManager walletManager) { this.walletManager = walletManager; }

        public UniTask ReceivePayout(Asset asset)
        {
            this.walletManager.Add(asset.AssetId, asset.Amount);
            return UniTask.CompletedTask;
        }
    }
}