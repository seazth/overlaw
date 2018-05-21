using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Classe représentant le badge d'un joueur dans le gameboard
/// </summary>
public class PlayerGBInfo : MonoBehaviour {

    public PhotonPlayer player;
    public Text txt_nickname;
    public Text txt_score;
    public Text txt_latence;
    public Text txt_capture;
    public Text txt_state;
}
