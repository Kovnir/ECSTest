using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TwoDWalk
{


    public class WalkSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                float3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                float time = Time.deltaTime;
                Entities.WithAll<FollowComponentTag>()
                    .ForEach((ref SpeedComponent speed, ref Translation translation) =>
                    {
                        float3 direction = position - translation.Value;
                        direction = math.normalize(direction);
                        translation.Value += direction * speed.Speed * time;
                    });
            }
        }
    }

    public struct FollowComponentTag : IComponentData
    {

    }

    public struct SpeedComponent : IComponentData
    {
        public float Speed;
    }
}