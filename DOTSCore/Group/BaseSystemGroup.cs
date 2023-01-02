namespace DOTSCore.Group
{
    using DOTSCore.World;
    using Unity.Entities;

    public abstract class BaseSystemGroup : ComponentSystemGroup
    {
        public new BaseWorld World { get; private set; }
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        protected override void OnCreate()
        {
            base.OnCreate();
            if (base.World is DOTSCore.World.BaseWorld baseWorld)
            {
                this.World = baseWorld;
                this.SetEnableSystemSorting(!baseWorld.UseExplicitSystemOrdering);
            }
            this.CreateSystems();
           
        }
#endif

        public void SetEnableSystemSorting(bool value)
        {
            this.EnableSystemSorting = value;
        }
        
        /// <summary>
        /// Define the systems to be added to this SystemGroup using<see cref="DOTSCore.Extension.DotsExtension.AddSystem"/>> 
        /// or <see cref="DOTSCore.Extension.DotsExtension.AddUnmanagedSystem"/>> . 
        /// </summary>
        protected abstract void CreateSystems();
    }
}