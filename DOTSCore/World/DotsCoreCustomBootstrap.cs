namespace DOTSCore.World
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DOTSCore.EntityFactory;
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
        public           World                  WorldInstance { get; set; }
        private readonly GameStateEntityFactory gameStateEntityFactory;

        public GameWorldController(GameStateEntityFactory gameStateEntityFactory) { this.gameStateEntityFactory = gameStateEntityFactory; }

        public virtual void Initialize(string worldName, string gameState, bool isDefault = true, bool filterSystemsRunInWorld = false)
        {
            this.WorldInstance = new World(worldName, WorldFlags.Game);

            if (isDefault)
            {
                World.DefaultGameObjectInjectionWorld = this.WorldInstance;
            }

            //Init game state entity
            this.gameStateEntityFactory.CreateEntity(this.WorldInstance.EntityManager, gameState);

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(this.WorldInstance, this.GetAllSystems());
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(this.WorldInstance);
        }

        protected virtual IReadOnlyList<Type> GetAllSystems()
        {
            var allDefaultSystem = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            var result           = new List<Type>();
            var worldName        = this.WorldInstance.Name;
            foreach (var systemType in allDefaultSystem)
            {
                if (Attribute.IsDefined(systemType, typeof(CreateSystemInWorldAttribute), true))
                {
                    if (worldName == systemType.GetCustomAttribute<CreateSystemInWorldAttribute>(true).WorldName)
                    {
                        result.Add(systemType);
                    }
                }
                else
                {
                    result.Add(systemType);
                }
            }

            return result;
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