namespace Transactions.Manager
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;
    using Transactions.Blueprint;
    using Transactions.Model;

    public interface ITransactionManager
    {
        /// <summary>
        ///   Begins a transaction with the specified <see cref="transactionRecord"/>.
        /// </summary>
        /// <param name="transactionRecord"></param>
        UniTask<TransactionResult> BeginTransaction(TransactionRecord transactionRecord);


        /// <summary>
        ///  Begins a transaction with the specified <see cref="costRecords"/> and <see cref="payoutRecords"/>.
        /// </summary>
        /// <param name="costRecords"></param>
        /// <param name="payoutRecords"></param>
        /// <returns></returns>
        UniTask<TransactionResult> BeginTransaction(IReadOnlyCollection<CostRecord> costRecords, IReadOnlyCollection<PayoutRecordAbstract> payoutRecords);

        /// <summary>
        /// Used for installment payments
        /// </summary>
        /// <param name="paymentProgresses"></param>
        /// <param name="payoutRecords"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        UniTask<TransactionResult> BeginTransaction(IReadOnlyCollection<PaymentProgress> paymentProgresses, IReadOnlyCollection<PayoutRecordAbstract> payoutRecords);

        /// <summary>
        ///  Verifies the costs with the specified <see cref="costRecords"/>.
        /// </summary>
        /// <param name="costRecords"></param>
        /// <returns></returns>
        bool VerifyCosts(IReadOnlyCollection<CostRecord> costRecords);
        
        /// <summary>
        ///  Makes payments with the specified <see cref="costRecords"/>.
        /// </summary>
        /// <param name="costRecords"></param>
        /// <returns></returns>
        UniTask MakePayments(IReadOnlyCollection<CostRecord> costRecords);

        /// <summary>
        /// Makes payments with the specified <see cref="paymentProgresses"/>.
        /// </summary>
        /// <param name="paymentProgresses"></param>
        /// <returns></returns>
        UniTask<bool> MakePayments(IReadOnlyCollection<PaymentProgress> paymentProgresses);

        /// <summary>
        /// Receives the payouts from the specified <see cref="transactionResult"/>.
        /// </summary>
        /// <param name="transactionResult"></param>
        /// <returns></returns>
        UniTask ReceivePayouts(TransactionResult transactionResult);
        
        UniTask ReceivePayout(Asset asset);
    }
}