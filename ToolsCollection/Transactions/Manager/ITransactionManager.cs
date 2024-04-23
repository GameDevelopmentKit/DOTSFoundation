namespace Transactions.Manager
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;
    using Transactions.Blueprint;
    using Transactions.Model;

    public interface ITransactionManager
    {
        /// <summary>
        ///    Begins a transaction with the specified <see cref="transactionId"/>.
        /// </summary>
        /// <param name="transactionId"></param>
        public UniTask<TransactionResult> BeginTransaction(string transactionId);

        /// <summary>
        ///   Begins a transaction with the specified <see cref="transactionRecord"/>.
        /// </summary>
        /// <param name="transactionRecord"></param>
        public UniTask<TransactionResult> BeginTransaction(TransactionRecord transactionRecord);

        public List<PayoutRecord> GetAllPayouts(string transactionId);
        public UniTask ReceivePayouts(TransactionResult transactionResult);
    }
}