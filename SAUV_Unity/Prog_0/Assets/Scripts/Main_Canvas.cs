using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main_Canvas : MonoBehaviour {

    private static Main_Canvas self { get; set; }
    public GameObject go_CrossAir;
    public GameObject go_Menu;
    public Text ui_text_clientsconnected;
    public Text ui_serverconsole ;
    public GameObject go_btn_connect;
    public GameObject go_btn_disconnect;


    public GameObject go_btn_startServer;
    public GameObject go_btn_stopServer;

    void Start ()
    {
        self = this;
        go_Menu.SetActive(true);
        MNG_GameManager.mainInstance.inMenu = true;
        set_switchServerButton(true);
        set_switchConnectButton(true);
    }
    void Update () {}
    public static void show_Menu(bool val)
    { 
        Cursor.visible = val;
        Cursor.lockState = (val ? CursorLockMode.None : CursorLockMode.Locked);
        show_Crosshair(!val);
        self.go_Menu.SetActive(val);
        MNG_GameManager.mainInstance.inMenu = val;
    }
    public static void set_WorldCamera(Camera camera) { self.GetComponent<Canvas>().worldCamera = camera; }
    public static void show_Crosshair(bool val){self.go_CrossAir.SetActive(val);}
    public static void add_serverlog(string val) { self.ui_serverconsole.text = "> "+val+"\n"+ self.ui_serverconsole.text; }
    public static void set_ClientsInfo(string val) { self.ui_text_clientsconnected.text = val; }
    public static void set_switchConnectButton(bool val) { self.go_btn_connect.GetComponent<Button>().interactable = val; self.go_btn_disconnect.GetComponent<Button>().interactable = !val; }
    public static void set_switchServerButton(bool val) {
        self.go_btn_startServer.GetComponent<Button>().interactable = val;
        self.go_btn_stopServer.GetComponent<Button>().interactable = !val; }
}
