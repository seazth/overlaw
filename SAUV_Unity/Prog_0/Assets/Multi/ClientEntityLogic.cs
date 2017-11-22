using UnityEngine;
using System.Collections;

public class ClientEntityLogic : MonoBehaviour
{

    public NES _NES { get; protected set; }

    protected virtual void Start()
    {
        _NES = this.GetComponentInChildren<NES>();
        var material = this.GetComponent<MeshRenderer>().material;
        if (_NES._entityType == EntityType.LocalPlayer) material.color = Color.green;
        else material.color = Color.red;
    }

    protected virtual void OnDisable()
    {
        if (_NES._entityType == EntityType.LocalPlayer) Main_Canvas.show_Menu(true);
    }

    protected virtual void Update()
    {
        if (_NES.isLocalPlayer) {
            UpdateLocalPlayer();
            _NES._data.UpdateLocalPlayerLogic(this);
        }
        else _NES._data.UpdateClientEntityLogic(this);
    }

    protected virtual void UpdateLocalPlayer(){}


}
