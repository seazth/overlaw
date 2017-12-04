using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;

/// <summary>
/// Entité coté client qui peut être sois le joueur local ou la vision d'un joueur sur le serveur.
/// </summary>
public abstract class ClientEntityLogic : MonoBehaviour
{
    /// <summary>
    /// NES est le l'objet qui synchronise les données de l'entité en fonction de son type.
    /// </summary>
    public NES _NES { get; private set; }
    public GameObject gop_camera { get; private set; }
    public GameObject go_mesh;

    protected virtual void Start()
    {
        _NES = this.GetComponentInChildren<NES>();
        if (_NES == null) { gameObject.SetActive(false); return; }
        //
        if (_NES._networkSide == NetworkSide.LocalPlayer && _NES._entityType == EntityType.Player) Start_LocalPlayer();
        else if (_NES._networkSide == NetworkSide.Server) {  Start_Server(); } // no data updating on server
        else
        {
            if(_NES._networkSide == NetworkSide.LocalPlayer && _NES._entityType == EntityType.Player) { Init_LocalPlayerEntity(); }
            else { Start_Client(); }
        }
    }


    // 
    private void Init_LocalPlayerEntity()
    {
        //go_mesh.SetActive(false);
        Utils.setActiveColliders(gameObject, true);
        Utils.setBodyKinematic(gameObject, false);
        Utils.setActiveNavMesh(gameObject, false);
    }

    protected virtual void OnDisable()
    {
        if (_NES != null && _NES._entityType == EntityType.Player) Main_Canvas.show_Menu(true);
    }

    /// <summary>
    /// Boucle Update par défaut. Il est déconseillé de la modifier si l'entité n'est pas sur le serveur.
    /// </summary>
    protected virtual void Update()
    {
        if (_NES.isLocalPlayer)
        {
            Update_LocalPlayer();
            _NES._data.Update_LocalPlayerLogic(this);
        }
        else if (_NES.isServer)
        { // no data updating on server
            Update_Server();
            _NES._data.Update_ServerEntityLogic(this);
        }
        else
        {
            Update_Client();
            _NES._data.Update_ClientEntityLogic(this);
        }
    }

   

    // start by networkside with server = NPC or SyncObject
    protected abstract void Start_LocalPlayer();
    protected virtual void Start_Server()
    {
        Utils.setLayer(go_mesh.transform, GlobalAssets.mainInstance.lay_serverEntity);
        //
        if (_NES._entityType == EntityType.Npc || _NES._entityType == EntityType.SyncObject || _NES._entityType == EntityType.SomeOtherNpc)
        {
            Utils.setRecursiveMaterial(gameObject, Color.red);
            Utils.setActiveNavMesh(gameObject, true);
        }
        else
        {
            Utils.setActiveNavMesh(gameObject, false);
        }
        Utils.setBodyKinematic(gameObject, false);
        Utils.setActiveColliders(gameObject, true);


        // SI JOUEUR LOCALDETECTEE => DETECTEE PAR NES => DESACTIVATION PHYSICS PAR COMMAND SUR SON ENTITE_SERVER



        //if (_NES._networkSide == NetworkSide.Server && _NES._entityType == EntityType.Player && MyServer._serverState == serverState.connected && MyClient._clientState != clientState.disconnect)
        //   Utils.setActiveColliders(gameObject, false);
    }
    protected virtual void Start_Client()
    {
        //if (_NES.isServer) Utils.setActiveColliders(gameObject, false);

        Utils.setBodyKinematic(gameObject, true);
        Utils.setActiveColliders(gameObject, false);
        Utils.setActiveNavMesh(gameObject, false);

    }
    // update by networkside
    protected abstract void Update_LocalPlayer();
    protected abstract void Update_Server();
    protected abstract void Update_Client();

    /// <summary>
    /// CETTE FONCTION PEUT CHANGER
    /// </summary>
    /// <param name="src"></param>
    /// <param name="value"></param>
    public virtual void Interact(NES src, bool value)
    {
        if (true/*logic de security*/)
        {
        }
    }
}
