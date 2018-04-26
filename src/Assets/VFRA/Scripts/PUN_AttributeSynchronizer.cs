using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PUN_AttributeSynchronizer : Photon.MonoBehaviour
{
    void Start()
    {

    }
    void Update()
    {

    }
    /*
    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
        Hashtable props = playerAndUpdatedProps[1] as Hashtable;
        print("Kebapi");
    }
    */
}

public enum RoomAttributes { GAMESTATE, PLAYERSREADY, PLAYERSCANSPAWN,
    PRISONOPEN,
    ALLTHIEFCATCHED,
    TIMEROUNDSTARTED,
    ROUNDNUMBER,
    IMMOBILIZEALL
}
public enum TeamAttributes { PLAYERSALIVE, PLAYERSCOUNT,
    ROUNDSWON
}
public enum GameState { GameState_error, isLoading, isWaitingNewGame,  RoundRunning, isGameFinishing, isGameReloading, 
    isRoundFinishing,
    BeginningRound,
    WarmUp,
    PrepareRound
}
public enum PlayerState
{
    PlayerState_error,inMenu, isSpectating, inGame, isReadyToPlay, isLoading, isIdle, isDisconnecting, isConnected, inLobby, joiningRoom, isWaitingToSpawn
}
public enum PlayerAttributes
{
    SCORE, TEAM, PLAYERSTATE, ISIDLE, ISLAGGY, testKey, ISREADY, ISROOMADMIN, HASSPAWNED,
    INZONE , ISCAPTURED, ISIMMOBILIZED,
    INPRISONZONE
}

public static class RoomAttributesExtension
{
    /**
     *  Pour faire une secu il faut integrer un check constant ?
        "PunTurnManager" est cool pour les events !
     *  > Faire Sécurité des properties avec une sauvegarde de celle-ci chez le owner (owner peut être le hacker ...). et une restauration si ce n'est pas autorisé.
     */

    public static void SetTeamAttribute(this Room room, int teamID, TeamAttributes teamAttribute, object value)
    {
        Hashtable newTable = new Hashtable(); newTable["TEAM" + teamID + "/" + teamAttribute.ToString()] = value; room.SetCustomProperties(newTable);
    }

    public static T GetTeamAttribute<T>(this Room room, int teamID, TeamAttributes teamAttribute, T defaultValue)
    {
        object attr; if (room.CustomProperties.TryGetValue("TEAM" + teamID + "/" + teamAttribute.ToString(), out attr)) return (T)attr; return defaultValue;
    }

    public static bool isGameOwner(this PhotonPlayer player) { return PhotonNetwork.inRoom /* SERA UTILE ?*/ && player.IsMasterClient; }
    public static bool isGameAdmin(this PhotonPlayer player) { return PhotonNetwork.inRoom && player.GetAttribute<bool>(PlayerAttributes.ISROOMADMIN, false); }
    public static void SetRoomState(this Room room, GameState state) { room.SetAttribute(RoomAttributes.GAMESTATE, (int)state); }
    public static GameState GetRoomState(this Room room) { return PhotonNetwork.room.GetAttribute<GameState>(RoomAttributes.GAMESTATE, GameState.GameState_error); }

    public static void SetAttribute(this Room room, RoomAttributes roomAttribute, object value)
    {
        Hashtable newTable = new Hashtable(); newTable[roomAttribute.ToString()] = value; room.SetCustomProperties(newTable);
    }
    public static T GetAttribute<T>(this Room room, RoomAttributes roomAttribute, T defaultValue)
    {
        object attr; if (room.CustomProperties.TryGetValue(roomAttribute.ToString(), out attr)) return (T)attr; return defaultValue;
    }

}



public static class PlayerAttributesExtension
{
    public static void SetPlayerState(this PhotonPlayer player, PlayerState state) { player.SetAttribute(PlayerAttributes.PLAYERSTATE, (int)state); }
    public static PlayerState GetPlayerState(this PhotonPlayer player) { return player.GetAttribute<PlayerState>(PlayerAttributes.PLAYERSTATE, PlayerState.PlayerState_error); }

    public static int getTeamID(this PhotonPlayer player)
    {
        return player.GetAttribute<int>(PlayerAttributes.TEAM, 0);
    }
    public static void SetAttribute(this PhotonPlayer player, PlayerAttributes playerAttribute, object value)
    {
        Hashtable newTable = new Hashtable()/*est bon ?? ou customproperties*/; newTable[playerAttribute.ToString()] = value; player.SetCustomProperties(newTable);
    }
    public static T GetAttribute<T>(this PhotonPlayer player, PlayerAttributes playerAttibute, T defaultValue)
    {
        object attr; if (player.CustomProperties.TryGetValue(playerAttibute.ToString(), out attr)) return (T)attr; return defaultValue;
    }
}