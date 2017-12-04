using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System;
using System.Collections.Generic;

public sealed class MyServer : MonoBehaviour
{
    public static serverState _serverState { get; private set; }
    public int ConnectionLength;

    void Awake()
    {
        print("(Server) Awake");
        Application.runInBackground = true;
        NetworkServer.RegisterHandler(MsgType.Connect, OnPlayerConnect);
        NetworkServer.RegisterHandler(MsgType.AddPlayer, OnAddPlayer);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnPlayerDisconnect);
    }
    

    public void StartServer()
    {

        if (_serverState != serverState.disconnected || MyClient._clientState != clientState.disconnect) { return; }
        NetworkServer.Listen(MNG_GameManager.mainInstance.serverport);
        Main_Canvas.add_serverlog("(Server) Started");
        gameObject.name = "MNG_Server:Started";
        Main_Canvas.set_switchServerButton(false);
        _serverState = serverState.connected;

        // spawn random
        spawnRandNpc(1,0,EntityType.Npc);
        spawnRandNpc(1,2,EntityType.Npc);

        // spawn specific obj
        ServerSpawnNPC(new Vector3(-0.85f, 0f, -7f), 1, EntityType.SyncObject);

        MNG_GameManager.mainInstance.serverip = "Localhost";
        FindObjectOfType<MyClient>().ConnectToServer();
    }

    void spawnRandNpc(int number, int assetid, EntityType entitytype)
    {
        Vector3 pos;
        for (int i = 0; i < number; i++)
        {
            pos= UnityEngine.Random.insideUnitCircle * 10f;
            ServerSpawnNPC(new Vector3(pos.x, 2f, pos.z), assetid, entitytype);
        }
    }

    public void StopServer()
    {
        if (_serverState == serverState.disconnected) { return; }
        NetworkServer.DisconnectAll();
        NetworkServer.Shutdown();
        for (int i = gameObject.transform.childCount-1; i >= 0; i--){Destroy(gameObject.transform.GetChild(i).gameObject);}
        Main_Canvas.set_switchServerButton(true);
        gameObject.name = "MNG_Server";
        _serverState = serverState.disconnected;
    }

    private void ServerSpawnNPC(Vector3 position,int assetid , EntityType entitytype)
    {
        if(entitytype == EntityType.Player) { throw new Exception("Impossible to instanciate manually a player entity !"); }
        print("(Server) ServerSpawn ["+ GlobalAssets.mainInstance.multiplayers_gop[assetid].name + "]");
        var npc = createNewNES(new Net_DefaultData() { position = position}, GlobalAssets.getMGOP_Assetid(assetid) , entitytype,false)
            .gameObject;
        NetworkServer.Spawn(npc);
    }



    private NES createNewNES(Net_DefaultData data, NetworkHash128 gopid, EntityType type , bool isLocalhostPlayer)
    {
        NES go = Instantiate<GameObject>(GlobalAssets.mainInstance.gop_NES).GetComponent<NES>();
        go._entityType = type;
        go._data = data;
        go._gop_id = gopid;
        return go;
    }

    private void OnPlayerConnect(NetworkMessage netMsg)
    {
        Main_Canvas.add_serverlog("(Server) CONNECTION Player " + getPlayerConnectionString(netMsg.conn));
        print("(Server) CONNECTION Player " + getPlayerConnectionString(netMsg.conn));
    }

    private void OnPlayerDisconnect(NetworkMessage netMsg)
    {
        Main_Canvas.add_serverlog("(Server) DISCONNECTION Player " + getPlayerConnectionString(netMsg.conn));
        print("(Server) DISCONNECTION Player " + getPlayerConnectionString(netMsg.conn));
        SaveClientState();
        GameObject playerGamePiece = netMsg.conn.playerControllers[0].gameObject;
        NetworkServer.UnSpawn(playerGamePiece);
        Destroy(playerGamePiece);
    }
    public void SaveClientState() {/* FAIRE ICI LES SAUVEGARDES DU JOUEURS SUR BDD ?*/ }

    private void OnApplicationQuit()
    {
        Main_Canvas.add_serverlog("(Server) Shutdown");
        NetworkServer.DisconnectAll();
        //for (int i = 0; i < Network.connections.Length; i++){Network.CloseConnection(Network.connections[0], true);}
        if(NetworkServer.active) NetworkServer.Shutdown();
    }

    private void OnAddPlayer(NetworkMessage netMsg)
    {
        print("(Server) INSTANCIATE Player " + getPlayerConnectionString(netMsg.conn));
        Vector3 pos = UnityEngine.Random.insideUnitCircle * 10f;

        GameObject go_player = createNewNES(
            new Net_DefaultData() { position = new Vector3(pos.x, 1f, pos.z) }
            , GlobalAssets.getMGOP_Assetid(0)
            , EntityType.Player
            , true
            )
            .gameObject;
        NetworkServer.AddPlayerForConnection(netMsg.conn, go_player, (short)Network.connections.Length);
    }
    public static string getPlayerConnectionString(NetworkConnection netconn)
    {
        return "[" + netconn.connectionId + "/" + netconn.address + "]";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(5,0,150,20), "Player ping values");
        for (int i = 0; i < Network.connections.Length; i++)
        {
            GUI.Label(new Rect(5, (i + 1) * 20, 150, 20), "Player " + Network.connections[i] + " - " + Network.GetAveragePing(Network.connections[i]) + " ms");
        }
    }
} 
