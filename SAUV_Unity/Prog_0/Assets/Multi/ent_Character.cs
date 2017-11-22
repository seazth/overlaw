using UnityEngine;
using System.Collections;

public class ent_Character : ClientEntityLogic
{
    public float mov_speed = 5f;
    public GameObject gop_camera;

    protected override void Start()
    {
        base.Start();
        if (_NES._entityType == EntityType.LocalPlayer)
        {
            gop_camera = Instantiate<GameObject>(gop_camera, gameObject.transform);
            MNG_GameManager.setActiveCamera(gop_camera.GetComponent<Camera>());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_NES._entityType == EntityType.LocalPlayer)
            MNG_GameManager.setStdCameraAsActive(false);
    }

    protected override void UpdateLocalPlayer()
    {
        if (MNG_GameManager.captureInput)
        {
            if (Input.GetKey(KeyCode.Z)) transform.Translate(Vector3.forward * mov_speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.back * mov_speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.Q)) transform.Translate(Vector3.left * mov_speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * mov_speed * Time.deltaTime);
            float roty = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * MNG_InputController.sensitivityX;
            transform.localEulerAngles = new Vector3(0, roty, 0);
        }
    }
}
