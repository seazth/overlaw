using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PrisonTrigger : MonoBehaviour {

    public const string forTaggedGO = "Player";
    public PrisonDoor[] Doors;
    List<PhotonPlayer> playerInZone;
    bool countsHasChanged = false;

    public void Awake()
    {
        playerInZone = new List<PhotonPlayer>();
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == forTaggedGO)
        {
            PhotonPlayer player = other.GetComponent<PhotonView>().owner;
            if (player != null
                && player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)
                && player.GetPlayerState() == PlayerState.inGame)
            {
                

                int tid = player.getTeamID();
                if (tid == 1 || tid == 2)
                {
                    playerInZone.Add(player);
                    countsHasChanged = true;
                    player.SetAttribute(PlayerAttributes.INPRISONZONE, true);
                }
                if (tid == 1 && player.GetAttribute<bool>(PlayerAttributes.ISCAPTURED, false))
                {
                    player.SetAttribute(PlayerAttributes.ISCAPTURED, false);
                    ChatVik.SendRoomMessage(player.NickName + " s'est évadé de prison !");
                }
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
                && player.GetPlayerState() == PlayerState.inGame)
            {
                int tid = player.getTeamID();
                if (tid == 1 || tid == 2)
                {
                    playerInZone.Remove(player);
                    player.SetAttribute(PlayerAttributes.INPRISONZONE, false);
                    countsHasChanged = true;
                }
            }
        }
    }

    public void Update()
    {
        foreach (PhotonPlayer p in playerInZone.ToList())
        {
            if (!PhotonNetwork.playerList.Contains(p)
                || !p.GetAttribute(PlayerAttributes.HASSPAWNED, false)
                || p.GetPlayerState() != PlayerState.inGame
                || !p.GetAttribute(PlayerAttributes.INPRISONZONE, false))
            {
                p.SetAttribute(PlayerAttributes.INPRISONZONE, false);
                playerInZone.Remove(p);
                countsHasChanged = true;
            }
        }

        if (countsHasChanged)
        {
            bool openPrison=false;
            // CECI EST DU LINQ EXPRESSION, APPELEZ MOI SI VOUS VOULEZ DES INFOS DESSUS - RL
            if (!(playerInZone.Any(s=>s.getTeamID() == 2)) && (playerInZone.Any(s => s.getTeamID() == 1)))
                    openPrison = true;
            if(PhotonNetwork.room.GetAttribute(RoomAttributes.PRISONOPEN,false) != openPrison)
                PhotonNetwork.room.SetAttribute(RoomAttributes.PRISONOPEN, openPrison);
            countsHasChanged = false;
        }

    }
}
