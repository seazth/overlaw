using UnityEngine;
using UnityEngine.AI;

public static class Utils
{
    /*
    [MenuItem("Tools/MyTool/Do It in C#")]
    static void DoIt(){EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");}
    */

    /// <summary>
    /// Apply a color on all materials found in gameobject components (himself & sons)
    /// </summary>
    /// <param name="go"></param>
    /// <param name="color"></param>
    public static void setRecursiveMaterial(GameObject go , Color color)
    {
        foreach (MeshRenderer mrs in go.GetComponentsInChildren<MeshRenderer>()){mrs.material.color = color;}
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null){mr.material.color = color;}
    }

    /// <summary>
    /// Instancie une nouvelle caméra par défaut à cet entité si celui ci n'en possède pas une.
    /// </summary>
    /// <param name="active">Active la caméra après son instanciation.</param>
    public static GameObject InstanciateNewCamera(Transform parent, GameObject gop_camera, bool active)
    {
        if (parent.GetComponents<Camera>().Length == 0 || parent.GetComponentsInChildren<Camera>().Length == 0)
        {
            GameObject camera = GameObject.Instantiate(gop_camera, parent);
            if (active) MNG_GameManager.setActiveCamera(gop_camera.GetComponent<Camera>());
            return camera;
        }
        return null;
    }

    /// <summary>
    /// Desactive tout les colliders component de go
    /// </summary>
    /// <param name="go">GameObject source</param>
    public static void setActiveColliders(GameObject go, bool value)
    {
        foreach (Collider coll in go.GetComponents<Collider>()) { coll.enabled = value; }
        foreach (Collider coll in go.GetComponentsInChildren<Collider>()) { coll.enabled = value; }
    }

    public static void setBodyKinematic(GameObject go, bool value)
    {
        foreach (Rigidbody body in go.GetComponents<Rigidbody>()) { body.isKinematic = value; }
        foreach (Rigidbody body in go.GetComponentsInChildren<Rigidbody>()) { body.isKinematic = value; }
    }
    public static void setActiveNavMesh(GameObject go, bool value)
    {
        foreach (NavMeshAgent body in go.GetComponents<NavMeshAgent>()) { body.enabled = value; }
        foreach (NavMeshAgent body in go.GetComponentsInChildren<NavMeshAgent>()) { body.enabled = value; }
    }

    public static void setLayer(Transform go, LayerMask value)
    {
        go.gameObject.layer = 8;
        for (int i = 0; i < go.childCount; i++)
        {
            setLayer(go.GetChild(i), value);
        }
    }

}