using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    
    private void Start()
    {
        var em = World.Active.EntityManager;

        EntityArchetype entityArchetype = em.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveSpeed)
        );


        NativeArray<Entity> entities = new NativeArray<Entity>(10000, Allocator.Temp);
        em.CreateEntity(entityArchetype, entities);
        foreach (var entity in entities)
        {
            em.SetComponentData(entity, new LevelComponent {Level = Random.Range(10,20)});
            em.SetComponentData(entity, new MoveSpeed {moveSpeed= Random.Range(0.1f,5)});
            em.SetComponentData(entity, new Translation {Value = new float3(Random.Range(-8,8),Random.Range(-5,5),0)});
            
            em.SetSharedComponentData(entity, new RenderMesh()
            {
                mesh = mesh,
                material = material
            });
        }
        
        entities.Dispose();
    }
}
