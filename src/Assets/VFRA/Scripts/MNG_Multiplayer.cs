using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MNG_Multiplayer : Photon.MonoBehaviour {


    //Multi 
    string MS_adress = "127.0.0.1";
    int MS_port = 80;
    ExitGames.Client.Photon.ConnectionProtocol MS_protocol = ExitGames.Client.Photon.ConnectionProtocol.WebSocket;
    string MS_appid = "6d12a3eb-889b-463f-998e-4dee86b25971";
    string MS_ConnVersion = "v1.0";
    bool MS_WebCloudConnection = true;

    public void SetMS_adress(string value) { MS_adress = value; }
    public void SetMS_port(string value) { try { MS_port = int.Parse(value); } catch { } }
    public void SetMS_protocol(int value) {
        try {
            switch (value)
            {
                case 0:
                    MS_protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
                    break;
                case 1:
                    MS_protocol = ExitGames.Client.Photon.ConnectionProtocol.Tcp;
                    break;
                case 2:
                    MS_protocol = ExitGames.Client.Photon.ConnectionProtocol.WebSocket;
                    break;
                case 3:
                    MS_protocol = ExitGames.Client.Photon.ConnectionProtocol.WebSocketSecure;
                    break;
            }
        }
        catch { }
    }
    public void SetMS_ConnVersion(string value) { MS_ConnVersion = value; }  
    public void SetMS_WebCloudConnection(string value) { try { MS_WebCloudConnection = bool.Parse(value); } catch { } }  

    public Text text_ConnectionState;

    public MultiplayerProtocol[] ConnectionsPreset; // INIT = WebCloud, CustomWebCloud, CustomTCPCloud,CustomUDPCloud
    public InputField AdressInput;
    public InputField PortInput;
    public Dropdown ConnectionsPresetDropdown;
    public InputField ConnVersionInput;
    public Dropdown ProtocolDropdown;
    public Button Btn_Multiplayer;
    public InputField Input_Pseudo;

    void Start () {

        loadConnectionsPresets();
        onNewConnectionPreset(0);
        Btn_Multiplayer.interactable = false;
        tryLoggingtoMaster();
    }
	
	void Update () {
        string context = ""
            + (PhotonNetwork.inRoom ? " inRoom" : "")
            + (PhotonNetwork.insideLobby ? " insideLobby" : "")
            + (PhotonNetwork.player.IsInactive ? " IsInactive" : "")
            ;
        text_ConnectionState.text = (context!=""?"["+ context + " ] ":"") + PhotonNetwork.networkingPeer.State.ToString();
    }

    public void tryLoggingtoMaster()
    {
        if (!PhotonNetwork.connected)
        {
            if (MS_WebCloudConnection)
            {
                print("<color=green>Trying ConnectToRegion EU : " + MS_ConnVersion+" </color>");
                PhotonNetwork.ConnectToRegion(CloudRegionCode.eu,MS_ConnVersion);
            }
            else
            {
                print("PhotonNetwork.gameVersion: " + PhotonNetwork.gameVersion);
                print("PhotonNetwork.versionPUN: " + PhotonNetwork.versionPUN);
                print("<color=green>Trying ConnectToMaster : " + MS_adress + ":" + MS_port + " " +MS_ConnVersion+ " "+ MS_protocol.ToString()+ " </color>");
                PhotonNetwork.SwitchToProtocol(MS_protocol);
                PhotonNetwork.ConnectToMaster(MS_adress, MS_port, MS_appid, MS_ConnVersion);
            }
        }
    }

    void OnGUI()
    {
        if (Input.GetKey(KeyCode.F1) && PhotonNetwork.inRoom)
        {   
            Rect centeredRect = new Rect(5, 45, Screen.width-10, (PhotonNetwork.playerList.Length+ MNG_GameManager.getTeams.Length+4) * 14 + 20);

            GUILayout.BeginArea(centeredRect, GUI.skin.box);
            {
                string tmp = string.Empty;
                tmp += "[ROOM INFOS]\n\n";
                tmp += "ROOM > " + PhotonNetwork.room   .Name
                        + " GAMESTATE: " + PhotonNetwork.room.GetAttribute<GameState>(RoomAttributes.GAMESTATE, GameState.GameState_error)
                        + (PhotonNetwork.room.GetAttribute<bool>(RoomAttributes.PLAYERSREADY, false) ? " PLAYERSREADY" : "")
                        + (PhotonNetwork.room.GetAttribute<bool>(RoomAttributes.PLAYERSCANSPAWN, false) ? " PLAYERSCANSPAWN" : "")
                        + (PhotonNetwork.room.GetAttribute<bool>(RoomAttributes.PRISONOPEN, false) ? " PRISONOPEN" : "")
                        + (PhotonNetwork.room.GetAttribute<bool>(RoomAttributes.IMMOBILIZEALL, false) ? " IMMOBILIZEALL" : "")
                        + " TS="+ (int)Time.timeSinceLevelLoad
                        + " Ping: " + PhotonNetwork.GetPing()+"ms"
                        + "\n";
                foreach (PhotonPlayer p in PhotonNetwork.playerList)
                {
                    tmp +=
                        "PLAYER > " + p.NickName //SCORE,TEAM,PLAYERSTATE,ISIDLE,ISLAGGY
                        + " TEAM: " + MNG_GameManager.getTeams[p.getTeamID()].TeamName
                        + " STATE: " + (p.GetPlayerState()).ToString()
                        + " ISALIVE: " + p.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)
                        + (p.IsMasterClient? " MASTERCLIENT" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISREADY, false)? " ISREADY" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISLAGGY, false)?" ISLAGGY":"")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISROOMADMIN, false)? " ISROOMADMIN" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISIDLE, false)? " ISIDLE" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.INPRISONZONE, false)? " INPRISONZONE" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISCAPTURED, false) ? " ISCAPTURED" : "")
                        + (p.GetAttribute<bool>(PlayerAttributes.ISIMMOBILIZED, false) ? " ISIMMOBLIZED" : "")
                        + " "+ p.GetAttribute<string>(PlayerAttributes.INZONE, "?")
                        + " SCORE: " + p.GetScore()
                        + " " + p.CustomProperties[PlayerAttributes.testKey]
                        + "\n";
                }
                tmp += "\n";
                for (int tid = 0; tid < MNG_GameManager.getTeams.Length; tid++)
                {
                    tmp +=
                        "TEAM > " + MNG_GameManager.getTeams[tid].TeamName
                        + " PLAYERSALIVE: " + PhotonNetwork.room.GetTeamAttribute<int>(tid,TeamAttributes.PLAYERSALIVE,0)
                        + " PLAYERSCOUNT: " + PhotonNetwork.room.GetTeamAttribute<int>(tid,TeamAttributes.PLAYERSCOUNT,0)
                        + "\n";
                }
                // AFICHER TEAM ICI
                GUILayout.Label(tmp);
            }
            GUILayout.EndArea();
        }
    }

    public void onNewConnectionPreset(int value)
    {
        MS_adress = ConnectionsPreset[value].protocolAdress;
        AdressInput.text = MS_adress;
        MS_port = ConnectionsPreset[value].protocolDefaultPort;
        PortInput.text = MS_port.ToString();
        MS_WebCloudConnection = ConnectionsPreset[value].useWebCloudConnection;
        MS_ConnVersion = ConnectionsPreset[value].connectionVersion;
        ConnVersionInput.text = MS_ConnVersion.ToString();
        MS_protocol = ConnectionsPreset[value].protocolConn;
        switch (MS_protocol)
        {
            case ExitGames.Client.Photon.ConnectionProtocol.Udp:
                ProtocolDropdown.value = 0;
                break;
            case ExitGames.Client.Photon.ConnectionProtocol.Tcp:
                ProtocolDropdown.value = 1;
                break;
            case ExitGames.Client.Photon.ConnectionProtocol.WebSocket:
                ProtocolDropdown.value = 2;
                break;
            case ExitGames.Client.Photon.ConnectionProtocol.WebSocketSecure:
                ProtocolDropdown.value = 3;
                break;
        }
        if (MS_WebCloudConnection)
        {
            print("<color=blue>Preset = ConnectToRegion : " + MS_ConnVersion + " </color>");
        }
        else
        {
            print("<color=blue>Preset = ConnectToMaster : " + MS_adress + ":" + MS_port + " " + MS_ConnVersion + " " + MS_protocol.ToString() + " </color>");
        }

    }

    public void loadConnectionsPresets()
    {
        List<Dropdown.OptionData> ops = new List<Dropdown.OptionData>();
        foreach (MultiplayerProtocol item in ConnectionsPreset)
            ops.Add(new Dropdown.OptionData {text = item.protocolName });
        ConnectionsPresetDropdown.AddOptions(ops);
    }

    void OnDisconnectedFromMasterServer(NetworkDisconnection info){
        Btn_Multiplayer.interactable = false;
        Input_Pseudo.interactable = false;

    }

    void OnConnectedToServer(){ }
    void OnServerInitialized(){}
    void OnDisconnectedFromServer(NetworkDisconnection info){}
    void OnFailedToConnect(NetworkConnectionError error){}
    void OnFailedToConnectToMasterServer(NetworkConnectionError error){
        Btn_Multiplayer.interactable = false;
        Input_Pseudo.interactable = false;

    }
    void OnPlayerConnected(NetworkPlayer player){}
    void OnPlayerDisconnected(NetworkPlayer player){ }
    void OnDisconnectedFromPhoton(){Debug.LogWarning("OnDisconnectedFromPhoton");}
    //PhotonNetwork.insideLobby ?
    public void LeaveRoom() { if (PhotonNetwork.room != null) PhotonNetwork.LeaveRoom(); }
    public void DisconnectFromServer() { if (PhotonNetwork.Server == ServerConnection.GameServer) PhotonNetwork.Disconnect(); }
    public void DisconnectFromMaster() { if (PhotonNetwork.Server == ServerConnection.MasterServer) PhotonNetwork.Disconnect(); }
    //public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)  { print("OnPhotonPlayerDisconnected"); }
    //public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) { print("OnPhotonPlayerConnected"); }

    IEnumerator OnLeftRoom()
    {
        print("OnLeftRoom");
        while (PhotonNetwork.room != null || PhotonNetwork.connected == false) yield return 0;
        Application.LoadLevel("0-MainMenu");

    }
    public void OnJoinedRoom()
    {
        print("OnJoinedRoom");
        Network.sendRate = 100;
        //Camera.main.farClipPlane = 1000; //Main menu set this to 0.4 for a nicer BG 
    }

    public void OnCreatedRoom()
    {
        print("OnCreatedRoom");
    }


    public void onPseudoChange(string value)
    {
        if (value.Length>14) { Input_Pseudo.text = PlayerPrefs.GetString("playerName"); return; }

        PlayerPrefs.SetString("playerName", value);
        PhotonNetwork.playerName = value;
        Input_Pseudo.text = value;
    }
    public void OnConnectedToMaster()
    {
        Btn_Multiplayer.interactable = true;
        Input_Pseudo.interactable = true;
        PhotonNetwork.player.SetPlayerState(PlayerState.inMenu);
        print("PhotonNetwork.JoinLobby()");
        if (PlayerPrefs.GetString("playerName") == "")
            onPseudoChange("Guest" + Random.Range(1, 9999));
        else
            Input_Pseudo.text = PlayerPrefs.GetString("playerName");

        PhotonNetwork.JoinLobby();  // this joins the "default" lobby
    }


}

[System.Serializable]
public struct MultiplayerProtocol
{
    public string protocolName;
    public string protocolAdress;
    public int protocolDefaultPort;
    public ExitGames.Client.Photon.ConnectionProtocol protocolConn;
    public string connectionVersion;
    public bool useWebCloudConnection;
}
