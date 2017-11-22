using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CTRL_Player : MonoBehaviour
{
    public static Camera cam_PlayerCamera { get; private set; }
    private static Actor go_ControlledActor { get; set; }

    public static void setControlledActor(Actor actor)
    {
        go_ControlledActor = actor;
        //create&setCamera
        cam_PlayerCamera = GameObject.Instantiate(GlobalAssets.mainInstance.gop_playerCameraForDude, go_ControlledActor.transform, false)
            .GetComponent<Camera>();
        if (cam_PlayerCamera != null && go_ControlledActor != null)
        {
            MNG_GameManager.setActiveCamera(cam_PlayerCamera);
            print("[PlayerController] Control on : " + go_ControlledActor.name);
        }
        else
        {
            throw new System.Exception("[ERROR] Le controller a mal initialisé un joueur !");
        }
    }

    void Update()
    {
        if (MNG_GameManager.captureInput && go_ControlledActor != null) { go_ControlledActor.UpdateInput(); }
    }


    public void OnDisable()
    {
        if (MNG_GameManager.mainInstance.cam_ActiveCamera == cam_PlayerCamera)
            MNG_GameManager.setStdCameraAsActive(false);
    }



}
