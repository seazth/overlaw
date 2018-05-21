using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone de passage des joueurs voleurs
/// </summary>
public class ThiefZonePoint : Photon.MonoBehaviour
{      // zones accessible aux voleur pour qu'ils puissent gagner des points 

    public string zoneName = "Undefined";
    public string forTaggedGO = "Player";
    public int point = 1;
    MNG_GameManager Manager_game;

    public void Start()
    {
        Manager_game = FindObjectOfType<MNG_GameManager>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == forTaggedGO)
        {
            PhotonPlayer player = other.GetComponent<PhotonView>().owner;
           
            if (player != null
                && other.GetComponent<PhotonView>().isMine
                && player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)
                && player.GetPlayerState() == PlayerState.inGame)
            {
                int team = player.getTeamID();                        //verification de l'équipe du joueur present dans la zone
                if (team == 1)
                {
                    int score = player.GetAttribute<int>(PlayerAttributes.SCORE, 0);
                    int scoreT = PhotonNetwork.room.GetTeamAttribute<int>(team, TeamAttributes.SCORE, 0);// attribution du point au joueurs et à l'équipe
                    score += point;
                    scoreT += point;
                    player.SetAttribute(PlayerAttributes.SCORE, score);
                    PhotonNetwork.room.SetTeamAttribute(team, TeamAttributes.SCORE, scoreT);
                    Manager_game.ZonesList.Remove(gameObject);
                    PhotonNetwork.Destroy(GetComponent<PhotonView>());        //destruction de la zone

                }
            }
        }
    }
}