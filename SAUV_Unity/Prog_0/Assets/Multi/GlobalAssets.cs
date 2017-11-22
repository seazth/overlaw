using UnityEngine;
using System.Collections;

public enum EntityType
{
    LocalPlayer,Npc,SyncObject,SomeOtherNpc
}
public enum clientState
{
    idle,spectate,playing,disconnect,tryconnect,loading
}
public enum serverState
{
     disconnected, connected,idle,loading
}
public enum NetworkSide
{
    Server,Client,LocalPlayer
}
public class GlobalAssets : MonoBehaviour
{
    public static GlobalAssets mainInstance { get; private set; }
    void Start(){mainInstance = this;}
    public GameObject gop_NES;
    public GameObject gop_serverEntity;

    public GameObject gop_playerChar;
    public GameObject gop_playerCameraForDude;
    public GameObject gop_botChar;

    public GameObject gop_door;
    public GameObject gop_character;

}

