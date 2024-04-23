namespace Wallet.Manager
{
    using Wallet.Model;

    /// <summary>
    ///     Manages the player currency balances.
    /// </summary>
    public interface IWalletManager
    {
        /// <summary>
        ///     Gets the balance of the specified <see cref="currencyId"/>.
        /// </summary>
        Currency Get(string currencyId);

        /// <summary>
        ///   Checks if the player can pay the specified currency value.
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="balance"></param>
        /// <returns></returns>
        public bool CanPay(string currencyId, int balance);
        
        /// <summary>
        ///     Increases the balance of the specified currency value.
        /// </summary>
        /// <param name="currencyId"> The currency you want to increase the balance. </param>
        /// <param name="balance">The amount to add to the balance. </param>
        void Add(string currencyId, int balance);

        /// <summary>
        ///     Decreases the balance of the specified currency value.
        /// </summary>
        /// <param name="currencyId"> The currency you want to decrease the balance.</param>
        /// <param name="balance"> The amount to remove to the balance. </param>
        /// <returns><c>true</c> if the update is valid, <c>false</c> otherwise.</returns>
        bool Pay(string currencyId, int balance);
    }
}