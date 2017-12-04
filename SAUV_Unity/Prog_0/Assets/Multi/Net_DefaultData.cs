using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Net_DefaultData
{
    public Vector3 position;
    public Quaternion rotation;
    public bool activated;

    public Net_DefaultData()
    {
        position = new Vector3(0f, 1f, 0f);
        rotation = new Quaternion();
        activated = false;
    }
    /// <summary>
    /// ss
    /// </summary>
    /// <param name="self">L'entité mutable</param>
    public virtual void Update_ServerEntityLogic(ClientEntityLogic self)
    {
        if (self._NES._entityType == EntityType.Npc)
        {
            position = self.transform.position;
            rotation = self.transform.rotation;
        }
        else
        {
            self.transform.position = position;
            self.transform.rotation = rotation;
        }
    }
    public virtual void Update_ClientEntityLogic(ClientEntityLogic self)
    {
        //self.transform.position = Vector3.Lerp(self.transform.position, position, .8f);
        //self.transform.rotation = Quaternion.Lerp(self.transform.rotation, rotation, .8f);
        self.transform.position = position;
        self.transform.rotation = rotation;

    }
   

    public virtual void Update_LocalPlayerLogic(ClientEntityLogic self)
    {
        position = self.transform.position;
        rotation = self.transform.rotation;
        //position = Vector3.Lerp(position, self.transform.position, .5f);
        //rotation = Quaternion.Lerp(rotation, self.transform.rotation, .5f);
    }
}


/*
public class Net_DefaultData : ScriptableObject
{
    [MenuItem("Tools/MyTool/Do It in C#")]
    static void DoIt()
    {
        EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
    }

}
*/
