using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class RotatorSystem : ComponentSystem
{

    private struct Job : IJob
    {
        public Rotation Rotation;
        [ReadOnly] public float Time;
        [ReadOnly] public float Speed;

        public void Execute()
        {
            Rotation.Value = math.mul(math.normalizesafe(Rotation.Value),
                quaternion.AxisAngle(math.up(), Speed * Time));
        }
    }
    protected override void OnUpdate()
    {
        NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((ref Rotation rotation, ref RotationSpeed rotator) =>
        {
            Job job = new Job
            {
                Rotation = rotation,
                Speed = rotator.Speed, 
                Time = Time.deltaTime
            };
            jobs.Add(job.Schedule());
//            rotation.Value = math.mul(math.normalizesafe(rotation.Value),
//                quaternion.AxisAngle(math.up(), rotator.Speed * Time.deltaTime));
        });
        JobHandle.CompleteAll(jobs);
        jobs.Dispose();          
    }
}