namespace Wallet.Model
{
    using System;
    using System.Collections.Generic;
    using DataManager.LocalData;
    using DataManager.UserData;

    /// <summary>
    ///     Serializable data structure that contains the state of the Wallet.
    /// </summary>
    [Serializable]
    public class WalletData : IUserData, ILocalData
    {
        public readonly Dictionary<string, Currency> Balances = new();
    }
}
