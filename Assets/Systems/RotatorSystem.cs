using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class RotatorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation, ref RotationSpeed rotator) =>
        {
            rotation.Value = math.mul(math.normalizesafe(rotation.Value),
                quaternion.AxisAngle(math.up(), rotator.Speed * Time.deltaTime));
        });
    }
}