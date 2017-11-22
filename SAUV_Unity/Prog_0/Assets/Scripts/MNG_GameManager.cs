using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MNG_GameManager : MonoBehaviour {

    public static MNG_GameManager mainInstance { get; private set; }
    public static bool captureInput { get;private set; }
    public Camera cam_StdCamera;
    public Camera cam_ActiveCamera { get; private set; }

    public bool inMenu = false;
    public bool inGame = false;

    public string pseudo { get; set; }
    public int serverport { get; set; }
    public string serverip { get; set; }
    public string password { private get; set; }


    void Start()
    {
        //
        pseudo = "Toto";
        serverport = 7777;
        serverip = "localhost";
        password = "";
        //
        cam_ActiveCamera = cam_StdCamera;
        setInputCapture(false);
        if (mainInstance==null)
            mainInstance = this;
        MNG_GameManager.mainInstance.inGame = false;
    }
    void Update () {
        updateInput();
        // ...
    }

    public static void setInputCapture(bool value)
    {
        if (captureInput != value)
        {
            captureInput = value;
            Main_Canvas.show_Menu(!value);
            print(value ? "[CAPTURING ON]" : "[CAPTURING OFF]");
        }
    }
    public static void setActiveCamera(Camera newCam,bool desactiveOld = true) {
        Main_Canvas.set_WorldCamera(newCam);
        if (desactiveOld) { mainInstance.cam_ActiveCamera.gameObject.SetActive(false); }
        mainInstance.cam_ActiveCamera = newCam;
        newCam.gameObject.SetActive(true);
        print("<MAIN CAMERA CHANGED>");
    }
    public static void setStdCameraAsActive(bool desactiveOld)
    {
        Main_Canvas.show_Menu(false);
        setActiveCamera(mainInstance.cam_StdCamera, desactiveOld);
    }

    public void OnResumeButton(){if (inGame && inMenu) {MNG_GameManager.setInputCapture(true);}}

    void updateInput()
    {
        if (captureInput && ( Input.GetKeyDown("escape") ))
        {
            setInputCapture(false);
        }
        else if (!captureInput && Input.GetMouseButton(0) && !inMenu
            && ((Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height)
            || (Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width))
            )
        {
            setInputCapture(true);
        }
        else if(Input.GetKeyDown("escape") && inGame && inMenu)
        {
            MNG_GameManager.setInputCapture(true);
        }

    }





}
