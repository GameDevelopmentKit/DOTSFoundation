namespace Transactions.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;
    using Transactions.Model;
    using Transactions.PaymentService;
    using Transactions.PayoutService;
    using Random = System.Random;

    public class TransactionManager : ITransactionManager
    {
        public UniTask<TransactionResult> BeginTransaction(TransactionRecord transactionRecord) { return this.BeginTransaction(transactionRecord.Costs, transactionRecord.Payouts); }

        public async UniTask<TransactionResult> BeginTransaction(IReadOnlyCollection<CostRecord> costRecords, IReadOnlyCollection<PayoutRecordAbstract> payoutRecords)
        {
            await this.MakePayments(costRecords);

            // Process payout
            return new TransactionResult() { Assets = this.GetPayoutAssets(payoutRecords) };
        }

        public async UniTask<TransactionResult> BeginTransaction(IReadOnlyCollection<PaymentProgress> paymentProgresses, IReadOnlyCollection<PayoutRecordAbstract> payoutRecords)
        {
            var result = await this.MakePayments(paymentProgresses);
            if (!result)
            {
                throw new InsufficientAssetException("Not enough resource to complete transaction");
            }

            // Process payout
            return new TransactionResult() { Assets = this.GetPayoutAssets(payoutRecords) };
        }

        #region Payment

        /// <summary>
        /// // check payment service is exist and available
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="paymentService"></param>
        /// <returns></returns>
        public bool TryGetPaymentService(PaymentType paymentType, out IPaymentService paymentService) { return this.paymentTypeToService.TryGetValue(paymentType, out paymentService); }


        public bool VerifyCosts(IReadOnlyCollection<CostRecord> costRecords)
        {
            foreach (var costRecord in costRecords)
            {
                if (this.TryGetPaymentService(costRecord.PaymentType, out var paymentService) && paymentService.VerifyCost(costRecord.CostAssetId, costRecord.CostAmount)) continue;
                return false;
            }

            return true;
        }

        public async UniTask MakePayments(IReadOnlyCollection<CostRecord> costRecords)
        {
            // Verify all costs
            foreach (var costRecord in costRecords)
            {
                if (!this.TryGetPaymentService(costRecord.PaymentType, out var paymentService))
                {
                    throw new PaymentServiceException($"Payment service {costRecord.PaymentType} is not available");
                }

                if (!paymentService.VerifyCost(costRecord.CostAssetId, costRecord.CostAmount))
                {
                    throw new InsufficientAssetException($"Payment service {costRecord.PaymentType}: not enough {costRecord.CostAssetId}");
                }
            }

            // Make payment and wait for all payment to complete
            var paymentTasks = costRecords.Select(costRecord => this.TryGetPaymentService(costRecord.PaymentType, out var paymentService)
                ? paymentService.MakePayment(costRecord.CostAssetId, costRecord.CostAmount)
                : UniTask.CompletedTask);
            await UniTask.WhenAll(paymentTasks);
        }

        public async UniTask<bool> MakePayments(IReadOnlyCollection<PaymentProgress> paymentProgresses)
        {
            // pay anything service has
            foreach (var paymentProgress in paymentProgresses)
            {
                if (paymentProgress.IsCompleted) continue;

                var paymentType = paymentProgress.CostRecord.PaymentType;
                if (this.TryGetPaymentService(paymentType, out var paymentService) && paymentService.Available(paymentProgress.CostRecord.CostAssetId))
                {
                    var remainingAmount             = paymentProgress.RemainingAmount;
                    var remainingAmountAfterPayment = await paymentService.MakePayment(paymentProgress.CostRecord.CostAssetId, remainingAmount);
                    paymentProgress.RemainingAmount = remainingAmountAfterPayment;
                }
            }

            return paymentProgresses.All(paymentProgress => paymentProgress.IsCompleted);
        }

        #endregion

        #region Payout

        private List<Asset> GetPayoutAssets(IReadOnlyCollection<PayoutRecordAbstract> payouts, int repeat = 1)
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var result = new List<Asset>();
            for (int i = 0; i < repeat; i++)
            {
                result.AddRange(from payoutRecord in payouts
                    where !(payoutRecord.Chance < random.NextDouble())
                    select new Asset()
                    {
                        AssetId   = payoutRecord.PayoutAssetId,
                        Amount    = payoutRecord.GetAmount(),
                        AssetType = payoutRecord.AssetType
                    });
            }

            return result;
        }

        bool TryGetPayoutService(string type, out IPayoutService payoutService) => this.payoutTypeToService.TryGetValue(type, out payoutService);
        public async UniTask ReceivePayouts(TransactionResult transactionResult)
        {
            var payoutTasks = transactionResult.Assets.Select(asset => this.TryGetPayoutService(asset.AssetType, out var payoutService)
                ? payoutService.ReceivePayout(asset)
                : UniTask.CompletedTask);
            await UniTask.WhenAll(payoutTasks);
        }
        public UniTask ReceivePayout(Asset asset) { return this.TryGetPayoutService(asset.AssetType, out var payoutService) ? payoutService.ReceivePayout(asset) : UniTask.CompletedTask; }

        #endregion


        #region Inject

        private Dictionary<PaymentType, IPaymentService> paymentTypeToService;
        private Dictionary<string, IPayoutService>       payoutTypeToService;

        public TransactionManager(List<IPaymentService> paymentServices, List<IPayoutService> payoutServices)
        {
            this.paymentTypeToService = paymentServices.ToDictionary(x => x.PaymentType);
            this.payoutTypeToService  = payoutServices.ToDictionary(x => x.AssetType);
        }

        #endregion
    }

    public class PaymentServiceException : Exception
    {
        public PaymentServiceException(string message) : base(message) { }
    }
}