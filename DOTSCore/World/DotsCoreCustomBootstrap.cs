namespace DOTSCore.World
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Unity.Entities;

    public class DefaultCustomBootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
#if TEST_DOTS
            World.DefaultGameObjectInjectionWorld = new World(defaultWorldName, WorldFlags.Game);
            var allSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            var filteredSystems = allSystems.Where(type =>
            {
                var assemblyFullName = type.Namespace ?? type.Assembly.FullName;
                return !assemblyFullName.Contains("Gameplay.") && !assemblyFullName.Contains("DOTSCore") &&
                       !assemblyFullName.Contains("GASCore");
            });

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld,
                filteredSystems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
#else
            World.DefaultGameObjectInjectionWorld = new World(defaultWorldName, WorldFlags.None);
#endif
            return true;
        }
    }


    public class GameWorldController
    {
        public World WorldInstance { get; set; }

        public virtual void Initialize(string worldName, bool isDefault = true, bool customFilterSystems = true)
        {
            this.WorldInstance = new World(worldName, WorldFlags.Game);

            if (isDefault)
            {
                World.DefaultGameObjectInjectionWorld = this.WorldInstance;
            }

            var allSystems = customFilterSystems ? this.GetAllSystems() : DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(this.WorldInstance, allSystems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(this.WorldInstance);
        }

        protected virtual IReadOnlyList<Type> GetAllSystems()
        {
            var allDefaultSystem = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            var worldName        = this.WorldInstance.Name;


            // filter system can create in world
            return allDefaultSystem.Where(type =>
                CanCreateInWorld(Attribute.IsDefined(type, typeof(UpdateInGroupAttribute), true) ? type.GetCustomAttribute<UpdateInGroupAttribute>(true).GroupType : type)).ToList();

            bool CanCreateInWorld(MemberInfo systemType)
            {
                if (!Attribute.IsDefined(systemType, typeof(CreateSystemInWorldAttribute), true)) return true;
                return worldName == systemType.GetCustomAttribute<CreateSystemInWorldAttribute>(true).WorldName;
            }
        }

        public void Cleanup()
        {
            if (this.WorldInstance != null && this.WorldInstance.IsCreated)
            {
                //This query deletes all entities
                this.WorldInstance.EntityManager.DestroyEntity(this.WorldInstance.EntityManager.UniversalQuery);
                this.WorldInstance.Dispose();
            }
        }
    }


    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class CreateSystemInWorldAttribute : Attribute
    {
        /// <summary>
        /// The World the system belongs in.
        /// </summary>
        public string WorldName;

        /// <summary></summary>
        /// <param name="worldName">Defines where systems should be created.</param>
        public CreateSystemInWorldAttribute(string worldName) { this.WorldName = worldName; }
    }
}