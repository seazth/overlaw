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
    public PlayerGBInfo[] playersGBInfoList;

    //ZONE VAL'
    public GameObject[] ThiefZonesPosition;
    public int max_Zones = 4;
    public List<GameObject> ZonesList;
    int ZoneActuel_Index =0;

    // ENDGAME
    public GameObject EndGamePanel;
    public bool GameFinished;
    public int WinnerTeam;

    // START
    public GameObject StartGamePanel;
    public bool WaitingToStart;

    // GAME_ROUND
    public int durationRound = 180000;
    public int minPlayers = 4;
    public int durationRoundPreparation = 10000;
    public int durationRoundFinalize = 5000;

    //DISPLAY
    public Text text_Gamestatus;
    public Text text_RoundNumber;
    public Text text_Timer;
    public Text text_btn_Ready;
    public GameObject go_checkmark_Ready;
    MNG_MainMenu mng_main;

    //PRISON
    public GameObject go_Prison;
    public static Vector3 getPrisonLocation {get{ return instance.go_Prison.transform.position; }}

    //GAMEBOARD
    public GameObject content_Gameboard;
    public GameObject gop_Gameboard_playerinfo;

    public static PlayerInitializer PlayerAvatar;
    public GameObject MainMenuCameraRoot;

    //SCORING
    public int PointGenereParPrisonnier = 5;

    private void Awake()
    {
        instance = this;
        mng_main = FindObjectOfType<MNG_MainMenu>();
        ZonesList = new List<GameObject>();
    }

    /// <summary>
    /// Selection d'une equipe
    /// </summary>
    /// <param name="value">Id Equipe</param>
    public void chooseTeam(int value)
    {
        if (PhotonNetwork.inRoom 
            && 
                (PhotonNetwork.room.GetRoomState()==GameState.WarmUp) 
                 || 
                ( !(PhotonNetwork.player.GetPlayerState()== PlayerState.inGame)
                && !PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED,false) 
                && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISREADY, false)))
        {
            PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, value);
            foreach (Team team in teamList) {
                if (team.Btn_join != null) team.Btn_join.interactable = true;
                if (team.BtnCheckmark != null) team.BtnCheckmark.SetActive(false);
            }
            teamList[value].BtnCheckmark.SetActive(true);
            teamList[value].Btn_join.interactable = false;
        }
    }

    /// <summary>
    /// Check si les deux equipes ont des joueurs prêt au jeu
    /// </summary>
    /// <returns></returns>
    bool isMinimumPlayersReady()
    { // IF NEED MASTERCLIENT LIKE KF Game = PhotonNetwork.playerList.AsEnumerable().Any(c => c.IsMasterClient && c.GetAttribute(PlayerAttributes.ISREADY, false))
        return PhotonNetwork.playerList.AsEnumerable().Count(c => ((c.getTeamID() == 1 || c.getTeamID() == 2) && c.GetAttribute(PlayerAttributes.ISREADY, false))) >= minPlayers;
    }

    void Update ()
    {
        if (!PhotonNetwork.inRoom) return;

        //MISE A JOUR AFFICHAGE DU STATUS DE CONNECTION A LA ROOM  
        text_Gamestatus.text = (PhotonNetwork.room.GetRoomState()).ToString();

        if ( PhotonNetwork.room.GetAttribute(RoomAttributes.PLAYERSCANSPAWN,false)
            && PhotonNetwork.player.getTeamID()!=0
            && !PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED, false))
        {
            SpawnMyAvatar();
        }

        // ON CHECK LA PRESENCE D'UN MASTER PLAYER // UNIQUEMENT SI CE N'EST PAS EN COURS // REVIENS A : PhotonNetwork.isNonMasterClientInRoom
        if (!PhotonNetwork.playerList.AsEnumerable().Any(a=>a.isGameOwner()) && !PhotonNetwork.room.GetAttribute(RoomAttributes.CHANGEINGMASTER, false))
        {
            // ON LOCK LA METHODE POUR EVITER QUE TOUT LES JOUEURS CHANGENT L'HOST !
            PhotonNetwork.room.SetAttribute(RoomAttributes.CHANGEINGMASTER, true);
            //On le designe automatiquement par le premier joueur de la liste
            PhotonNetwork.SetMasterClient(PhotonNetwork.playerList[0]);
            // ON DELOCK
            PhotonNetwork.room.SetAttribute(RoomAttributes.CHANGEINGMASTER, false);
        }

        // CONTROLLER DE STATUT DE JEU = GEREE PAR LE PLAYERMASTER
        if ( PhotonNetwork.player.isGameOwner())
        {
            if (PhotonNetwork.room.GetRoomState() == GameState.isWaitingNewGame)
            {
                ChatVik.SendRoomMessage("Warmup : Waiting " + minPlayers + " players minimum to start game");
                StartCoroutine(WarmUp());
            }
            else if(PhotonNetwork.room.GetRoomState() == GameState.BeginningRound)
            {
                StartCoroutine(PrepareRound());
            }
            else if (PhotonNetwork.room.GetRoomState() == GameState.RoundRunning )
            {
                ManageWaypoints();
                UpdateGameInfos();
            }
        }

        //FONCTION DE DEBUG POUR FAIRE RESPAWN LE JOUEUR
        if (Input.GetKeyDown(KeyCode.F10)){
            photonView.RPC("rpc_UnspawnPlayerAvatar", PhotonTargets.All, new object[] { PhotonNetwork.player});
        }
        UpdateTimer();
    }

    /// <summary>
    /// MAJ du Timer de fin de round
    /// </summary>
    public void UpdateTimer()
    {
        int timeleft = (int)(PhotonNetwork.ServerTimestamp - PhotonNetwork.room.GetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp))/1000;
        if (PhotonNetwork.room.GetRoomState() == GameState.RoundRunning)
            text_Timer.text = (timeleft/60) +"m "+(timeleft%60) +"s";
        else
            text_Timer.text = "-";
    }

    /// <summary>
    /// Gestion des points de passages
    /// </summary>
    public void ManageWaypoints()
    {
        if (ZonesList.Count < max_Zones)
        {
            if (ZoneActuel_Index >= ThiefZonesPosition.Length) ZoneActuel_Index = 0;
            ZonesList.Add(PhotonNetwork.Instantiate("Thief_zone"
                , instance.ThiefZonesPosition[ZoneActuel_Index].transform.position
               , Quaternion.identity, 0));
            ZoneActuel_Index++;
        }
    }

    /// <summary>
    /// Une fois la room créé, on la rend visible et accessible
    /// </summary>
    public void OnCreatedRoom()
    {
        if(PhotonNetwork.inRoom && !PhotonNetwork.isNonMasterClientInRoom)
        {
            PhotonNetwork.room.IsVisible = true;
            PhotonNetwork.room.IsOpen = true;
            PhotonNetwork.room.SetRoomState( GameState.isWaitingNewGame);
            InitRoomAttributes(true);
            InvokeRepeating("RefreshGameBoard", 0f, 1f);
        }
    }

    /// <summary>
    /// Mets a jour du bouton Ready
    /// </summary>
    public void UpdateReadyBtnState()
    {
        bool value = PhotonNetwork.player.GetAttribute<bool>(PlayerAttributes.ISREADY, false);
        text_btn_Ready.text = value ? "Ready" : "Not Ready";
        
        go_checkmark_Ready.SetActive(value);
    }

    /// <summary>
    /// Mettre un joueur en status "prêt"
    /// </summary>
    public void switchReadyState()
    {
        if (!PhotonNetwork.inRoom
            && !PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED, false)
            && PhotonNetwork.player.GetPlayerState()!=PlayerState.inGame)
            return;
        bool newVal = !PhotonNetwork.player.GetAttribute<bool>(PlayerAttributes.ISREADY, false);
        PhotonNetwork.player.SetAttribute(PlayerAttributes.ISREADY, newVal);
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " is now " + (newVal ? "ready" : "not ready"));
        UpdateReadyBtnState();
    }
    void OnJoinedRoom()
    {
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " enter the game");
        InitPlayerAttributes(PhotonNetwork.player,false);
        
        PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, 0);
        InvokeRepeating("RefreshGameBoard", 0f, 1f);
    }
    void OnLeftRoom()
    {
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " leave the game");
        PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, 0);
        InitPlayerAttributes(PhotonNetwork.player,false);
    }
    void ReloadRoomScene()
    {
        return; // pour l'instant
        PhotonNetwork.DestroyAll();
        PhotonNetwork.LoadLevel("0-MainMenu");
    }

    /// <summary>
    /// Ici on raffraichis, le gameboard
    /// </summary>
    void RefreshGameBoard()
    {
        PhotonPlayer[] plist = PhotonNetwork.playerList; // BUGFIX CAR LA LISTE PEUT CHANGER EN TRAITEMENT

        // ACTUALISE L'ENTETE DU GAMEBOARD, AFIN DE SAVOR LE NUMERO DU ROUND ACTUEL
        if (PhotonNetwork.room.GetRoomState() == GameState.WarmUp) text_RoundNumber.text = "Warmup";
        else text_RoundNumber.text = PhotonNetwork.room.GetRoomState().ToString() + " " + PhotonNetwork.room.GetAttribute(RoomAttributes.ROUNDNUMBER, 0);
        
        // RAFRAICHISSEMENT DU GAMEBOARD DES JOUEURS
        List<PlayerGBInfo> newList = playersGBInfoList.ToList();
        foreach (PlayerGBInfo PGBI in playersGBInfoList.ToList())
        {
            if (!plist.Contains(PGBI.player))
            { // remove 
                Destroy(PGBI);
                newList.Remove(PGBI);
            }
            else//refresh
            {
                PGBI.txt_state.text = getPlayerStrState(PGBI.player);
                PGBI.txt_latence.text = "-";
                PGBI.txt_score.text = PGBI.player.GetAttribute(PlayerAttributes.SCORE, 0).ToString();
                PGBI.txt_capture.text = PGBI.player.GetAttribute(PlayerAttributes.CAPTURESCORE, 0).ToString();
                PGBI.gameObject.SetActive(false);
            }
        }
        playersGBInfoList = newList.ToArray();
        //
        foreach (PhotonPlayer player in plist)
        {
            if (!playersGBInfoList.ToList().Any(w => w.player == player)) // add
            {
                PlayerGBInfo newone = Instantiate(gop_Gameboard_playerinfo, content_Gameboard.transform).GetComponent<PlayerGBInfo>();
                newone.txt_nickname.text = player.NickName;
                newone.player = player;
                newList.Add(newone);

                newone.txt_state.text = getPlayerStrState(newone.player);
                newone.txt_latence.text = "-";
                newone.txt_score.text = newone.player.GetAttribute(PlayerAttributes.SCORE, 0).ToString();
                newone.txt_capture.text = newone.player.GetAttribute(PlayerAttributes.CAPTURESCORE, 0).ToString();
                newone.gameObject.SetActive(false);
            }
        }
        playersGBInfoList = newList.ToArray();
        // ORDERING
        int j = 0;
        foreach (PlayerGBInfo item in playersGBInfoList.Where(w => w.player.getTeamID() == 1).OrderByDescending(o => o.player.GetAttribute(PlayerAttributes.SCORE, 0)))
        {
            item.gameObject.SetActive(true);
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(135f, -12 + (-22f * j), 0f);
            j++;
        }
        j = 0;
        foreach (PlayerGBInfo item in playersGBInfoList.Where(w => w.player.getTeamID() == 2).OrderByDescending(o => o.player.GetAttribute(PlayerAttributes.SCORE, 0)))
        {
            item.gameObject.SetActive(true);
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(135f + 276.5f, -12 + (-22f * j), 0f);
            j++;
        }

        // AFFICHAGE DU NOMBRE DE MANCHE GAGNEE POUR CHAQUE EQUIPE
        teamList[1].txt_roundswon.text = PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.ROUNDSWON, 0).ToString();
        teamList[2].txt_roundswon.text = PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.ROUNDSWON, 0).ToString();

        // AFFICHAGE DU SCORE PAR EQUIPE 
        teamList[1].txt_tscore.text = PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.SCORE, 0).ToString();
        teamList[2].txt_tscore.text = PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.SCORE, 0).ToString();
    }
    string getPlayerStrState(PhotonPlayer player)
    {
        return (player.GetAttribute(PlayerAttributes.ISCAPTURED, false) ? "Captured" : player.GetPlayerState() == PlayerState.inGame ? "ig" : player.GetAttribute(PlayerAttributes.ISREADY, false) ? "Ready" : "-");
    }

    //============================================================================================//

    /// <summary>
    /// Cette méthode vérifie l'état de jeu dans une session en cours.
    /// </summary>
    public void UpdateGameInfos()
    {
        // UPDATE DE LA PARTIE UNIQUEMENT POUR LE MASTERPLAYER
        if (!(PhotonNetwork.player.isGameOwner() && PhotonNetwork.room.GetRoomState() == GameState.RoundRunning)) return;
        // ON CHECK SI LE TIMER EST ATTEInt OU TOUT LES THIEFS SONT CAPTUREE
        if (PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 && s.GetAttribute(PlayerAttributes.HASSPAWNED, false)).All(s => s.GetAttribute(PlayerAttributes.ISCAPTURED, false)))
        {
            PhotonNetwork.room.SetAttribute(RoomAttributes.ALLTHIEFCATCHED, true);
            ChatVik.SendRoomMessage("COPS CATCH ALL THIEVES AND WIN THE ROUND");
            PhotonNetwork.room.SetTeamAttribute(2, TeamAttributes.ROUNDSWON, PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.ROUNDSWON, 0) + 1);
            foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 || s.getTeamID() == 2))
                p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, true);
            StartCoroutine(FinalizeRound(2));

        }
        else if (PhotonNetwork.ServerTimestamp - PhotonNetwork.room.GetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp) > durationRound)
        {
            int tid_winner = CalculateScores();
            switch (tid_winner)
            {
                case 1: 
                    ChatVik.SendRoomMessage("THIEVES WIN THE ROUND");
                    PhotonNetwork.room.SetTeamAttribute(tid_winner, TeamAttributes.ROUNDSWON, PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.ROUNDSWON, 0) + 1);
                    break;
                case 2:
                    ChatVik.SendRoomMessage("COPS WIN THE ROUND");
                    PhotonNetwork.room.SetTeamAttribute(tid_winner, TeamAttributes.ROUNDSWON, PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.ROUNDSWON, 0) + 1);
                    break;
                case 0:
                    ChatVik.SendRoomMessage("DRAW !");
                    break;
            }
            
            foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(s => s.getTeamID() == 1 || s.getTeamID() == 2))
                p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, true);
            StartCoroutine(FinalizeRound(tid_winner));
        }
    }

    /// <summary>
    /// Calcul le score en fin de round et retourne l'id de l'équipe gagnante.
    /// </summary>
    /// <returns></returns>
    public int CalculateScores()
    {
        // On ajoute un score a chaque joueur capturé en fin de manche
        foreach (PhotonPlayer player in PhotonNetwork.playerList.Where(x=>x.GetAttribute(PlayerAttributes.ISCAPTURED,false)))
            PhotonNetwork.room.AddTeamScore(2, PointGenereParPrisonnier);

        print("SCORE : COPS : " + PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.SCORE, 0));
        print("SCORE : THIEVES : " + PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.SCORE, 0));
        if (PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.SCORE, 0) > PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.SCORE, 0)) return 1;
        else if (PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.SCORE, 0) < PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.SCORE, 0)) return 2;
        else return 0;
    }

    /// <summary>
    /// Coroutine de jeu : Echauffement
    /// </summary>
    /// <returns></returns>
    IEnumerator WarmUp()
    {
        //INIT DES VARIABLES DE WARMUP
        PhotonNetwork.room.SetRoomState(GameState.WarmUp); // au cas ou
        InitRoomAttributes(true);
        SetImmobilizeAll(false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, true);

        // RESET AFFICHAGE DES EQUIPËS
        teamList[1].txt_roundswon.text = "-";
        teamList[2].txt_roundswon.text = "-";
        teamList[1].txt_tscore.text = "-";
        teamList[2].txt_tscore.text = "-";

        //ECOUTE DU NOMBRE DE JOUEUR READY
        while (!isMinimumPlayersReady()) yield return new WaitForSeconds(1f);

        // CREATION DU PREMIER ROUND
        PhotonNetwork.room.SetRoomState(GameState.BeginningRound);
        ReloadRoomScene();
    }

    /// <summary>
    /// Coroutine de jeu : Preparation au lancement d'une nouvelle session de jeu
    /// </summary>
    /// <returns></returns>
    IEnumerator PrepareRound()
    {
        // INIT DES VARIABLES DE NOUVEAU ROUND
        InitRoomAttributes(false);
        PhotonNetwork.room.SetRoomState(GameState.PrepareRound); // au cas ou
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, true);
        SetImmobilizeAll(true);

        //DEPOP DES JOUEURS
        foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(w => (w.getTeamID() == 1 || w.getTeamID() == 2) && w.GetAttribute(PlayerAttributes.HASSPAWNED, false)))
        {
            call_UnspawnPlayerAvatar(p);
        }

        // COMPTE A REBOURG AVANT LE DEBUT DU ROUND
        int timeRoundPreparation = PhotonNetwork.ServerTimestamp;
        foreach (PhotonPlayer p in PhotonNetwork.playerList) InitPlayerAttributes(p,true);
        ChatVik.SendRoomMessage("New round begin in " + (durationRoundFinalize/1000) + "sec");

        while (PhotonNetwork.ServerTimestamp - timeRoundPreparation < durationRoundFinalize)
        {
            yield return new WaitForSeconds(1f);
            int time = durationRoundFinalize/1000 - (PhotonNetwork.ServerTimestamp - timeRoundPreparation - 1000)/1000;
            if (time == 15 || time == 10 || time <=5) ChatVik.SendRoomMessage("New round begin in "+time+"sec"); 
        }

        // CHECK FINAL : IL Y A ASSEZ DE JOUEUR POUR JOUER
        if ( PhotonNetwork.playerList.Count(s => s.getTeamID() == 1) < 1
            && PhotonNetwork.playerList.Count(s => s.getTeamID() == 2) < 1)
        {
            ChatVik.SendRoomMessage("There is no players in both team ! Restart Warmup ...");
            StartCoroutine(WarmUp());
            yield break;
        }

        // INIT DES JOUEURS AU ROUND
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        print(durationRound);
        PhotonNetwork.room.SetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp);
        PhotonNetwork.room.SetRoomState(GameState.RoundRunning);
        PhotonNetwork.room.SetAttribute(RoomAttributes.ROUNDNUMBER, PhotonNetwork.room.GetAttribute(RoomAttributes.ROUNDNUMBER, 0)+1);
        ChatVik.SendRoomMessage("New round : "+ PhotonNetwork.room.GetAttribute(RoomAttributes.ROUNDNUMBER, 0));
        SetImmobilizeAll(false);    
    }

    /// <summary>
    /// Coroutine de jeu : Fincalisation d'un round avec changement de camps des joueurs
    /// </summary>
    /// <param name="winnerTeamId"></param>
    /// <returns></returns>
    IEnumerator FinalizeRound(int winnerTeamId)
    {
        // INIT DES VARIABLES DE NOUVEAU ROUND
        InitRoomAttributes(false);
        PhotonNetwork.room.SetRoomState(GameState.isRoundFinishing); // au cas ou
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        SetImmobilizeAll(true);
        for (int i = 0; i < ZonesList.Count; i++)
        {
            PhotonNetwork.Destroy(ZonesList[0].GetPhotonView());
            ZonesList.RemoveAt(0);
        }
        //SETUP WINNER PANEL
        if (PhotonNetwork.room.GetTeamAttribute(winnerTeamId, TeamAttributes.ROUNDSWON,0) == 3 )
        {
            StartCoroutine(FinalizeGame(winnerTeamId));
            yield break;
        }

        //RESET DU SCORE DES EQUIPES
        PhotonNetwork.room.SetTeamAttribute(1, TeamAttributes.SCORE, 0);
        PhotonNetwork.room.SetTeamAttribute(2, TeamAttributes.SCORE, 0);

        // AFFICHE L'EQUIPE GAGNANTE
        photonView.RPC("rpc_callWinner", PhotonTargets.All, new object[] { winnerTeamId , false });

        // COMPTE A REBOURG AVANT LE DEBUT DU ROUND
        int timeRoundPreparation = PhotonNetwork.ServerTimestamp;
        foreach (PhotonPlayer p in PhotonNetwork.playerList) InitPlayerAttributes(p, false);
        ChatVik.SendRoomMessage("Revert teams in " + (durationRoundFinalize / 1000) + "sec");

        while (PhotonNetwork.ServerTimestamp - timeRoundPreparation < durationRoundFinalize)
        {
            yield return new WaitForSeconds(1f);
            int time = durationRoundFinalize / 1000 - (PhotonNetwork.ServerTimestamp - timeRoundPreparation - 1000) / 1000;
            if (time == 15 || time == 10 || time <= 5) ChatVik.SendRoomMessage("Revert teams in " + time + "sec");
        }

        //CHANGEMENT DES CAMPS
        ChangeTeamSides();

        // CHECK FINAL : IL Y A ASSEZ DE JOUEUR POUR JOUER
        if (PhotonNetwork.playerList.Count(s => s.getTeamID() == 1) < 1
            && PhotonNetwork.playerList.Count(s => s.getTeamID() == 2) < 1)
        {
            ChatVik.SendRoomMessage("There is no players in both team ! Restart Warmup ...");
            StartCoroutine(WarmUp());
            yield break;
        }

        // INIT DES JOUEURS AU ROUND
        PhotonNetwork.room.SetRoomState(GameState.BeginningRound);
    }

    [PunRPC]
    public void rpc_callWinner(int teamid,bool wintype)
    {
        if (wintype)
        {
            teamList[teamid].panel_winround.SetActive(true);
            Invoke("CloseEndPanels", 5f);
        }
        else
        {
            teamList[teamid].panel_wingame.SetActive(true);
            Invoke("CloseEndPanels", 5f);
        }
    }

    /// <summary>
    /// Méthode permettant d'intervertir les camps des joueurs
    /// </summary>
    void ChangeTeamSides()
    {
        //CHENGEMENT DE CAMPS
        List<PhotonPlayer> thieves = PhotonNetwork.playerList.Where(w => w.getTeamID() == 1).ToList();
        List<PhotonPlayer> cops = PhotonNetwork.playerList.Where(w => w.getTeamID() == 2).ToList();
        foreach (PhotonPlayer p in thieves) p.SetAttribute(PlayerAttributes.TEAM, 2);
        foreach (PhotonPlayer p in cops) p.SetAttribute(PlayerAttributes.TEAM, 1);

        int t1_rw = PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.ROUNDSWON, 0);
        int t1_s = PhotonNetwork.room.GetTeamAttribute(1, TeamAttributes.SCORE, 0);
        int t2_rw = PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.ROUNDSWON, 0);
        int t2_s = PhotonNetwork.room.GetTeamAttribute(2, TeamAttributes.SCORE, 0);

        PhotonNetwork.room.SetTeamAttribute(2, TeamAttributes.ROUNDSWON, t1_rw);
        PhotonNetwork.room.SetTeamAttribute(2, TeamAttributes.SCORE, t1_s);
        PhotonNetwork.room.SetTeamAttribute(1, TeamAttributes.ROUNDSWON, t2_rw);
        PhotonNetwork.room.SetTeamAttribute(1, TeamAttributes.SCORE, t2_s);

        //DEPOP DES JOUEURS
        foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(w => (w.getTeamID() == 1 || w.getTeamID() == 2) && w.GetAttribute(PlayerAttributes.HASSPAWNED, false)))
        {
            call_UnspawnPlayerAvatar(p);
        }
    }

    /// <summary>
    /// Coroutine de jeu : Finalisation d'un match avec une équipe gagnante
    /// </summary>
    /// <param name="winnerTeamId"></param>
    /// <returns></returns>
    IEnumerator FinalizeGame(int winnerTeamId)
    {
        // INIT DES VARIABLES DE NOUVEAU ROUND
        InitRoomAttributes(false);
        PhotonNetwork.room.SetRoomState(GameState.isGameFinishing); // au cas ou
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        SetImmobilizeAll(true);

        //AFFICHAGE WINNER
        ChatVik.SendRoomMessage("TEAM " + (winnerTeamId == 2 ? "COPS" : "THIEVES") + " WIN THE GAME");
        photonView.RPC("rpc_callWinner", PhotonTargets.All, new object[] { winnerTeamId, true });


        // COMPTE A REBOURG AVANT LE DEBUT DU ROUND
        int timeRoundPreparation = PhotonNetwork.ServerTimestamp;
        foreach (PhotonPlayer p in PhotonNetwork.playerList) InitPlayerAttributes(p, false);
        ChatVik.SendRoomMessage("Restart Game in " + (durationRoundFinalize / 1000) + "sec");

        while (PhotonNetwork.ServerTimestamp - timeRoundPreparation < durationRoundFinalize)
        {
            yield return new WaitForSeconds(1f);
            int time = durationRoundFinalize / 1000 - (PhotonNetwork.ServerTimestamp - timeRoundPreparation - 1000) / 1000;
            if (time == 15 || time == 10 || time <= 5) ChatVik.SendRoomMessage("Restart Game in " + time + "sec");
        }

        //DEPOP DES JOUEURS
        foreach (PhotonPlayer p in PhotonNetwork.playerList.Where(w => (w.getTeamID() == 1 || w.getTeamID() == 2) && w.GetAttribute(PlayerAttributes.HASSPAWNED, false)))
        {
            call_UnspawnPlayerAvatar(p);
        }
        PhotonNetwork.room.SetRoomState(GameState.isWaitingNewGame);
    }

    //============================================================================================//
    //============================================================================================//



    //============================================================================================//
    //============================================================================================//

    /// <summary>
    /// Ca peut servir....
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // custom method to load the new scene by name
        while (true/*newSceneDidNotFinishLoading*/)
        {
            yield return null;
        }
        PhotonNetwork.isMessageQueueRunning = true;
    }



    /*============================================================================================*/

    /// <summary>
    ///  GESTION SPAWNING DES JOUEURS
    /// </summary>
    public string playerprefabname_overlaw = "Overlaw_Player";
    public string spectatorPrefabName = "Spectator";
    /// <summary>
    /// Faire apparaitre le joueur au point de spawn d'équipe choisit.
    /// </summary>
    public void SpawnMyAvatar()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)) return;

        //bugfix
        ResetCameraTransform();
        //PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
        DestroyPlayerCharacters(PhotonNetwork.player);

        // CREATE AVATAR
        GameObject playerChar;
        Vector2 randpos = UnityEngine.Random.insideUnitCircle * 5f;
        if (PhotonNetwork.player.getTeamID() == 1 || PhotonNetwork.player.getTeamID() == 2)
        {
            playerChar = PhotonNetwork.Instantiate(this.playerprefabname_overlaw, getTeams[PhotonNetwork.player.getTeamID()].TeamSpawnLocation + new Vector3(randpos.x, 0f, randpos.y), Quaternion.identity, 0);
        }
        else
        {
            PhotonNetwork.player.SetAttribute(PlayerAttributes.TEAM, 3);
            playerChar = PhotonNetwork.Instantiate(this.spectatorPrefabName, getTeams[PhotonNetwork.player.getTeamID()].TeamSpawnLocation + new Vector3(randpos.x, 0f, randpos.y), Quaternion.identity, 0);

        }
        PhotonNetwork.player.SetAttribute(PlayerAttributes.HASSPAWNED, true);
        PhotonNetwork.player.SetPlayerState(PlayerState.inGame);

        //SETUP CAMERA
        playerChar.GetComponent<MNG_CameraController>().camera = Camera.main;
        Camera.main.transform.parent = playerChar.transform;
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        Camera.main.transform.localEulerAngles = new Vector3(0, 0, 0);
        //LOCKING MOUSE
        MNG_MainMenu.captureMouse = true;
        ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " has spawn as " + getTeams[PhotonNetwork.player.getTeamID()].TeamName);
    }

    public void call_UnspawnPlayerAvatar(PhotonPlayer p)
    {
        photonView.RPC("rpc_UnspawnPlayerAvatar", PhotonTargets.All, new object[] { p });
    }

    /// <summary>
    /// Appelle la destruction d'un avatar d'un joueur
    /// </summary>
    /// <param name="player"></param>
    [PunRPC]
    public void rpc_UnspawnPlayerAvatar(PhotonPlayer player)
    {
        if (!PhotonNetwork.inRoom || !player.GetAttribute<bool>(PlayerAttributes.HASSPAWNED, false)) return;
        
        //RESET DE LAA CAMERA UNIQUEMENT CHEZ LE CLIENT CONCERNEE
        if (PhotonNetwork.player == player)
        {
            ResetCameraTransform();
            //DESTROY AVATAR UNIQUEMENT PAR LE MASTERCLIENT
            //PhotonNetwork.DestroyPlayerObjects(player);
            DestroyPlayerCharacters(player);
            PlayerAvatar = null;
            player.SetAttribute(PlayerAttributes.HASSPAWNED, false);
        }

        

    }

    /// <summary>
    /// Methode permettant de retrouver l'avatar du joueur
    /// </summary>
    /// <param name="player"></param>
    void DestroyPlayerCharacters(PhotonPlayer player)
    {
        foreach(PhotonView pv in FindObjectsOfType<PhotonView>().Where(x=> x.ownerId == player.ID && x.gameObject.tag == "Player"))
        {
            PhotonNetwork.Destroy(pv);
        }
    }

    //============================================================================================//

    void ResetCameraTransform()
    {
        //SETUP CAMERA
        Camera.main.transform.parent = MainMenuCameraRoot.transform;
        Camera.main.transform.localPosition = new Vector3(0, 0, -48);
        Camera.main.transform.localEulerAngles = new Vector3(40, 0, 0);
    }
    void InitTeamAttributes()
    {
        for (int i = 0; i < teamList.Length; i++)
        {
            PhotonNetwork.room.SetTeamAttribute(i,TeamAttributes.ROUNDSWON, 0);
        }
    }
    void InitRoundAttributes()
    {

    }

    /// <summary>
    /// Réinitialise tous les attributs par défaut  d'une room
    /// </summary>
    /// <param name="reinitgameattr"></param>
    void InitRoomAttributes(bool reinitgameattr)
    {
        PhotonNetwork.room.SetAttribute(RoomAttributes.PRISONOPEN, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.PLAYERSCANSPAWN, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.ALLTHIEFCATCHED, false);
        PhotonNetwork.room.SetAttribute(RoomAttributes.IMMOBILIZEALL, false);
        //PhotonNetwork.room.SetAttribute(RoomAttributes.TIMEROUNDSTARTED, PhotonNetwork.ServerTimestamp);
        
        if (reinitgameattr)
        {
            InitTeamAttributes();
            PhotonNetwork.room.SetAttribute(RoomAttributes.ROUNDNUMBER, 0);
        }
    }

    /// <summary>
    /// Réinitialise tous les attributs par défauts d'un joueur
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="reinitgameattr"></param>
    public void InitPlayerAttributes(PhotonPlayer Player,bool reinitgameattr)
    {
        if (!PhotonNetwork.inRoom) return;
        Player.SetAttribute(PlayerAttributes.ISIDLE, false);
        Player.SetAttribute(PlayerAttributes.ISREADY, false);
        Player.SetAttribute(PlayerAttributes.ISLAGGY, false);
        Player.SetAttribute(PlayerAttributes.ISROOMADMIN, false);
        Player.SetAttribute(PlayerAttributes.ISIMMOBILIZED, PhotonNetwork.room.GetAttribute(RoomAttributes.IMMOBILIZEALL, false));
        Player.SetPlayerState(PlayerState.isReadyToPlay);
        if (reinitgameattr)
        {
            Player.SetAttribute(PlayerAttributes.SCORE, 0);
            Player.SetAttribute(PlayerAttributes.CAPTURESCORE, 0);
            
            Player.SetAttribute(PlayerAttributes.ISCAPTURED, false);
            Player.SetAttribute(PlayerAttributes.INPRISONZONE, false);
            Player.SetAttribute(PlayerAttributes.testKey, "INITIED");

        }
        UpdateReadyBtnState();
    }
    public void SetImmobilizeAll(bool value)
    {
        if (!PhotonNetwork.inRoom) return;
        PhotonNetwork.room.SetAttribute(RoomAttributes.IMMOBILIZEALL, value);
        foreach (PhotonPlayer p in PhotonNetwork.playerList) p.SetAttribute(PlayerAttributes.ISIMMOBILIZED, value);
    }

    public void CloseEndPanels()
    {
        teamList[1].panel_wingame.SetActive(false);
        teamList[1].panel_winround.SetActive(false);
        teamList[2].panel_wingame.SetActive(false);
        teamList[2].panel_winround.SetActive(false);
    }

}

[System.Serializable]
public struct Team
{
    public string TeamName;
    public Vector3 TeamSpawnLocation;
    public GameObject BtnCheckmark;
    public Button Btn_join;
    public Text txt_roundswon;
    public Text txt_tscore;

    public GameObject panel_winround;
    public GameObject panel_wingame;


}

