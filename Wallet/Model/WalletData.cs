namespace Wallet.Model
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Interfaces;
    using UserData;

    /// <summary>
    ///     Serializable data structure that contains the state of the Wallet.
    /// </summary>
    [Serializable]
    public class WalletData : IUserData, ILocalData
    {
        public readonly Dictionary<string, Currency> Balances = new();
    }
}
