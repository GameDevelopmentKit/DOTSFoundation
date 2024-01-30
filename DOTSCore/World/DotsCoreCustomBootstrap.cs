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
            List<Type> list = new List<Type>();
            foreach (var type in allDefaultSystem)
            {
                if ((!Attribute.IsDefined(type, typeof(UpdateInGroupAttribute), true) ||
                     type.GetCustomAttributes<UpdateInGroupAttribute>(true).Any(systemType => CanCreateInWorld(systemType.GroupType))) && CanCreateInWorld(type))
                {
                    list.Add(type);
                }
            }

            return list;

            bool CanCreateInWorld(MemberInfo systemType)
            {
                return !Attribute.IsDefined(systemType, typeof(CreateSystemInWorldAttribute), true) ||
                       systemType.GetCustomAttributes<CreateSystemInWorldAttribute>(true).Any(createSystemInWorld => worldName == createSystemInWorld.WorldName);
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


    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
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