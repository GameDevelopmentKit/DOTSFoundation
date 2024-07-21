namespace TaskModule.Authoring
{
    using TaskModule.Actions;
    using Unity.Entities;
    using UnityEngine;

    public class SetActiveOnTaskCompletedAuthoring : MonoBehaviour
    {
        public GameObject TargetObject;
        public bool Value;
        public class Baker : Baker<SetActiveOnTaskCompletedAuthoring>
        {
            public override void Bake(SetActiveOnTaskCompletedAuthoring authoring)
            {
                var entity = this.GetEntity(TransformUsageFlags.Dynamic);
                var targetEntity = this.GetEntity(authoring.TargetObject, TransformUsageFlags.Dynamic);
 
                this.AddComponent(entity, new SetActiveOnTaskCompleted()
                {
                    TargetEntity = targetEntity,
                    Value = authoring.Value
                });
            }
        }
    }
}