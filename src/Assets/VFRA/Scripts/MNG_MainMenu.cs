using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cette classe permet de gérer l'interface du jeu et d'effectuer quelques paramètrages supplémentaires. 
/// </summary>
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
    public static Vector2 cameraRotationSpeed = new Vector2(80, 80);

    //SETTINGSTEXTSHOW
    public GameObject Panel_Settingstext;
    public Text text_settings;
    public float duration_settingshide = 1f;
    public float time_settingsshow;

    //=============================================================================================//

    /// <summary>
    /// On configure quelques paramètres de jeu comme la sensibilité 
    /// de la souris et la gestion de la musique.
    /// </summary>
    public void Start()
    {
        RoomMenu.SetActive(PhotonNetwork.inRoom);
        Panel_Gameboard.SetActive(false);
        Show_PanelMain();
        time_settingsshow = Time.timeSinceLevelLoad;

        if(PlayerPrefs.HasKey("MusicMute")) MainMusic.mute = PlayerPrefs.GetString("MusicMute") == true.ToString();
        if (PlayerPrefs.HasKey("MusicVolume")) MainMusic.volume = PlayerPrefs.GetFloat("MusicVolume");
        if (PlayerPrefs.HasKey("MouseSensibility")) cameraRotationSpeed = new Vector2(PlayerPrefs.GetFloat("MouseSensibility"), PlayerPrefs.GetFloat("MouseSensibility"));
    }


    /// <summary>
    /// Rafraichis la liste des rooms dans l'interface du jeu
    /// </summary>
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

    /// <summary>
    /// Selection d'une room dans l'interface.
    /// </summary>
    /// <param name="newRoomBtn"></param>
    /// <param name="alreadyconnected"></param>
    /// <param name="gamename"></param>
    public void SelectRoom(Button newRoomBtn, bool alreadyconnected, string gamename)
    {
        if(Btn_SelectedRoom!=null) Btn_SelectedRoom.interactable = !alreadyconnected;
        SelectedRoom = gamename;
        newRoomBtn.interactable = false;
        Btn_SelectedRoom = newRoomBtn;
    }

    /// <summary>
    /// Creation de serveur
    /// </summary>
    public void CreateServer(){PhotonNetwork.CreateRoom("Serveur de "+PhotonNetwork.playerName, new RoomOptions() { MaxPlayers = 16,IsOpen = false }, TypedLobby.Default);}

    /// <summary>
    /// Tentative de connection à une room
    /// </summary>
    public void JoinServer(){
        if (PhotonNetwork.connected && SelectedRoom != null && SelectedRoom != "")
        {
            PhotonNetwork.JoinRoom(SelectedRoom);
            PhotonNetwork.player.SetPlayerState(PlayerState.joiningRoom);
        }
    }

    /// <summary>
    /// Quitter la room
    /// </summary>
    public void ExitServer() { if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom(false);}

    // GESTION DES PANNEAUX D'AFFICHAGE
    public void Show_PanelMultiplayer(){HideAll_Panel();Panel_Multiplayer.SetActive(true);}
    public void Show_PanelSettings(){HideAll_Panel();Panel_Settings.SetActive(true);}
    public void Show_PanelMain(){HideAll_Panel();Panel_MainSelection.SetActive(true);}
    public void HideAll_Panel()
    {
        Panel_MainSelection.SetActive(false);
        Panel_Multiplayer.SetActive(false);
        Panel_Settings.SetActive(false);
    }

    private void OnConnectedToServer() { print("OnConnectedToServer : " + PhotonNetwork.connectionStateDetailed); }

    /// <summary>
    /// Quelques actions possibles pour le joueur
    /// </summary>
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

        //AFFICHAGE DES CONFIGURATIONS
        InputsSettings();

        //MONTRE LE PANNEAU DES CREDITS
        if (Input.GetKey(KeyCode.F12)) Panel_Credits.SetActive(true);
        else Panel_Credits.SetActive(false);

        // HIDESHOW SETTINGS INPUTS
        if (Panel_Settingstext.activeInHierarchy && Time.timeSinceLevelLoad - time_settingsshow > duration_settingshide)
        {
            HideSettingsPanel();
        }
    }

    public void ShowSettingsText(string text)
    {
        Panel_Settingstext.SetActive(true);
        text_settings.text = text;
        time_settingsshow = Time.timeSinceLevelLoad;

    }
    public void HideSettingsPanel()
    {
        Panel_Settingstext.SetActive(false);
    }

    /// <summary>
    /// Gestion de la musique
    /// </summary>
    void InputMusic()
    {
        //ACTIVE/DESACTIVE LA MUSIQUE
        if (Input.GetKeyDown(KeyCode.F5))
        {
            MainMusic.mute = !MainMusic.mute;
            PlayerPrefs.SetString("MusicMute", MainMusic.mute.ToString());
        }
        //BAISSE LE VOLUME
        if (Input.GetKeyDown(KeyCode.F6))
        {
            MainMusic.volume -= 0.1f;
            if (MainMusic.volume < 0) MainMusic.volume = 0;
            ShowSettingsText("Music volume : " + MainMusic.volume);
            PlayerPrefs.SetFloat("MusicVolume", MainMusic.volume);
        }
        //AUGMENTE LE VOLUME
        if (Input.GetKeyDown(KeyCode.F7))
        {
            MainMusic.volume += 0.1f;
            if (MainMusic.volume > 1) MainMusic.volume = 1;
            ShowSettingsText("Music volume : " + MainMusic.volume);
            PlayerPrefs.SetFloat("MusicVolume", MainMusic.volume);
        }

    }

    /// <summary>
    /// Gestion du paramètrage de la sensibilité de la souris
    /// </summary>
    void InputsSettings()
    {

        //AUGMENTE LA SENSIBILITE DE LA SOURIS
        if (Input.GetKeyDown(KeyCode.F8))
        {
            cameraRotationSpeed.x -= 10;
            cameraRotationSpeed.y -= 10;
            ShowSettingsText("Mouse sensibility : " + cameraRotationSpeed.x);
            PlayerPrefs.SetFloat("MouseSensibility", cameraRotationSpeed.y);
        }
        //DIMINUE LA SENSIBILITE DE LA SOURIS
        if (Input.GetKeyDown(KeyCode.F9))
        {
            cameraRotationSpeed.x += 10;
            cameraRotationSpeed.y += 10;
            ShowSettingsText("Mouse sensibility : " + cameraRotationSpeed.x);
            PlayerPrefs.SetFloat("MouseSensibility", cameraRotationSpeed.y);
        }

    }

    public void OnJoinedRoom() { RoomMenu.SetActive(true); MenuPanel.SetActive(false); }
    public void OnLeftRoom() { RoomMenu.SetActive(false); }
    
    /// <summary>
    /// Fonction de fermeture de l'application
    /// </summary>
    public void CloseGame()
    {
        if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.insideLobby) PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
}
