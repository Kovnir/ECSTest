using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TwoDWalk;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class TwoDWalkSpawner : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        var em = World.Active.EntityManager;

        var npcArchetype = em.CreateArchetype(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(SpeedComponent),
            typeof(FollowComponentTag)
        );

        NativeArray<Entity> array = new NativeArray<Entity>(50000, Allocator.Temp);
        em.CreateEntity(npcArchetype, array);

        float w = Screen.width / 40f;
        float h = Screen.height / 40f;
            
        foreach (var npc in array)
        {
            
            em.SetComponentData(npc, new Translation
            {
                Value = new float3(Random.Range(-w,w),Random.Range(-h,h),0f)
//                Value = new float3(Random.Range(-w,w),Random.Range(-h,h),0f)
            });
            em.SetSharedComponentData(npc, new RenderMesh
            {
                mesh = mesh,
                material = material
            });
            em.SetComponentData(npc, new SpeedComponent
            {
                Speed = 2
            });   
        }
        
        array.Dispose();
    }
}
