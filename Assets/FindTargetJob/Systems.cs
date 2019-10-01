using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FindTarget
{
    [UsedImplicitly]
    public class FindTargetSystems : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<NPCTag>().WithNone<HasTarget>().ForEach((Entity npc, ref Translation tr) =>
            {
                float3 npcPos = tr.Value;

                float minDistance = float.MaxValue;
                Entity target = Entity.Null;

                Entities.WithAll<TargetTag>().ForEach((Entity entity, ref Translation position) =>
                {
                    float distance = math.length(npcPos - position.Value);
                    if (distance < minDistance)
                    {
                        target = entity;
                        minDistance = distance;
                    }
                });
                
                if (target != Entity.Null)
                {
                    PostUpdateCommands.AddComponent(npc, new HasTarget
                    {
                        Target = target
                    });
                }
            });
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