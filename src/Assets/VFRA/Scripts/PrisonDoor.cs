using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonDoor : MonoBehaviour {
    public bool Opened = false;
    static Quaternion quatDest = Quaternion.Euler(new Vector3(0, 120, 0));
    static Quaternion quatSrc = Quaternion.Euler(new Vector3(0, 0, 0));

    void Update ()
    {
        if (!PhotonNetwork.inRoom) return;
        bool Opened = PhotonNetwork.room.GetAttribute(RoomAttributes.PRISONOPEN,false);
        if (Opened && transform.rotation.y < quatDest.eulerAngles.y)
            transform.rotation = Quaternion.Lerp(transform.rotation, quatDest, 0.1f);
        else if(!Opened && transform.rotation.y > quatSrc.eulerAngles.y)
            transform.rotation = Quaternion.Lerp(transform.rotation, quatSrc, 0.1f);
    }
}
