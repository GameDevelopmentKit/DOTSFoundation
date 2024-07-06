namespace Transactions.PaymentService
{
    using System;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;
    using Wallet.Manager;

    public class WalletPaymentService : IPaymentService
    {
        private readonly IWalletManager walletManager;
        public           PaymentType    PaymentType                              => PaymentType.Currency;
        public           bool           Available(string assetId)                { return this.walletManager.CanPay(assetId, 1); }
        public           bool           VerifyCost(string assetId, float value)  { return this.walletManager.CanPay(assetId, (int)value); }
        public           UniTask<float> MakePayment(string assetId, float value) { return UniTask.FromResult((float)this.walletManager.TryPay(assetId, (int)value)); }

        public WalletPaymentService(IWalletManager walletManager) { this.walletManager = walletManager; }
    }

    public class InsufficientAssetException : Exception
    {
        public InsufficientAssetException(string message) : base(message) { }
    }
}