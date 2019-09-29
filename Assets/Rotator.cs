using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct Rotator : IComponentData
{
    public float Speed;
}

public class RotatorComponent : ComponentDataProxy<Rotator> {}

public class RotatorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation, ref Rotator rotator) =>
        {
            var deltaTime = Time.deltaTime;
            rotation.Value = math.mul(math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(), rotator.Speed * deltaTime));
        });
    }
}
