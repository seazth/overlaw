using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This simple chat example showcases the use of RPC targets and targetting certain players via RPCs.
/// </summary>
public class ChatVik : Photon.MonoBehaviour
{

    public static ChatVik SP;
    public List<string> messages = new List<string>();

    private int chatHeight = (int)200;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";
    private float lastUnfocusTime = 0;
    
    void Awake()
    {
        SP = this;
    }

    void OnGUI()
    {        
        GUI.SetNextControlName("");

        GUILayout.BeginArea(new Rect(0, Screen.height - chatHeight, Screen.width, chatHeight));
        
        //Show scroll list of chat messages
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUI.color = Color.cyan;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(messages[i]);
        }
        GUILayout.EndScrollView();
        GUI.color = Color.white;

        //Chat input
        
        /*
        GUILayout.BeginHorizontal(); 
        GUI.SetNextControlName("ChatField");
        chatInput = GUILayout.TextField(chatInput, GUILayout.MinWidth(300));
       
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n'){
            if (GUI.GetNameOfFocusedControl() == "ChatField")
            {                
                SendChat(PhotonTargets.All);
                lastUnfocusTime = Time.time;
                GUI.FocusControl("");
                GUI.UnfocusWindow();
            }
            else
            {
                if (lastUnfocusTime < Time.time - 0.1f)
                {
                    GUI.FocusControl("ChatField");
                }
            }
        }
        
        //if (GUILayout.Button("SEND", GUILayout.Height(17)))
         //   SendChat(PhotonTargets.All);
        //GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        */
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 3, Screen.height / 2 + 8, Screen.width / 2, Screen.height / 2));
        GUILayout.Label("o");
        GUILayout.EndArea();

    }

    public static void AddMessage(string text)
    {
        SP.messages.Add(text);
        if (SP.messages.Count > 15)
            SP.messages.RemoveAt(0);
    }

    public static void SendRoomMessage(string text)
    {
        if(PhotonNetwork.inRoom)SP.photonView.RPC("SendRoomMessage", PhotonTargets.All, text);
    }

    [PunRPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[" + info.sender + "] " + text);
    }
    [PunRPC]
    void SendRoomMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[SV] " + text);
    }

    void SendChat(PhotonTargets target)
    {
        if (chatInput != "")
        {
            photonView.RPC("SendChatMessage", target, chatInput);
            chatInput = "";
        }
    }

    void SendChat(PhotonPlayer target)
    {
        if (chatInput != "")
        {
            chatInput = "[PM] " + chatInput;
            photonView.RPC("SendChatMessage", target, chatInput);
            chatInput = "";
        }
    }

    void OnLeftRoom()
    {
        this.enabled = false;
    }

    void OnJoinedRoom()
    {
        this.enabled = true;
    }
    void OnCreatedRoom()
    {
        this.enabled = true;
    }

    private void OnPlayerConnected(NetworkPlayer player)
    {
        //NetworkPlayer
        chatInput = ""; 
        foreach (var item in PhotonNetwork.playerList)
        {
            if (item.UserId == player.guid) chatInput = item.NickName;
        } 
        SendChat(PhotonTargets.All);
    }
}
