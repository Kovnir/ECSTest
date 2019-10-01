using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FindTarget
{
    public class FindTargetSpawner : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material npcMaterial;
        [SerializeField] private Material targetMaterial;

        private float maxHeight;
        private float maxWidth;
        
        [SerializeField]
        private float spawnTime;
        private float currentTime;

        private EntityManager entityManager;
        private EntityArchetype npcArchetype;
        private EntityArchetype targetArchetype;

        void Start()
        {
            entityManager = World.Active.EntityManager;

            npcArchetype = entityManager.CreateArchetype(
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(RenderMesh),
                typeof(SpeedComponent),
                typeof(NPCTag)
            );
            targetArchetype = entityManager.CreateArchetype(
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(RenderMesh),
                typeof(SpeedComponent),
                typeof(TargetTag),
                typeof(Scale)
            );

            var orthographicSize = Camera.main.orthographicSize;
            float pixelsInUnit = Screen.height / (orthographicSize * 2);
            maxHeight = Screen.height / pixelsInUnit / 2f;
            maxWidth = Screen.width / pixelsInUnit / 2f;

            for (int i = 0; i < 500; i++)
            {
                CreateNpc(entityManager, npcArchetype);
            }

            for (int i = 0; i < 500; i++)
            {
                CreateTarget(entityManager, targetArchetype);
            }
        }

        private void Update()
        {
            if (spawnTime <= 0)
            {
                return;
            }
            currentTime += Time.deltaTime;
            while (currentTime - spawnTime > 0)
            {
                CreateTarget(entityManager, targetArchetype);
                currentTime -= spawnTime;
            }
        }

        private void CreateTarget(EntityManager em, EntityArchetype targetArchetype)
        {
            var target = em.CreateEntity(targetArchetype);

            em.SetComponentData(target, new Translation
            {
                Value = new float3(Random.Range(-maxWidth, maxWidth), Random.Range(-maxHeight, maxHeight), 0f)
            });
            em.SetSharedComponentData(target, new RenderMesh
            {
                mesh = mesh,
                material = targetMaterial
            });

            em.SetComponentData(target, new Scale()
            {
                Value = 0.5f,
            });
        }

        private void CreateNpc(EntityManager em, EntityArchetype npcArchetype)
        {
            var npc = em.CreateEntity(npcArchetype);

            em.SetComponentData(npc, new Translation
            {
                Value = new float3(Random.Range(-maxWidth, maxWidth), Random.Range(-maxHeight, maxHeight), 0f)
            });
            em.SetSharedComponentData(npc, new RenderMesh
            {
                mesh = mesh,
                material = npcMaterial
            });
            em.SetComponentData(npc, new SpeedComponent
            {
                Speed = 2
            });
        }
    }

    public struct NPCTag : IComponentData{}
    public struct TargetTag : IComponentData{}
}
