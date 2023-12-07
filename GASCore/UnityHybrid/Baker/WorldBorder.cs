namespace GASCore.UnityHybrid.Baker
{
    using Unity.Entities;
    using UnityEngine;

    public struct WorldBorderComponent : IComponentData
    {
        public float Left;
        public float Right;
        public float Back;
        public float Front;
    }

    public class WorldBorder : MonoBehaviour
    {
        [SerializeField] private Transform left;
        [SerializeField] private Transform right;
        [SerializeField] private Transform back;
        [SerializeField] private Transform front;

        public class Baker : Baker<WorldBorder>
        {
            public override void Bake(WorldBorder authoring)
            {
                this.AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new WorldBorderComponent()
                {
                    Left  = authoring.left.position.x + 1,
                    Right = authoring.right.position.x - 1,
                    Back  = authoring.back.position.z + 1,
                    Front = authoring.front.position.z - 1,
                });
            }
        }
    }
}