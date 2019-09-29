using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class JobTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        RunSimple();
        RunJobs();
        RunBurstJobs();
    }

    private void RunBurstJobs()
    {
        var startTime = Time.realtimeSinceStartup;
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(10, Allocator.Temp);
        for (int i = 0; i < 10; i++)
        {
            JobHandle jobHandle = ReallyToughTaskJobBurst(50000);
            jobHandles.Add(jobHandle);
        }

        JobHandle.CompleteAll(jobHandles);
        jobHandles.Dispose();

        Debug.Log("Job Burst: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
    }
    private void RunJobs()
    {
        var startTime = Time.realtimeSinceStartup;
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(10, Allocator.Temp);
        for (int i = 0; i < 10; i++)
        {
            JobHandle jobHandle = ReallyToughTaskJob(50000);
            jobHandles.Add(jobHandle);
        }

        JobHandle.CompleteAll(jobHandles);
        jobHandles.Dispose();

        Debug.Log("Job: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
    }

    private void RunSimple()
    {
        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < 10; i++)
        {
            ReallyToughTask(50000);
        }

        Debug.Log("Simple: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
    }

    private void ReallyToughTask(float count)
    {
        float value = 0f;
        for (int i = 0; i < count; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ReallyToughTaskJob(float count)
    {
        ReallyToughTaskJob reallyToughTaskJob = new ReallyToughTaskJob() {Count = count};
        return reallyToughTaskJob.Schedule();
    }
    private JobHandle ReallyToughTaskJobBurst(float count)
    {
        ReallyToughTaskJobBurst reallyToughTaskJobBurst = new ReallyToughTaskJobBurst() {Count = count};
        return reallyToughTaskJobBurst.Schedule();
    }
}


public struct ReallyToughTaskJob : IJob
{
    public float Count;
    
    public void Execute()
    {
        float value = 0f;
        for (int i = 0; i < Count; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct ReallyToughTaskJobBurst : IJob
{
    public float Count;
    
    public void Execute()
    {
        float value = 0f;
        for (int i = 0; i < Count; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}