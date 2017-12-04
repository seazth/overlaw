using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ent_Door : ClientEntityLogic
{

    protected override void Start_Client(){ }
    protected override void Start_LocalPlayer(){}
    protected override void Update_Client(){}
    protected override void Update_LocalPlayer(){}

    protected override void Update_Server(){}

    protected override void Update()
    {
        base.Update();
        if (_NES._data.activated && go_mesh.transform.rotation.eulerAngles.y < 90f)
        {
            go_mesh.transform.rotation = Quaternion.Lerp(go_mesh.transform.rotation, Quaternion.Euler(0f, 90f, 0f), 0.1f);
        }
        else if (!_NES._data.activated && go_mesh.transform.rotation.eulerAngles.y > 0f)
        {
            go_mesh.transform.rotation = Quaternion.Lerp(go_mesh.transform.rotation, Quaternion.Euler(0f, 0f, 0f), 0.1f);
        }
    }

}
