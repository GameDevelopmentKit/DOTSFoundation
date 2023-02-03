namespace GASCore.UnityHybrid.Baker
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    public class TeamOwner : MonoBehaviour
    {
        public TeamType Team;
    }

    public class TeamOwnerBaker : Baker<TeamOwner>
    {
        public override void Bake(TeamOwner authoring) { this.AddComponent(new TeamOwnerId() { Value = (int)authoring.Team }); }
    }
}