namespace GASCore.UnityHybrid.Baker
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    public class ObjectTag : MonoBehaviour
    {
        public string TagName;
    }
    
    public class ObjectTagBaker : Baker<ObjectTag>
    {
        public override void Bake(ObjectTag authoring)
        {
            AddComponent(this.GetEntity(TransformUsageFlags.Dynamic),new TagComponent(){Value = authoring.TagName});
        }
    }
}