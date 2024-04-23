namespace Transactions.Manager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;
    using Transactions.Model;
    using Transactions.PaymentService;
    using Transactions.PayoutService;
    using UnityEngine;
    using Random = System.Random;

    public class TransactionManager : ITransactionManager
    {
        public UniTask<TransactionResult> BeginTransaction(string transactionId)
        {
            try
            {
                var transactionRecord = transactionBlueprint.GetDataById(transactionId);
                return this.BeginTransaction(transactionRecord);
            }
            catch (InvalidDataException e)
            {
                Debug.LogException(e);
                return default;
            }
        }
        public async UniTask<TransactionResult> BeginTransaction(TransactionRecord transactionRecord)
        {
            var costRecords = transactionRecord.Costs;
            // Verify all costs
            foreach (var costRecord in costRecords)
            {
                if (!this.TryGetPaymentService(costRecord.PaymentType, out var paymentService))
                {
                    throw new PaymentServiceException($"Payment service {costRecord.PaymentType} is not available");
                }

                if (!paymentService.VerifyCost(costRecord))
                {
                    throw new InsufficientAssetException($"Payment service {costRecord.PaymentType}: not enough {costRecord.CostAssetId}");
                }
            }

            // Make payment and wait for all payment to complete
            var paymentTasks = costRecords.Select(costRecord => this.TryGetPaymentService(costRecord.PaymentType, out var paymentService)
                ? paymentService.MakePayment(costRecord)
                : UniTask.CompletedTask);
            await UniTask.WhenAll(paymentTasks);

            // Process payout
            return new TransactionResult() { Assets = this.GetRandomAssets(transactionRecord.Payouts) };
        }

        /// <summary>
        /// // check payment service is exist and available
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="paymentService"></param>
        /// <returns></returns>
        public bool TryGetPaymentService(PaymentType paymentType, out IPaymentService paymentService) { return this.paymentTypeToService.TryGetValue(paymentType, out paymentService); }

        public List<Asset> GetRandomAssets(List<PayoutRecord> payouts, int repeat = 1)
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
                        Amount    = random.Next(payoutRecord.MinAmount, payoutRecord.MaxAmount),
                        AssetType = payoutRecord.AssetType
                    });
            }

            return result;
        }

        public List<PayoutRecord> GetAllPayouts(string transactionId)
        {
            var transactionRecord = transactionBlueprint.GetDataById(transactionId);
            return transactionRecord.Payouts;
        }

        bool TryGetPayoutService(string type, out IPayoutService payoutService) => this.payoutTypeToService.TryGetValue(type, out payoutService);
        public async UniTask ReceivePayouts(TransactionResult transactionResult)
        {
            var payoutTasks = transactionResult.Assets.Select(asset => this.TryGetPayoutService(asset.AssetType, out var payoutService)
                ? payoutService.ReceivePayout(asset)
                : UniTask.CompletedTask);
            await UniTask.WhenAll(payoutTasks);
        }

        #region Inject

        private readonly TransactionBlueprint                     transactionBlueprint;
        private          Dictionary<PaymentType, IPaymentService> paymentTypeToService;
        private          Dictionary<string, IPayoutService>       payoutTypeToService;

        public TransactionManager(TransactionBlueprint transactionBlueprint, List<IPaymentService> paymentServices, List<IPayoutService> payoutServices)
        {
            this.transactionBlueprint = transactionBlueprint;
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