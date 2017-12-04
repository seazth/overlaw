using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public sealed class MyClient : MonoBehaviour
{
    public static  NetworkClient _client { get; private set; }

    private NetworkIdentity _networkStateEntityProtoType;   
    public static clientState _clientState { get; private set; }
    //public CTRL_Player playerController;
    void Start()
    {
        //on client, this isn't required but is nice for testing.
        _clientState = clientState.disconnect;
        Application.runInBackground = true;
        var globals = FindObjectOfType<GlobalAssets>();
        _networkStateEntityProtoType = globals.gop_NES.GetComponent<NetworkIdentity>();
        ClientScene.RegisterSpawnHandler(_networkStateEntityProtoType.assetId, OnSpawnEntity, OnDespawnEntity);
        //
        
    }   

    public bool isLocalServer(string serverip,int serverport)
    {
        return NetworkServer.listenPort == serverport
            && (serverip.ToLower() == "localhost" || serverip.Replace(" ", "") == "127.0.0.1");
    }
    public void ConnectToServer()
    {
        print("<TRY CONNECT>");
        _client = new NetworkClient();
        _client.Connect(MNG_GameManager.mainInstance.serverip, MNG_GameManager.mainInstance.serverport);
        _client.RegisterHandler(MsgType.Connect, OnClientConnected);
        _client.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);
        _clientState = clientState.tryconnect;
        gameObject.name = "MNG_Client:TryConnect";
    }
    public void DisconnectedFromServer()
    {
        if (_clientState == clientState.disconnect) { return; }
        print("<TRY DISCONNECT>");
        ClientScene.DestroyAllClientObjects();
        _client.Disconnect();

        MNG_GameManager.mainInstance.inGame = false;
        _clientState = clientState.disconnect;

        Main_Canvas.set_switchConnectButton(true);
        //throw new Exception("L'action disconnect n'est pas corretement implémenté : brutal ! Il faut envoyer un message");
        gameObject.name = "MNG_Client";
    }
    private void OnDespawnEntity(GameObject go)
    {
        print("(Client) OnDespawnEntity");
        Destroy(go);
    }
    private void OnClientConnected(NetworkMessage netMsg)
    {
        print("(Client) OnClientConnected");
        _clientState = clientState.idle;
        ClientScene.Ready(netMsg.conn);
        //
        ClientScene.AddPlayer(1); //MODIF
        MNG_GameManager.mainInstance.inGame = true;
        Main_Canvas.set_switchConnectButton(false);
        gameObject.name = "MNG_Client:Connected";
    }
    private void OnClientDisconnected(NetworkMessage netMsg)
    {
        print("(Client) OnClientDisconnected (SERVER HAS SHUTDOWN ?)");
        for (int i = gameObject.transform.childCount - 1; i > 0; i--) { Destroy(gameObject.transform.GetChild(i).gameObject); }
        MNG_GameManager.mainInstance.inGame = false;
        _clientState = clientState.disconnect;
        Main_Canvas.set_switchConnectButton(true);
    }
    private GameObject OnSpawnEntity(Vector3 position, NetworkHash128 assetId)
    {
        //print("(Client) OnSpawnEntity");
        var networkEntity = Instantiate<NetworkIdentity>(_networkStateEntityProtoType);
        networkEntity.transform.SetParent(this.transform);
        return networkEntity.gameObject;
    }
    public void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player[" + player.externalIP + "] connected from " + player.ipAddress + ":" + player.port);
    }
}
