using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System;
using System.Collections.Generic;

public sealed class MyServer : MonoBehaviour
{
    public serverState _serverState { get; private set; }

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
        if (_serverState != serverState.disconnected) { return; }
        NetworkServer.Listen(MNG_GameManager.mainInstance.serverport);
        Main_Canvas.add_serverlog("(Server) Started");
        gameObject.name = "MNG_Server:Started";
        Main_Canvas.set_switchServerButton(false);
        _serverState = serverState.connected;
        ServerSpawnNPC();
        ServerSpawnNPC();
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

    private void ServerSpawnNPC()
    {
        print("(Server) ServerSpawnNPC");
        var globals = FindObjectOfType<GlobalAssets>();
        //initialize all server-controlled entities here
        var npc = Instantiate<GameObject>(globals.gop_NES);
        npc.GetComponent<NES>()._entityType = EntityType.Npc;
        NetworkServer.Spawn(npc);
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
        NetworkServer.Shutdown();
    }

    private void OnAddPlayer(NetworkMessage netMsg)
    {
        print("(Server) INSTANCIATE Player " + getPlayerConnectionString(netMsg.conn));
        var globals = FindObjectOfType<GlobalAssets>();
        var playerStateGo = Instantiate<GameObject>(globals.gop_NES);
        NES playerState = playerStateGo.GetComponent<NES>();
        playerState._entityType = EntityType.LocalPlayer;
        playerState.transform.SetParent(this.transform);
        NetworkServer.AddPlayerForConnection(netMsg.conn, playerStateGo, (short)Network.connections.Length);
    }
    public static string getPlayerConnectionString(NetworkConnection netconn)
    {
        return "[" + netconn.connectionId + "/" + netconn.address + "]";
    }
} 
