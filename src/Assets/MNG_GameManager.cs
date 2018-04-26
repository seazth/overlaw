using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class MNG_GameManager : Photon.MonoBehaviour
{

    static MNG_GameManager instance;
    public static Team[] getTeams { get { return instance.teamList; } }
    public Team[] teamList;

    // ENDGAME
    public GameObject EndGamePanel;
    public bool GameFinished;
    public int WinnerTeam;

    // START
    public GameObject StartGamePanel;
    public bool WaitingToStart;

    // GAME_ROUND
    public int durationRound = 1000*60*5;
    int minPlayers = 16;
    int durationPreparation = 20000;

    //DISPLAY
    public Text text_Gamestatus;
    public Text text_btn_Ready;
    public GameObject go_checkmark_Ready;
    MNG_MainMenu mng_main;

    public GameObject go_Prison;
    public static Vector3 getPrisonLocation {get{ return instance.go_Prison.transform.position; }}
    private void Awake()
    {
        instance = this;
        mng_main = FindObjectOfType<MNG_MainMenu>();
    }


    public void chooseTeam(int value)
    {
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, value);
            foreach (Team team in teamList) team.BtnCheckmark.SetActive(false);
            teamList[value].BtnCheckmark.SetActive(true);
        }
    }


    void Update ()
    {
        if (!PhotonNetwork.inRoom) return;

        text_Gamestatus.text = (PhotonNetwork.room.GetRoomState()).ToString();

        if ( PhotonNetwork.room.GetAttribute(RoomAttributes.PLAYERSCANSPAWN,false)
            && PhotonNetwork.player.getTeamID()!=0)
        {
            if (!PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED, false))
            {
                mng_main.Spawn();
            }
        }
            
        if ( PhotonNetwork.player.isGameOwner())
        {
            if (PhotonNetwork.room.GetRoomState() == GameState.isWaitingNewGame)
            {
                StartCoroutine(WarmUp());
            }
            else if(PhotonNetwork.room.GetRoomState() == GameState.BeginningRound)
            {
                StartCoroutine(PrepareRound());
            }
            else if (PhotonNetwork.room.GetRoomState() == GameState.RoundRunning )
            {
                UpdateGameInfos();
            }
            else if (PhotonNetwork.room.GetRoomState() == GameState.isGameFinishing)
            {

            }
        }
    }

    public void OnCreatedRoom()
    {
        if(PhotonNetwork.inRoom && !PhotonNetwork.isNonMasterClientInRoom)
        {
            print("OnCreatedRoom() - MY ROOM ");
            PhotonNetwork.room.IsVisible = true;
            PhotonNetwork.room.IsOpen = true;
            PhotonNetwork.room.SetRoomState( GameState.isWaitingNewGame);
            InitRoomAttributes();
        }
    }
    public void switchReadyState()
    {
        if (!PhotonNetwork.inRoom) return;
        bool newVal = !PhotonNetwork.player.GetAttribute<bool>(PlayerAttributes.ISREADY, false);
        PhotonNetwork.player.SetAttribute(PlayerAttributes.ISREADY, newVal);
        text_btn_Ready.text = newVal ? "Ready" : "Not Ready";
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " is now " + (newVal ? "ready" : "not ready"));
        go_checkmark_Ready.SetActive(newVal);

    }

    void OnJoinedRoom()
    {
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " enter the game");
        InitializePlayerAttributes(PhotonNetwork.player);
    }
    void OnLeftRoom()
    {
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " leave the game");
        InitializePlayerAttributes(PhotonNetwork.player);
    }

    void InitRoomAttributes()
    {
        PhotonNetwork.room.SetAttribute(RoomAttributes.PRISONOPEN, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.ALLTHIEFCATCHED, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.IMMOBILIZEALL, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp);
        for (int i = 0; i < teamList.Length; i++)
        {
            PhotonNetwork.room.SetTeamAttribute(i, TeamAttributes.PLAYERSALIVE, 0);
            PhotonNetwork.room.SetTeamAttribute(i, TeamAttributes.PLAYERSCOUNT, 0);
            PhotonNetwork.room.SetTeamAttribute(i, TeamAttributes.ROUNDSWON, 0);
        }
    }
    public void InitializePlayerAttributes(PhotonPlayer Player)
    {
        if (!PhotonNetwork.inRoom) return;
        print("init de " + Player.NickName);
        Player.SetAttribute(PlayerAttributes.HASSPAWNED, false);
        Player.SetAttribute(PlayerAttributes.SCORE, -255);
        Player.SetAttribute(PlayerAttributes.ISIDLE, false);
        Player.SetAttribute(PlayerAttributes.ISREADY, false);
        Player.SetAttribute(PlayerAttributes.ISLAGGY, false);
        Player.SetAttribute(PlayerAttributes.ISROOMADMIN, false);
        Player.SetAttribute(PlayerAttributes.ISIMMOBILIZED, PhotonNetwork.room.GetAttribute(RoomAttributes.IMMOBILIZEALL, false));
        Player.SetPlayerState(PlayerState.isReadyToPlay);
        Player.SetAttribute(PlayerAttributes.TEAM, 0);
    }

    void ReloadRoomScene()
    {
        return; // pour l'instant
        PhotonNetwork.DestroyAll();
        PhotonNetwork.LoadLevel("0-MainMenu");
    }

    public void SetImmobilizeAll(bool value)
    {
        if (!PhotonNetwork.inRoom) return;
        PhotonNetwork.room.SetAttribute(RoomAttributes.IMMOBILIZEALL, value);
        foreach (PhotonPlayer p in PhotonNetwork.playerList) p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, value);
    }

    IEnumerator WarmUp()
    {
        PhotonNetwork.room.SetRoomState(GameState.WarmUp); // au cas ou
        InitRoomAttributes();
        SetImmobilizeAll(false);
        ChatVik.SendRoomMessage("Warmup : Waiting "+ minPlayers+" players minimum to start game");
        PhotonNetwork.room.SetAttribute(RoomAttributes.ROUNDNUMBER, 0);
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, true);
        while (PhotonNetwork.room.PlayerCount< minPlayers) yield return new WaitForSeconds(1f);
        PhotonNetwork.room.SetRoomState(GameState.BeginningRound);
        ReloadRoomScene();
    }

    IEnumerator PrepareRound()
    {
        InitRoomAttributes();
        PhotonNetwork.room.SetRoomState(GameState.PrepareRound); // au cas ou
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, true);
        SetImmobilizeAll(true);
        int timeRoundPreparation = PhotonNetwork.ServerTimestamp;
        
        foreach (PhotonPlayer p in PhotonNetwork.playerList) InitializePlayerAttributes(p);

        ChatVik.SendRoomMessage("Round begin in " + (durationPreparation/1000) + "sec");

        while (PhotonNetwork.ServerTimestamp - timeRoundPreparation < durationPreparation)
        {
            yield return new WaitForSeconds(1f);
            if (PhotonNetwork.room.PlayerCount < minPlayers)
            {
                StartCoroutine(WarmUp());
                yield break;
            }
            int time = durationPreparation/1000 - (PhotonNetwork.ServerTimestamp - timeRoundPreparation - 1000)/1000;
            if (time == 15 || time == 10 || time <=5)
            {
                ChatVik.SendRoomMessage("Round begin in "+time+"sec");
            }
        }
        if ( PhotonNetwork.playerList.Count(s => s.getTeamID() == 1 && s.GetAttribute(PlayerAttributes.HASSPAWNED, false)) < 1
            && PhotonNetwork.playerList.Count(s => s.getTeamID() == 2 && s.GetAttribute(PlayerAttributes.HASSPAWNED, false)) < 1)
        {
            ChatVik.SendRoomMessage("There is no enough players in both team !");
            StartCoroutine(WarmUp());
            yield break;
        }
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        PhotonNetwork.room.SetRoomState(GameState.RoundRunning); // au cas ou
        foreach (PhotonPlayer p in PhotonNetwork.playerList) p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, false);
    }


    public void UpdateGameInfos()
    {
        if (!(PhotonNetwork.player.isGameOwner() && PhotonNetwork.room.GetRoomState() == GameState.RoundRunning)) return;
        // ...
        for (int i = 0;i< teamList.Length;i++)
        {
            PhotonNetwork.room.SetTeamAttribute(i, TeamAttributes.PLAYERSALIVE, PhotonNetwork.playerList.Count(s=>s.getTeamID()==i && s.GetAttribute(PlayerAttributes.HASSPAWNED,false)));
            PhotonNetwork.room.SetTeamAttribute(i, TeamAttributes.PLAYERSCOUNT, PhotonNetwork.playerList.Count(s => s.getTeamID() == i));
        }
        if (PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 && s.GetAttribute(PlayerAttributes.HASSPAWNED, false)).All(s => s.GetAttribute(PlayerAttributes.ISCAPTURED, false)))
        {
            PhotonNetwork.room.SetAttribute(RoomAttributes.ALLTHIEFCATCHED, true);
            ChatVik.SendRoomMessage("Cops catch all thiefs !");
        }
        if (PhotonNetwork.room.GetAttribute(RoomAttributes.ALLTHIEFCATCHED, false) )
        {
            ChatVik.SendRoomMessage("Cops win the round !");
            PhotonNetwork.room.SetTeamAttribute(2, TeamAttributes.ROUNDSWON, PhotonNetwork.room.GetTeamAttribute(1,TeamAttributes.ROUNDSWON,0)+1);
            foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 || s.getTeamID() == 2))
                p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, true);
            PhotonNetwork.room.SetRoomState(GameState.isRoundFinishing);
        }
        else if(PhotonNetwork.ServerTimestamp - PhotonNetwork.room.GetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp) > durationRound)
        {
            ChatVik.SendRoomMessage("Thiefs win the round !");
            PhotonNetwork.room.SetTeamAttribute(1, TeamAttributes.ROUNDSWON, PhotonNetwork.room.GetTeamAttribute(1,TeamAttributes.ROUNDSWON,0)+1);
            foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 || s.getTeamID() == 2))
                p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, true);
            PhotonNetwork.room.SetRoomState(GameState.isRoundFinishing);

        }
    }

    /// <summary>
    /// Ca peut servir....
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
        SceneManager.LoadScene("SCENE NAME"); // custom method to load the new scene by name
        while (true/*newSceneDidNotFinishLoading*/)
        {
            yield return null;
        }
        PhotonNetwork.isMessageQueueRunning = true;
    }
}

[System.Serializable]
public struct Team
{
    public string TeamName;
    public Vector3 TeamSpawnLocation;
    public GameObject BtnCheckmark;
}
