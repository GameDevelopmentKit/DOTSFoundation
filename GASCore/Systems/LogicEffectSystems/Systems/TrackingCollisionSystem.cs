// namespace GASCore.Systems.LogicEffectSystems.Systems
// {
//     using GASCore.Systems.LogicEffectSystems.Components;
//     using GASCore.Systems.TimelineSystems.Components;
//     using Unity.Burst;
//     using Unity.Collections;
//     using Unity.Entities;
//     using Unity.Physics;
//     using Unity.Physics.Systems;
//     using UnityEngine;
//
//     [UpdateInGroup(typeof(PhysicsSystemGroup))]
//     [UpdateAfter(typeof(PhysicsSimulationGroup))]
//     [BurstCompile]
//     public partial struct TrackingCollisionSystem : ISystem
//     {
//         ComponentLookup<AbilityEffectId> abilityActionLookup;
//         BufferLookup<OnHitTargetElement> onHitLookup;
//
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//             this.abilityActionLookup = state.GetComponentLookup<AbilityEffectId>(true);
//             this.onHitLookup         = state.GetBufferLookup<OnHitTargetElement>();
//         }
//
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state) { }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             this.abilityActionLookup.Update(ref state);
//             this.onHitLookup.Update(ref state);
//             var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
//             var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
//             state.Dependency = new TrackingCollisionJob()
//             {
//                 Ecb                 = ecb,
//                 AbilityActionLookup = this.abilityActionLookup,
//                 OnHitLookup         = this.onHitLookup
//             }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
//         }
//     }
//
//     [BurstCompile]
//     struct TrackingCollisionJob : ITriggerEventsJob
//     {
//         public            EntityCommandBuffer              Ecb;
//         [ReadOnly] public ComponentLookup<AbilityEffectId> AbilityActionLookup;
//         public            BufferLookup<OnHitTargetElement> OnHitLookup;
//         public void Execute(TriggerEvent collisionEvent)
//         {
//             Entity entityHit   = collisionEvent.EntityA;
//             Entity entityIsHit = collisionEvent.EntityB;
//
//             if (this.AbilityActionLookup.HasComponent(entityIsHit))
//             {
//                 (entityIsHit, entityHit) = (entityHit, entityIsHit);
//             }
//
//             if (!this.OnHitLookup.TryGetBuffer(entityHit, out var onHitBuffer))
//             {
//                 onHitBuffer = this.Ecb.AddBuffer<OnHitTargetElement>(entityHit);
//             }
//
//             Debug.Log($"TrackingCollisionJob - entityHit {entityHit.Index} - entityIsHit {entityIsHit.Index}, on hit count = {onHitBuffer.Length}");
//             onHitBuffer.Add(new OnHitTargetElement()
//             {
//                 Target = entityIsHit
//             });
//         }
//     }
// }