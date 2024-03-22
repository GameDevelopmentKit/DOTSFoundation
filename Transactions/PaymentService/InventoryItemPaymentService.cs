namespace Transactions.PaymentService
{
    using System;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;

    public class InventoryItemPaymentService : IPaymentService
    {
        public PaymentType PaymentType                        => PaymentType.Item;
        public bool        Available()                        { return false; }
        public bool        VerifyCost(CostRecord costRecord)  { throw new NotImplementedException(); }
        public UniTask     MakePayment(CostRecord costRecord) { throw new NotImplementedException(); }
    }
}