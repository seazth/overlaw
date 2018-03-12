using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : Photon.MonoBehaviour {
    public string zoneName = "Undefined";
    public string forTaggedGO = "Player";
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == forTaggedGO )
        {
            PhotonPlayer player = other.GetComponent<PhotonView>().owner;
            if (player != null 
                && player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED,false)
                && player.GetPlayerState() == PlayerState.inGame)
            {
                print("Player " + other.name + " enter " + zoneName);
                player.SetAttribute(PlayerAttributes.INZONE, zoneName);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == forTaggedGO)
        {
            PhotonPlayer player = other.GetComponent<PhotonView>().owner;
            if (player != null
                && player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)
                && player.GetAttribute<string>(PlayerAttributes.INZONE, "") == zoneName
                && player.GetPlayerState() == PlayerState.inGame)
            {
                print("Player " + other.name + " left " + zoneName);
                player.SetAttribute(PlayerAttributes.INZONE, "");
            }
        }
    }
}
