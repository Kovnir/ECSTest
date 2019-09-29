using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class JobParallelForTesting : MonoBehaviour
{
    public class DataToProcess
    {
        public float3 Position;
    }

    List<DataToProcess> dataToProcesses = new List<DataToProcess>();

    private void Start()
    {
        for (int i = 0; i < 100000; i++)
        {
            dataToProcesses.Add(new DataToProcess());
        }
    }

    // Update is called once per frame
    void Update()
    {
        RunSimple();
        RunJobs();
//        RunBurstJobs();
    }

//    private void RunBurstJobs()
//    {
//        var startTime = Time.realtimeSinceStartup;
//        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(10, Allocator.Temp);
//        for (int i = 0; i < 10; i++)
//        {
//            JobHandle jobHandle = ReallyToughTaskJobBurst(50000);
//            jobHandles.Add(jobHandle);
//        }
//
//        JobHandle.CompleteAll(jobHandles);
//        jobHandles.Dispose();
//
//        Debug.Log("Job Burst: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
//    }

    private void RunJobs()
    {
        var startTime = Time.realtimeSinceStartup;

        ReallyToughTaskJob();
        
        Debug.Log("Job: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
    }


    private void RunSimple()
    {
        float startTime = Time.realtimeSinceStartup;

        float random = Random.Range(0, 100);
            
        float time = Time.deltaTime;
        for (int i = 0; i < dataToProcesses.Count; i++)
        {
            DataToProcess data = dataToProcesses[i];
            data.Position = JobTestingTool.Process(data.Position, random) * time;
        }

        Debug.Log("Simple: " + (Time.realtimeSinceStartup - startTime) * 1000f + " ms");
    }


    private void ReallyToughTaskJob()
    {
        NativeArray<float3> dataArray = new NativeArray<float3>(dataToProcesses.Count, Allocator.TempJob);
        for (var index = 0; index < dataToProcesses.Count; index++)
        {
            dataArray[index] = dataToProcesses[index].Position;
        }

        ReallyToughTaskJobParallelFor reallyToughTaskJobParallelFor = new ReallyToughTaskJobParallelFor
        {
            Time = Time.deltaTime,
            DataArray = dataArray,
            Random = Random.Range(0, 100)
        };
        JobHandle jobHandle = reallyToughTaskJobParallelFor.Schedule(dataArray.Length, 100);
        jobHandle.Complete();

        for (int i = 0; i < dataArray.Length; i++)
        {
            dataToProcesses[i].Position = dataArray[i];
        }
        
        dataArray.Dispose();
    }
    
//    private JobHandle ReallyToughTaskJobBurst(float count)
//    {
//        ReallyToughTaskJobBurst reallyToughTaskJobBurst = new ReallyToughTaskJobBurst() {Count = count};
//        return reallyToughTaskJobBurst.Schedule();
//    }


}

public static class JobTestingTool
{
    public static float Process(float value, float random)
    {
        return math.sqrt(value) + math.exp10(random);
    }
    public static float3 Process(float3 value, float random)
    {
        return new float3(Process(value.x, random), Process(value.y, random), Process(value.z, random));
    }
}


public struct ReallyToughTaskJobParallelFor : IJobParallelFor
{
    public NativeArray<float3> DataArray;
    [ReadOnly] public float Time;
    [ReadOnly] public float Random;

    public void Execute(int index)
    {
        float3 data = DataArray[index];
        DataArray[index] = JobTestingTool.Process(data, Random) * Time;
    }
}
//
//[BurstCompile]
//public struct ReallyToughTaskJobBurst : IJob
//{
//    public float Count;
//    
//    public void Execute()
//    {
//        float value = 0f;
//        for (int i = 0; i < Count; i++)
//        {
//            value = math.exp10(math.sqrt(value));
//        }
//    }
//}