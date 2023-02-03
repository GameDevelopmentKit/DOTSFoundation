namespace DOTSCore.World
{
    using System;
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
        public World WorldInstance { get; set; }

        public virtual void Initialize(string worldName, bool isDefault = true)
        {
            this.WorldInstance = new World(worldName, WorldFlags.Game);

            if (isDefault)
            {
                World.DefaultGameObjectInjectionWorld = this.WorldInstance;
            }

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