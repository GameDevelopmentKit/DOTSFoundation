namespace Wallet.Manager
{
    using System.Collections.Generic;
    using DataManager.MasterData;
    using DataManager.UserData;
    using Wallet.Blueprint;
    using Wallet.Model;

    public class WalletManager : BaseDataManager<WalletData>, IWalletManager
    {
        public Currency Get(string currencyId) { return this.Data.Balances.GetValueOrDefault(currencyId); }

        public bool CanPay(string currencyId, int balance) { return this.Data.Balances.ContainsKey(currencyId) && this.Data.Balances[currencyId].HasValue(balance); }

        public void Add(string currencyId, int balance)
        {
            if (!this.Data.Balances.ContainsKey(currencyId))
            {
                this.Data.Balances.Add(currencyId, new Currency(currencyId, balance) { StaticData = this.resourceBlueprint.GetDataById(currencyId) });
            }

            this.Data.Balances[currencyId].Add(balance);
        }

        public bool Pay(string currencyId, int balance) { return this.CanPay(currencyId, balance) && this.Data.Balances[currencyId].Remove(balance); }


        #region Inject

        private readonly ResourceBlueprint resourceBlueprint;

        public WalletManager(MasterDataManager masterDataManager, ResourceBlueprint resourceBlueprint) : base(masterDataManager) { this.resourceBlueprint = resourceBlueprint; }

        protected override void OnDataLoaded(MasterDataManager masterDataManager)
        {
            //Init wallet
            foreach (var resource in this.resourceBlueprint)
            {
                if (!this.Data.Balances.ContainsKey(resource.Key) && resource.Value.DefaultValue > 0)
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