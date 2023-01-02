namespace DOTSCore.UnityPhysicExtension.Utils
{
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;
    using Unity.Physics.Authoring;
    using Unity.Physics.GraphicsIntegration;

    public static class PhysicExtension
    {
        public static void AddBoxPhysicsCollider(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, float3 size, float3 center, CollisionResponsePolicy collisionResponse,
            PhysicsCategoryTags colliderBelongsTo, PhysicsCategoryTags colliderCollidesWith)
        {
            var colliderFilter = CollisionFilter.Default;
            colliderFilter.BelongsTo    = colliderBelongsTo.Value;
            colliderFilter.CollidesWith = colliderCollidesWith.Value;

            var colliderMaterial = Material.Default;
            colliderMaterial.CollisionResponse = collisionResponse;

            var colliderBlob = BoxCollider.Create(new BoxGeometry
            {
                BevelRadius = 0.05f,
                Center      = center,
                Orientation = quaternion.identity,
                Size        = size
            }, colliderFilter, colliderMaterial);

            ecb.AddComponent(index, entity, new PhysicsCollider { Value = colliderBlob });
            ecb.AddSharedComponent(index, entity, new PhysicsWorldIndex());
        }
        
        public static void AddSpherePhysicsCollider(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, float radius, float3 center, CollisionResponsePolicy collisionResponse,
            PhysicsCategoryTags colliderBelongsTo, PhysicsCategoryTags colliderCollidesWith)
        {
            var colliderFilter = CollisionFilter.Default;
            colliderFilter.BelongsTo    = colliderBelongsTo.Value;
            colliderFilter.CollidesWith = colliderCollidesWith.Value;

            var colliderMaterial = Material.Default;
            colliderMaterial.CollisionResponse = collisionResponse;

            var colliderBlob = SphereCollider.Create(new SphereGeometry()
            {
                Center = center,
                Radius = radius
            }, colliderFilter, colliderMaterial);
            
            ecb.AddComponent(index, entity, new PhysicsCollider { Value = colliderBlob });
            ecb.AddSharedComponent(index, entity, new PhysicsWorldIndex());
        }

        public static void AddPhysicBody(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, PhysicBodyData physicBodyData)
        {
            ecb.AddSharedComponent(index, entity, new PhysicsWorldIndex(physicBodyData.WorldIndex));

            var customTags = physicBodyData.CustomTags;
            if (!customTags.Equals(CustomPhysicsBodyTags.Nothing))
                ecb.AddComponent(index, entity, new PhysicsCustomTags { Value = customTags.Value });

            if (physicBodyData.MotionType == BodyMotionType.Static)
                return;

            var massProperties = MassProperties.UnitSphere;

            ecb.AddComponent(index, entity,
                physicBodyData.MotionType == BodyMotionType.Dynamic ? PhysicsMass.CreateDynamic(massProperties, physicBodyData.Mass) : PhysicsMass.CreateKinematic(massProperties));

            var physicsVelocity = new PhysicsVelocity
            {
                Linear  = physicBodyData.InitialLinearVelocity,
                Angular = physicBodyData.InitialAngularVelocity
            };
            ecb.AddComponent(index, entity, physicsVelocity);

            if (physicBodyData.MotionType == BodyMotionType.Dynamic)
            {
                ecb.AddComponent(index, entity, new PhysicsDamping
                {
                    Linear  = physicBodyData.LinearDamping,
                    Angular = physicBodyData.AngularDamping
                });
                if (physicBodyData.GravityFactor != 1)
                {
                    ecb.AddComponent(index, entity, new PhysicsGravityFactor
                    {
                        Value = physicBodyData.GravityFactor
                    });
                }
            }
            else if (physicBodyData.MotionType == BodyMotionType.Kinematic)
            {
                ecb.AddComponent(index, entity, new PhysicsGravityFactor
                {
                    Value = 0
                });
            }

            if (physicBodyData.Smoothing != BodySmoothing.None)
            {
                ecb.AddComponent(index, entity, new PhysicsGraphicalSmoothing());
                if (physicBodyData.Smoothing == BodySmoothing.Interpolation)
                {
                    ecb.AddComponent(index, entity, new PhysicsGraphicalInterpolationBuffer
                    {
                        PreviousTransform = Math.DecomposeRigidBodyTransform(float4x4.identity),
                        PreviousVelocity  = physicsVelocity,
                    });
                }
            }
        }
    }

    public class PhysicBodyData
    {
        public BodyMotionType MotionType = BodyMotionType.Kinematic;

        public bool             OverrideDefaultMassDistribution = false;
        public MassDistribution CustomMassDistribution;

        public float3 InitialLinearVelocity  = float3.zero;
        public float3 InitialAngularVelocity = float3.zero;

        public float         LinearDamping  = 0.01f;
        public float         AngularDamping = 0.05f;
        public BodySmoothing Smoothing      = BodySmoothing.None;

        public uint                  WorldIndex = 0;
        public CustomPhysicsBodyTags CustomTags = CustomPhysicsBodyTags.Nothing;
        
        public float Mass
        {
            get => this.MotionType == BodyMotionType.Dynamic ? this.m_mass : float.PositiveInfinity;
            set => this.m_mass = math.max(0.001f, value);
        }
        private float m_mass = 1.0f;
        
        public float GravityFactor
        {
            get => this.MotionType == BodyMotionType.Dynamic ? this.m_gravityFactor : 0f;
            set => this.m_gravityFactor = value;
        }

        private float m_gravityFactor = 1f;
    }
}