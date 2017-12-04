using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public enum EntityType
{
    Player, Npc, SyncObject, SomeOtherNpc
}
public enum clientState
{
    idle, spectate, playing, disconnect, tryconnect, loading
}
public enum serverState
{
    disconnected, connected, idle, loading
}
public enum NetworkSide
{
    Server, Client, LocalPlayer
}


/// <summary>
/// Synchroniseur de données multijoueur mutable
/// </summary>
public class NES : NetworkBehaviour
{
    [SyncVar]
    public EntityType _entityType;
    public float _SyncRate = 0.1f;
    public NetworkSide _networkSide;
    [SyncVar]
    public NetworkHash128 _gop_id;

    public Net_DefaultData _data;
    private void Start()
    {
        if (isServer)
        {
            _networkSide = NetworkSide.Server;
                InvokeRepeating("UpdateClientData", 0, _SyncRate);
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                _networkSide = NetworkSide.LocalPlayer;
                InvokeRepeating("UpdateLocalData", 0, _SyncRate);
            }
            else
            {
                _networkSide = NetworkSide.Client;
            }
            print("("+ _networkSide.ToString() + ") Creation Entité : " + _entityType.ToString());
        }
        else { throw new Exception("Un objet inconnu du serveur tente une instanciation !"); }
        //
        InstanciateGOP();
        if (_networkSide == NetworkSide.LocalPlayer)
        {
            Utils.InstanciateNewCamera(transform.GetComponentInParent<ClientEntityLogic>().transform,GlobalAssets.mainInstance.gop_playerCamera, true);
        }
    }
    GameObject InstanciateGOP()
    {
        GameObject Ent = (isServer ?
             Instantiate<GameObject>(GlobalAssets.getMGOP(_gop_id) , FindObjectOfType<MyServer>().transform)
            : Instantiate<GameObject>(GlobalAssets.getMGOP(_gop_id), FindObjectOfType<MyClient>().transform));
        Ent.name = _networkSide + ":" + _entityType.ToString()+":"+ Ent.name;
        name = _networkSide + " [" + netId + "]";
        Ent.transform.position = _data.position;
        Ent.transform.rotation = _data.rotation;
        transform.SetParent(Ent.transform);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        return Ent;
    }
    /*
    public override void OnStartServer(){base.OnStartServer();}
    public override void OnStartClient(){base.OnStartClient();}
    public override void OnStartLocalPlayer(){base.OnStartLocalPlayer();}
    */
    private void Update(){}

    private void UpdateLocalData(){Cmd_SendDataToServer(_data);}
    [Command]
    private void Cmd_SendDataToServer(Net_DefaultData data){_data = data;}

    [Server]
    private void UpdateClientData(){Rpc_UpdateClientData(_data); }
    [ClientRpc]
    private void Rpc_UpdateClientData(Net_DefaultData data){_data = data;}

    /// <summary>
    /// Interact with dest NES
    /// </summary>
    /// <param name="dest">NES destination</param>
    public void Interact(NES dest){Cmd_Interact(dest.netId, !dest._data.activated);}

    /// <summary>
    /// Server Command to contact a NES with NetworkInstanceId
    /// </summary>
    /// <param name="nid"></param>
    /// <param name="value"></param>
    [Command]
    public void Cmd_Interact(NetworkInstanceId nid,bool value)
    {
        NES go = NetworkServer.FindLocalObject(nid).GetComponent<NES>();
        go._data.activated = value;
    }

    [Command]
    public void Cmd_SetPlayerAsRemote(NetworkInstanceId nid, bool value)
    {
        Utils.setActiveColliders(NetworkServer.FindLocalObject(nid).GetComponent<NES>().transform.parent.gameObject, true);
    }

    void OnDestroy(){Destroy(this.transform.parent.gameObject);}
    
   
}
