namespace GASCore.Systems.StatSystems.Components
{
    using Unity.Collections;

    //Common statistics
    public partial struct StatName
    {
        #region Offensive

        public static readonly FixedString64Bytes AttackDamage         = "AttackDamage";
        public static readonly FixedString64Bytes AttackSpeed          = "AttackSpeed";
        public static readonly FixedString64Bytes CriticalStrikeChance = "CriticalStrikeChance";
        public static readonly FixedString64Bytes CriticalStrikeDamage = "CriticalStrikeDamage";

        public static readonly FixedString64Bytes Damage = "Damage";

        #endregion

        #region Defensive

        public static readonly FixedString64Bytes Armor              = "Armor";
        public static readonly FixedString64Bytes Health             = "Health";
        public static readonly FixedString64Bytes HealthRegeneration = "HealthRegeneration";

        #endregion

        #region Utility

        #endregion

        #region Other

        public static readonly FixedString64Bytes MovementSpeed = "MovementSpeed";
        public static readonly FixedString64Bytes RotateSpeed   = "RotateSpeed";
        public static readonly FixedString64Bytes AttackRange   = "AttackRange";

        public static readonly FixedString64Bytes Coin = "Coin";

        #endregion
    }
}