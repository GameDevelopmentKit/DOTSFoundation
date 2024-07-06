namespace Wallet.Manager
{
    using System.Collections.Generic;
    using DataManager.MasterData;
    using DataManager.UserData;
    using Wallet.Blueprint;
    using Wallet.Model;

    public class WalletManager : BaseDataManager<WalletData>, IWalletManager
    {
        public Currency                      Get(string currencyId)           { return this.Data.Balances.GetValueOrDefault(currencyId); }
        public ResourceRecord                GetStaticData(string currencyId) { return this.resourceBlueprint.GetDataById(currencyId); }
        public IReadOnlyCollection<Currency> GetAllBalances()                 { return this.Data.Balances.Values; }

        public bool CanPay(string currencyId, int value) { return this.Data.Balances.ContainsKey(currencyId) && this.Data.Balances[currencyId].HasValue(value); }

        public void Add(string currencyId, int value)
        {
            if (!this.Data.Balances.ContainsKey(currencyId))
            {
                this.Data.Balances.Add(currencyId, new Currency(currencyId, value) { StaticData = this.resourceBlueprint.GetDataById(currencyId) });
            }

            this.Data.Balances[currencyId].Add(value);
        }

        public bool Pay(string currencyId, int value) { return this.CanPay(currencyId, value) && this.Data.Balances[currencyId].Remove(value); }

        public int TryPay(string currencyId, int value)
        {
            if (!this.Data.Balances.ContainsKey(currencyId))
                return value;

            var currency = this.Data.Balances[currencyId];
            if (currency.Value >= value)
            {
                currency.Remove(value);
                return 0;
            }

            var remain = value - currency.Value;
            currency.Remove(currency.Value);
            return remain;
        }


        #region Inject

        private readonly ResourceBlueprint resourceBlueprint;

        public WalletManager(MasterDataManager masterDataManager, ResourceBlueprint resourceBlueprint) : base(masterDataManager) { this.resourceBlueprint = resourceBlueprint; }

        protected override void OnDataLoaded()
        {
            //Init wallet
            foreach (var resource in this.resourceBlueprint)
            {
                if (!this.Data.Balances.ContainsKey(resource.Key) && (resource.Value.IsDefault || resource.Value.DefaultValue > 0))
                    this.Data.Balances.Add(resource.Key, new Currency(resource.Value.Id, resource.Value.DefaultValue));
            }


            //Init static data
            var balances = this.Data.Balances;
            foreach (var currency in balances)
            {
                currency.Value.StaticData = this.resourceBlueprint.GetDataById(currency.Key);
            }
        }

        #endregion
    }
}