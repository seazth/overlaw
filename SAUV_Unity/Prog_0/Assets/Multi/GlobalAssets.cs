using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AI;


public class GlobalAssets : MonoBehaviour
{
    public static GlobalAssets mainInstance { get; private set; }
    void Awake(){mainInstance = this;}

    public GameObject gop_NES;
    public GameObject gop_serverEntity;
    public GameObject gop_playerCamera;
    public Material mat_MaterialMissing;
    public NavMeshAgent gop_defaultNavMesh;
    public Material mat_serverEnt;

    public LayerMask lay_serverEntity;

    public GameObject[] multiplayers_gop;

    public static NetworkHash128 getMGOP_Assetid(int index)
    {
        if (index > mainInstance.multiplayers_gop.Length || index < 0) { throw new System.Exception("L'asset [" + index + "] n'existe pas !"); }
            return mainInstance.multiplayers_gop[index].GetComponent<NetworkIdentity>().assetId;
    }
    public static GameObject getMGOP(NetworkHash128 assetid)
    {
        for (int i = 0; i < mainInstance.multiplayers_gop.Length; i++)
        {
            if(mainInstance.multiplayers_gop[i].GetComponent<NetworkIdentity>().assetId.Equals(assetid)) { return mainInstance.multiplayers_gop[i]; }
        }
        throw new System.Exception("L'asset ["+ assetid + "] n'existe pas !");
    }

}

