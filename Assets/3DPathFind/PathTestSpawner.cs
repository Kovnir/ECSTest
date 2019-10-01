using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using _3DPathFind;

public class PathTestSpawner : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private Transform start;


    private EntityManager em;
    private Entity npc;

    // Start is called before the first frame update
    void Start()
    {
        em = World.Active.EntityManager;

        npc = em.CreateEntity(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(PathComponent),
            typeof(PathBuffer),
            typeof(SpeedComponent));

        em.SetSharedComponentData(npc, new RenderMesh()
        {
            mesh = mesh,
            material = material
        });
        em.SetComponentData(npc, new Translation()
        {
            Value = start.position
        });
        em.SetComponentData(npc, new PathComponent()
        {
            PathIndex = -1
        });
        em.SetComponentData(npc, new SpeedComponent()
        {
            Speed = 3,
        });
    }

    private Vector3 target;

}
