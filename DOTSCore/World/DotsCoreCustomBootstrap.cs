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
            return true;
        }

        public static bool FilterUnitySystemType(Type type)
        {
            var chosenString = type.Namespace ?? type.Assembly.FullName;
            return chosenString.Contains("Unity");
        }
    }

    
    public class GameWorldController
    {
        public           World                  WorldInstance { get; set; }
        private readonly GameStateEntityFactory gameStateEntityFactory;

        public GameWorldController(GameStateEntityFactory gameStateEntityFactory) { this.gameStateEntityFactory = gameStateEntityFactory; }

        public virtual void Initialize(string worldName, string gameState ,bool isDefault = true)
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