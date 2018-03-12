using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MNG_MainMenu : MonoBehaviour {

    public GameObject MenuPanel;
    public GameObject Panel_MainSelection;
    public GameObject Panel_Multiplayer;
    public GameObject Panel_Settings;
    public GameObject Panel_Credits;

    //Multiplayer panel room management
    public RectTransform Rect_RoomslistScrollview;
    public Button gop_BtnRoom;
    Button Btn_SelectedRoom;
    public string SelectedRoom = "";
    public GameObject RoomMenu;
    public GameObject SpecateMenu;
    public static bool captureMouse {
        get
        {
            return _captureStatic;
        }
        set
        {
            _captureStatic = value;
            if (value) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
    }
    static bool _captureStatic = false;


    //=============================================================================================//

    public void Start()
    {
        RoomMenu.SetActive(PhotonNetwork.inRoom);
        Show_PanelMain();

    }

    public void RefreshRoomList()
    {
        if (!PhotonNetwork.connected) return;
        int row = 0;
        foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        {
            Button newRoomBtn = Instantiate<Button>(gop_BtnRoom, Rect_RoomslistScrollview);
            RectTransform rect = newRoomBtn.GetComponent<RectTransform>();
            rect.anchoredPosition.Set(rect.anchoredPosition.x, row * 50);

            if(game.Name == SelectedRoom){newRoomBtn.interactable = false;Btn_SelectedRoom = newRoomBtn;}
            if (game.IsLocalClientInside){newRoomBtn.interactable = false;}

            string gamename = game.Name;
            bool alreadyconnected = game.IsLocalClientInside;
            newRoomBtn.onClick.AddListener(() => SelectRoom(newRoomBtn,alreadyconnected, gamename));

            Text[] txts = newRoomBtn.GetComponentsInChildren<Text>();
            txts[0].text = game.Name+(game.IsLocalClientInside?" [Joined]":"" );
            txts[1].text = game.PlayerCount + "/" + game.MaxPlayers;
        }
    }

    public void SelectRoom(Button newRoomBtn, bool alreadyconnected, string gamename)
    {
        if(Btn_SelectedRoom!=null) Btn_SelectedRoom.interactable = !alreadyconnected;
        SelectedRoom = gamename;
        newRoomBtn.interactable = false;
        Btn_SelectedRoom = newRoomBtn;
    }

    public void CreateServer(){PhotonNetwork.CreateRoom("Serveur de "+PhotonNetwork.playerName, new RoomOptions() { MaxPlayers = 16,IsOpen = false }, TypedLobby.Default);}
    public void JoinServer(){
        if (PhotonNetwork.connected && SelectedRoom != null && SelectedRoom != "")
        {
            PhotonNetwork.JoinRoom(SelectedRoom);
            PhotonNetwork.player.SetPlayerState(PlayerState.joiningRoom);
        }
    }
    public void ExitServer() { if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom(false);}


    public void Show_PanelMultiplayer(){HideAll_Panel();Panel_Multiplayer.SetActive(true);}
    public void Show_PanelSettings(){HideAll_Panel();Panel_Settings.SetActive(true);}
    public void Show_PanelCredits(){HideAll_Panel();Panel_Credits.SetActive(true);}
    public void Show_PanelMain(){HideAll_Panel();Panel_MainSelection.SetActive(true);}

    public void HideAll_Panel()
    {
        Panel_MainSelection.SetActive(false);
        Panel_Multiplayer.SetActive(false);
        Panel_Settings.SetActive(false);
        //Panel_Credits.SetActive(false);
    }



    private void OnConnectedToServer() { print("OnConnectedToServer : " + PhotonNetwork.connectionStateDetailed); }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuPanel.SetActive(!MenuPanel.activeInHierarchy);
            captureMouse = !MenuPanel.activeInHierarchy;
        }
        if (Input.GetKeyDown(KeyCode.Tab))  captureMouse = false; 
        if(Input.GetKeyUp(KeyCode.Tab)) captureMouse = !MenuPanel.activeInHierarchy;

        //BUG FIX : On maintient le cursor bloqué à chaque update !
        if (captureMouse) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }




    public string playerPrefabName = "VikCharprefab";
    public string spectatorPrefabName = "Spectator";
    public void Spawn()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)) return;
        // Spawn our local player
        GameObject playerChar;
        Vector2 randpos = UnityEngine.Random.insideUnitCircle * 5f;
        if (PhotonNetwork.player.getTeamID() == 1 || PhotonNetwork.player.getTeamID() == 2)
        {
            bool[] enabledRenderers = new bool[2];
            enabledRenderers[0] = Random.Range(0, 2) == 0;//Axe
            enabledRenderers[1] = Random.Range(0, 2) == 0;//Shield

            object[] objs = new object[1]; // Put our bool data in an object array, to send
            objs[0] = enabledRenderers;

            PhotonNetwork.player.SetPlayerState(PlayerState.inGame);
            playerChar = PhotonNetwork.Instantiate(this.playerPrefabName, MNG_GameManager.getTeams[PhotonNetwork.player.getTeamID()].TeamSpawnLocation + new Vector3(randpos.x, 0f, randpos.y), Quaternion.identity, 0, objs);
        }
        else
        {
            PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, 3);
            PhotonNetwork.player.SetPlayerState(PlayerState.isSpectating);
            playerChar = PhotonNetwork.Instantiate(this.spectatorPrefabName, MNG_GameManager.getTeams[PhotonNetwork.player.getTeamID()].TeamSpawnLocation + new Vector3(randpos.x, 0f, randpos.y), Quaternion.identity, 0);

        }

        PhotonNetwork.player.SetAttribute(PlayerAttributes.HASSPAWNED, true);
        playerChar.GetComponent<MNG_CameraController>().camera = Camera.main;
        Camera.main.transform.parent = playerChar.transform;
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.transform.localEulerAngles = new Vector3(0, 0, 0);
        captureMouse = true;
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " has spawn as " + MNG_GameManager.getTeams[PhotonNetwork.player.getTeamID()].TeamName);
    }

    public void OnJoinedRoom()
    {
        RoomMenu.SetActive(true);
    }
    public void OnLeftRoom()
    {
        RoomMenu.SetActive(false);
    }
    
    public void CloseGame()
    {
        if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.insideLobby) PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
}
