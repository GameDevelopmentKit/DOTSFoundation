using System.Linq;

namespace DOTSCore.World
{
    using System;
    using DOTSCore.EntityFactory;
    using Unity.Entities;

    public class DefaultCustomBootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            World.DefaultGameObjectInjectionWorld = new World(defaultWorldName, WorldFlags.None);
#if TEST_DOTS
              var allSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            var filteredSystems = allSystems.Where(type =>
            {
                var assemblyFullName = type.Namespace ?? type.Assembly.FullName;
                return !assemblyFullName.Contains("Gameplay") && !assemblyFullName.Contains("DOTSCore") &&
                       !assemblyFullName.Contains("GASCore");
            });
            
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld,
                filteredSystems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
#endif
            return true;
        }
    }


    public class GameWorldController
    {
        public World WorldInstance { get; set; }
        private readonly GameStateEntityFactory gameStateEntityFactory;

        public GameWorldController(GameStateEntityFactory gameStateEntityFactory)
        {
            this.gameStateEntityFactory = gameStateEntityFactory;
        }

        public virtual void Initialize(string worldName, string gameState, bool isDefault = true)
        {
            this.WorldInstance = new World(worldName, WorldFlags.Game);

            if (isDefault)
            {
                World.DefaultGameObjectInjectionWorld = this.WorldInstance;
            }

            //Init game state entity
            this.gameStateEntityFactory.CreateEntity(this.WorldInstance.EntityManager, gameState);

            var allSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(this.WorldInstance, allSystems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(this.WorldInstance);
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
}