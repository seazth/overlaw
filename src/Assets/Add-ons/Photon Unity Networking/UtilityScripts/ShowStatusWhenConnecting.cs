using UnityEngine;
using System.Collections;

public class ShowStatusWhenConnecting : MonoBehaviour 
{
    public GUISkin Skin;

    void OnGUI()
    {
        if( Skin != null )
        {
            GUI.skin = Skin;
        }

        float width = 300;
        float height = 50;

        Rect centeredRect = new Rect( ( Screen.width - width -5 ) , ( Screen.height - height -5 ), width, height );

        GUILayout.BeginArea( centeredRect, GUI.skin.box );
        {
            GUILayout.Label( "Connecting" + GetConnectingDots(), GUI.skin.customStyles[ 0 ] );
            GUILayout.Label( "Status: " + PhotonNetwork.connectionStateDetailed );
        }
        GUILayout.EndArea();

        
    }

    string GetConnectingDots()
    {
        string str = "";
        int numberOfDots = Mathf.FloorToInt( Time.timeSinceLevelLoad * 3f % 3 );

        for( int i = 0; i < numberOfDots; ++i )
        {
            str += " .";
        }

        return str;
    }
}
