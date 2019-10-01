using System;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace _3DPathFind
{
    [UsedImplicitly]
    public class MovePathSystem : ComponentSystem
    {
        
        private EntityManager entityManager;

        protected override void OnStartRunning()
        {
            entityManager = World.Active.EntityManager;
        }
        
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity e, ref PathComponent pathComponent, ref SpeedComponent speed, ref Translation tr) =>
            {
                if (pathComponent.PathIndex != -1)
                {
                    DynamicBuffer<PathBuffer> buffer = entityManager.GetBuffer<PathBuffer>(e);
                    var nextPosition = buffer[pathComponent.PathIndex].Value;
                    
                    float3 direction = nextPosition - tr.Value;
                    float3 move = math.normalize(direction) * speed.Speed * Time.deltaTime;
                    //nan because firs point can be equal to this position and math.normalize will return nan
                    if (Single.IsNaN(move.x) || math.length(move) >= math.length(direction))
                    {
                        tr.Value = nextPosition;
                        pathComponent.PathIndex++;
                        if (pathComponent.PathIndex > buffer.Length -1)
                        {
                            pathComponent.PathIndex = -1;
                        }
                    }
                    else
                    {
                        tr.Value += move;
                    }
                }
            });
        }
    }

    [UsedImplicitly]
    public class SetPathSystem : ComponentSystem
    {
        private Camera camera;
        private NavMeshPath path;

        private EntityManager entityManager;

        protected override void OnStartRunning()
        {
            camera = Camera.main;
            path = new NavMeshPath();
            entityManager = World.Active.EntityManager;
        }

        protected override void OnUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Vector3 targer;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    targer = hit.point;
                    Entities.ForEach((Entity e, ref PathComponent pathComponent, ref Translation tr) =>
                    {
                        NavMesh.CalculatePath(tr.Value, targer, NavMesh.AllAreas, path);
                        var corners = path.corners;
                        if (corners.Length > 0)
                        {
                            DynamicBuffer<PathBuffer> buffer = entityManager.GetBuffer<PathBuffer>(e);
                            buffer.Clear();
                            foreach (Vector3 corner in corners)
                            {
                                buffer.Add(corner);
                            }

                            pathComponent.PathIndex = 0;
                        }
                    });
                }
            }

//            if (path != null)
//            {
//                for (int i = 0; i < path.corners.Length - 1; i++)
//                    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
//            }

        }
    }

    public struct PathComponent : IComponentData
    {
        public int PathIndex;
    }
    
    public struct SpeedComponent : IComponentData
    {
        public int Speed;
    }
    
    [InternalBufferCapacity(8)]
    public struct PathBuffer : IBufferElementData
    {
        public static implicit operator float3(PathBuffer e) { return e.Value; }
        public static implicit operator PathBuffer(float3 e) { return new PathBuffer { Value = e }; }
        public static implicit operator Vector3(PathBuffer e) { return e.Value; }
        public static implicit operator PathBuffer(Vector3 e) { return new PathBuffer { Value = e }; }

        // Actual value each buffer element will store.
        public float3 Value;
    }
}