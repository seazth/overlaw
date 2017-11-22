using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ServerEntityLogic : MonoBehaviour
{

    public NES _NES { get; protected set; }
    NavMeshAgent nav_navmesh;


    void Start()
    {
        nav_navmesh = GetComponent<NavMeshAgent>();
        _NES = this.GetComponentInChildren<NES>();
        if (_NES._entityType == EntityType.Npc)
            InvokeRepeating("chooseDestination", 0, 3f);
    }

    void chooseDestination()
    {
        Vector3 random2D;
        random2D = (20 * Random.insideUnitCircle);
        nav_navmesh.SetDestination(transform.position + new Vector3(random2D.x, 0f, random2D.y));
    }

    void Update()
    {
        _NES._data.UpdateServerEntityLogic(this);
    }

}

