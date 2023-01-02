namespace DOTSCore.World
{
    using System;
    using System.Linq;
    using Unity.Entities;
    using UnityEngine.LowLevel;
    using Zenject;

    [Obsolete]
    public abstract class BaseWorld : World, IInitializable
    {
        /// <summary>
        /// Specifies the default system ordering behavior for any newly created ComponentGroupSystem.
        /// If true, automatic system ordering will by default be disabled for those SuperSystems.
        /// </summary>
        public bool UseExplicitSystemOrdering { get; }

        public BaseWorld(string name, WorldFlags flags = WorldFlags.Simulation, bool useExplicitSystemOrdering = true) : base(name, flags)
        {
            this.UseExplicitSystemOrdering = useExplicitSystemOrdering;
        }

        public void Initialize()
        {
            //Init default system groups
            var allSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(this, allSystems.Where(this.FilterUnitySystemType));

            //Init systems in this world
            this.SetupSystemGroups();

            //Sort systems in root group
            this.GetOrCreateSystemManaged<InitializationSystemGroup>().SortSystems();
            this.GetOrCreateSystemManaged<SimulationSystemGroup>().SortSystems();
            this.GetOrCreateSystemManaged<PresentationSystemGroup>().SortSystems();

            //Reset player loop so we don't infinitely add systems.
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(this);
        }

        /// <summary>
        /// Define the systems group to be added to this World using<see cref="DOTSCore.Extension.DotsExtension.AddSystemGroup"/>>
        /// Any SystemGroup must be inherited from BaseSystemGroup
        /// </summary>
        protected abstract void SetupSystemGroups();

        protected virtual bool FilterUnitySystemType(Type type)
        {
            var chosenString = type.Namespace ?? type.Assembly.FullName;
            return chosenString.Contains("Unity") && !chosenString.Contains("Unity.NetCode");
        }
    }
}