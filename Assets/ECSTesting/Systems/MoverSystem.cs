using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref MoveSpeed moveSpeed
        ) =>
        {
            translation.Value.y += moveSpeed.moveSpeed * Time.deltaTime;
            if (translation.Value.y > 5)
            {
                moveSpeed.moveSpeed = -math.abs(moveSpeed.moveSpeed);
            }
            if (translation.Value.y < -5)
            {
                moveSpeed.moveSpeed = math.abs(moveSpeed.moveSpeed);
            }
        });
    }
}
