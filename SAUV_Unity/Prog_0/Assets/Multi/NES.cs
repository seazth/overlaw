using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
/// <summary>
/// Synchroniseur de données multijoueur
/// </summary>
public class NES : NetworkBehaviour
{

    [SyncVar]
    public EntityType _entityType;
    public float _SyncRate = 0.02f;
    public NetworkSide _networkSide;

    public Net_DefaultData _data; 

    private void Start()
    {

        _data = new Net_DefaultData();
        GameObject Ent;
        if (isServer)
        {
            _networkSide = NetworkSide.Server;
            Ent = Instantiate<GameObject>(GlobalAssets.mainInstance.gop_serverEntity, FindObjectOfType<MyServer>().transform);
            transform.SetParent(Ent.transform);
            Ent.name = _networkSide + ":" + _entityType.ToString();
            Ent.transform.position = UnityEngine.Random.insideUnitCircle * 10f;
            Ent.transform.position = new Vector3(Ent.transform.position.x, 1f, Ent.transform.position.z);
            //Ent.GetComponent<ServerEntityLogic>().DestinationPos = Ent.transform.position;
            _data.position = Ent.transform.position;
            _data.rotation = Ent.transform.rotation;
            InvokeRepeating("UpdateClientPosition", 0, _SyncRate);
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                _networkSide = NetworkSide.LocalPlayer;
                Ent = Instantiate<GameObject>(GlobalAssets.mainInstance.gop_character, FindObjectOfType<MyClient>().transform);
                InvokeRepeating("UpdateLocalplayerPosition", 0, _SyncRate);
            }
            else
            {
                _networkSide = NetworkSide.Client;
                Ent = Instantiate<GameObject>(GlobalAssets.mainInstance.gop_character, FindObjectOfType<MyClient>().transform);
            }
            print("("+ _networkSide.ToString() + ") Creation Entité : " + _entityType.ToString());

            transform.SetParent(Ent.transform);
            Ent.name = _networkSide + ":" + _entityType.ToString();
            Ent.transform.position = UnityEngine.Random.insideUnitCircle * 20f;
            Ent.transform.position = new Vector3(Ent.transform.position.x,1f, Ent.transform.position.z);
        }
    }
    /*
    public override void OnStartServer(){base.OnStartServer();}
    public override void OnStartClient(){base.OnStartClient();}
    public override void OnStartLocalPlayer(){base.OnStartLocalPlayer();}
    */
    private void Update(){}

    /// <summary>
    /// 
    /// </summary>
    public void UpdateLocalplayerPosition()
    {
        Cmd_ServerMove(_data);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    [Command]
    public void Cmd_ServerMove(Net_DefaultData data)
    {
        //GetComponentInParent<ServerEntityLogic>().UpdateNetData(data);
        _data = data;
    }

    /// <summary>
    /// 
    /// </summary>
    [Server]
    private void UpdateClientPosition()
    {
        RpcClientMove(_data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    [ClientRpc]
    private void RpcClientMove(Net_DefaultData data)
    {
        _data = data;
        //GetComponentInParent<ClientEntityLogic>().UpdateNetData(data);
    }
    
    void OnDestroy()
    {
        Destroy(this.transform.parent.gameObject);
    }
    
   
}
public interface INet_Data
{
    Net_DefaultData _data { get; set; }
    void UpdateNetData(Net_DefaultData data);
}
public class Net_DefaultData
{
    public Vector3 position = new Vector3(0f, 1f, 0f);
    public Quaternion rotation  = new Quaternion();

    public virtual void UpdateServerEntityLogic(ServerEntityLogic self)
    {
        if (self._NES._entityType == EntityType.Npc)
        {
            self._NES._data.position = self.transform.position;
            self._NES._data.rotation = self.transform.rotation;
        }
        else
        {
            self.transform.position = self._NES._data.position;
            self.transform.rotation = self._NES._data.rotation;
        }
    }

    public virtual void UpdateClientEntityLogic(ClientEntityLogic self)
    {
        self.transform.position = Vector3.Lerp(self.transform.position, self._NES._data.position, .1f);
        self.transform.rotation = Quaternion.Lerp(self.transform.rotation, self._NES._data.rotation, 0.1f);
    }

    public virtual void UpdateLocalPlayerLogic(ClientEntityLogic self)
    {
        self._NES._data.position = self.transform.position;
        self._NES._data.rotation = self.transform.rotation;
    }
}