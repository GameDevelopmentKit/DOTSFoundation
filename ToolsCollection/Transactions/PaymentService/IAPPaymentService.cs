namespace Transactions.PaymentService
{
    using System;
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;

    public class IAPPaymentService : IPaymentService
    {
        public PaymentType PaymentType                                                                                  => PaymentType.IAP;
        public bool        Available()                                                                                  { return false; }
        public bool        VerifyCost(CostRecord costRecord)                                                            { throw new NotImplementedException(); }
        public UniTask     MakePayment(CostRecord costRecord)                                                           { throw new NotImplementedException(); }
    }
}