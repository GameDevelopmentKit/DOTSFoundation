namespace TaskModule.Authoring
{
    using TaskModule.Actions;
    using Unity.Entities;
    using UnityEngine;

    public class OnTaskCompletedActionAuthoring : MonoBehaviour
    {
        public GameObject ActionPrefab;

        public class Baker : Baker<OnTaskCompletedActionAuthoring>
        {
            public override void Bake(OnTaskCompletedActionAuthoring authoring)
            {
                this.AddBuffer<OnTaskCompletedAction>(this.GetEntity(TransformUsageFlags.Dynamic)).Add(new OnTaskCompletedAction()
                    { ActionEntityPrefab = this.GetEntity(authoring.ActionPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}