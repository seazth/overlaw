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
    public GameObject Panel_Gameboard;
    public GameObject Panel_Help;

    //Multiplayer panel room management
    public RectTransform Rect_RoomslistScrollview;
    public Button gop_BtnRoom;
    Button Btn_SelectedRoom;
    public string SelectedRoom = "";
    public GameObject RoomMenu;
    public GameObject SpecateMenu;
    public static bool captureMouse {
        get { return _captureStatic; }
        set 
        {
            _captureStatic = value;
            if (value) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
    }
    static bool _captureStatic = false;
    public AudioSource MainMusic;


    //=============================================================================================//

    public void Start()
    {
        RoomMenu.SetActive(PhotonNetwork.inRoom);
        Panel_Gameboard.SetActive(false);
        Show_PanelMain();

    }

    public void RefreshRoomList()
    {
        for (int i = 0; i < Rect_RoomslistScrollview.childCount; i++)
        {
            Destroy(Rect_RoomslistScrollview.GetChild(i));
        }
        try
        {
            int row = 0;
            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                Button newRoomBtn = Instantiate<Button>(gop_BtnRoom, Rect_RoomslistScrollview);
                RectTransform rect = newRoomBtn.GetComponent<RectTransform>();
                rect.position = new Vector3(rect.position.x, rect.position.y - 50f * row, 0f);

                if (game.Name == SelectedRoom) { newRoomBtn.interactable = false; Btn_SelectedRoom = newRoomBtn; }
                if (game.IsLocalClientInside) { newRoomBtn.interactable = false; }

                string gamename = game.Name;
                bool alreadyconnected = game.IsLocalClientInside;
                newRoomBtn.onClick.AddListener(() => SelectRoom(newRoomBtn, alreadyconnected, gamename));

                Text[] txts = newRoomBtn.GetComponentsInChildren<Text>();
                txts[0].text = game.Name + (game.IsLocalClientInside ? " [Joined]" : "");
                txts[1].text = game.PlayerCount + "/" + game.MaxPlayers;
                row++;
            }
        }
        catch
        {
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

        // MENU NAVIGUATION KEY
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuPanel.SetActive(!MenuPanel.activeInHierarchy);
            captureMouse = !MenuPanel.activeInHierarchy;
            Panel_Gameboard.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            captureMouse = false;
            if (!PhotonNetwork.inRoom) MenuPanel.SetActive(false);
            else Panel_Gameboard.SetActive(!Panel_Gameboard.activeInHierarchy);
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            captureMouse = !MenuPanel.activeInHierarchy;
            if (!PhotonNetwork.inRoom) MenuPanel.SetActive(false);
            else Panel_Gameboard.SetActive(!Panel_Gameboard.activeInHierarchy);
        }
        
        //MOUSE LOCKING BUG FIX : On maintient le cursor bloqué à chaque update !
        if (captureMouse) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            
        // HELP MENU
        Panel_Help.SetActive(Input.GetKey(KeyCode.F1));

        //MUSIC MANAGEMENT
        InputMusic();
    }

    void InputMusic()
    {
        //ACTIVE/DESACTIVE LA MUSIQUE
        if (Input.GetKeyDown(KeyCode.F5))
        {
            MainMusic.mute = !MainMusic.mute;
        }
        //BAISSE LE VOLUME
        if (Input.GetKeyDown(KeyCode.F6))
        {
            MainMusic.volume-=0.1f;
            if(MainMusic.volume<0) MainMusic.volume =0;
        }
        //AUGMENTE LE VOLUME
        if (Input.GetKeyDown(KeyCode.F7))
        {
            MainMusic.volume += 0.1f;
            if (MainMusic.volume < 0) MainMusic.volume = 100;
        }

    }


    public void OnJoinedRoom() { RoomMenu.SetActive(true); MenuPanel.SetActive(false); }
    public void OnLeftRoom() { RoomMenu.SetActive(false); }
    
    public void CloseGame()
    {
        if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.insideLobby) PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
}
