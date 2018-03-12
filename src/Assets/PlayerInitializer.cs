using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInitializer : Photon.MonoBehaviour {
    public Text textinfo;
    void Reset()
    {
        Setup();
    }

    void Setup()
    {
        if (photonView.owner != null)
            gameObject.name = photonView.owner.NickName;
        textinfo.text = gameObject.name;
        Color _color = Color.black;
        switch (GetComponent<PhotonView>().owner?.GetAttribute<int>(PlayerAttributes.TEAM, 0))
        {
            case 1:
                _color = Color.red;
                break;
            case 2:
                _color = Color.blue;
                break;
        }
        transform.GetComponentInChildren<SkinnedMeshRenderer>().material.color = _color;
    }

    void Start()
    {
        Setup(); 
    }

}
