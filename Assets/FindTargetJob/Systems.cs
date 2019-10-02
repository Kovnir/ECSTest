using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FindTarget
{
    [UsedImplicitly]
    public class FindTargetSystems : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
        protected override void OnCreate()
        {
            endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityQuery eq = GetEntityQuery(typeof(TargetTag), ComponentType.ReadOnly<Translation>());
            NativeArray<Entity> entities = eq.ToEntityArray(Allocator.TempJob);
            NativeArray<Translation> translations = eq.ToComponentDataArray<Translation>(Allocator.TempJob);
            
            NativeArray<EntityWithPosition> targetEntitysWithPosition = new NativeArray<EntityWithPosition>(entities.Length, Allocator.TempJob);
            for (int i = 0; i < entities.Length; i++)
            {
                targetEntitysWithPosition[i] = new EntityWithPosition()
                {
                    Entity = entities[i],
                    Position = translations[i].Value
                };
            }

            entities.Dispose();
            translations.Dispose();

            NativeArray<Entity> foundTargets = new NativeArray<Entity>(
                GetEntityQuery(typeof(NPCTag), ComponentType.Exclude<HasTarget>()).CalculateEntityCount(),
                Allocator.TempJob);
            
            FindTargetJob findTargetJob = new FindTargetJob()
            {
                TargetArray = targetEntitysWithPosition,
                SelectedTargets = foundTargets
            };
            JobHandle findTargetJobHandle = findTargetJob.Schedule(this, inputDeps);

            SetTargetJob setTargetJob = new SetTargetJob()
            {
                EntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                TargetArray = foundTargets
            };
            JobHandle setTargetJobHandle = setTargetJob.Schedule(this, findTargetJobHandle);
            
            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(setTargetJobHandle);
            return setTargetJobHandle;
        }

        public struct EntityWithPosition
        {
            public Entity Entity;
            public float3 Position;
        }

        [RequireComponentTag(typeof(NPCTag))]
        [ExcludeComponent(typeof(HasTarget))]
        [BurstCompile]
        private struct FindTargetJob : IJobForEachWithEntity<Translation>
        {
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<EntityWithPosition> TargetArray;
            public NativeArray<Entity> SelectedTargets;

            public void Execute(Entity npc, int index, [ReadOnly] ref Translation translation)
            {
                float3 npcPos = translation.Value;

                float minDistance = float.MaxValue;
                Entity target = Entity.Null;

                for (int i = 0; i < TargetArray.Length; i++)
                {
                    EntityWithPosition ent = TargetArray[i];
                    float distance = math.length(npcPos - ent.Position);
                    if (distance < minDistance)
                    {
                        target = ent.Entity;
                        minDistance = distance;
                    }
                }

                SelectedTargets[index] = target;
            }
        }

        [RequireComponentTag(typeof(NPCTag))]
        [ExcludeComponent(typeof(HasTarget))]
        private struct SetTargetJob : IJobForEachWithEntity<Translation>
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> TargetArray;

            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity npc, int index, [ReadOnly] ref Translation translation)
            {
                var target = TargetArray[index];
                if (target != Entity.Null)
                {
                    EntityCommandBuffer.AddComponent(index, npc, new HasTarget
                    {
                        Target = target
                    });
                }
            }
        }
    }

    [UsedImplicitly]
    public class GoToTargetSystems : ComponentSystem
    {
        protected override void OnUpdate()
        {
            float time = Time.deltaTime;
            var em = World.Active.EntityManager;
            Entities.WithAll<NPCTag>().ForEach((Entity npc, ref Translation tr, ref HasTarget hasTarget, ref SpeedComponent speed) =>
            {
                if (!em.Exists(hasTarget.Target))
                {
                    PostUpdateCommands.RemoveComponent(npc, typeof(HasTarget));   
                    return;
                }
                float3 targetPosition =
                    em.GetComponentData<Translation>(hasTarget.Target).Value;
                
                float3 direction = targetPosition - tr.Value;
                float3 move = math.normalize(direction) * speed.Speed * time;
                if (math.length(move) >= math.length(direction))
                {
                    tr.Value = targetPosition;
                    PostUpdateCommands.RemoveComponent(npc, typeof(HasTarget));
                    PostUpdateCommands.DestroyEntity(hasTarget.Target);
                }
                else
                {
                    tr.Value += move;
                }
            });
        }
    }
    
    public struct HasTarget : IComponentData
    {
        public Entity Target;
    }
    public struct SpeedComponent : IComponentData
    {
        public float Speed;
    }
}