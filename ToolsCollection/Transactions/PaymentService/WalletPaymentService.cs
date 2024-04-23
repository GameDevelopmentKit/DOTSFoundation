namespace Transactions.PaymentService
{
    using System;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;
    using Wallet.Manager;

    public class WalletPaymentService : IPaymentService
    {
        private readonly IWalletManager walletManager;
        public PaymentType PaymentType => PaymentType.Currency;

        public WalletPaymentService(IWalletManager walletManager) { this.walletManager = walletManager; }

        public bool Available() => true;
        public bool VerifyCost(CostRecord costRecord) { return this.walletManager.CanPay(costRecord.CostAssetId, (int)costRecord.Amount); }
        public UniTask MakePayment(CostRecord costRecord)
        {
            if (!this.walletManager.Pay(costRecord.CostAssetId, (int)costRecord.Amount))
            {
                throw new InsufficientAssetException($"Not enough {costRecord.CostAssetId}");
            }

            return UniTask.CompletedTask;
        }
    }

    public class InsufficientAssetException : Exception
    {
        public InsufficientAssetException(string message) : base(message) { }
    }
}