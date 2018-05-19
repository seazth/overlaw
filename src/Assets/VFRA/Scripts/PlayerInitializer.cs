using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInitializer : Photon.MonoBehaviour {

    public TextMesh textname;
    public Color[] Colors;
    public bool isMyAvatar;

    void Reset()
    {
        Setup();
    }
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        isMyAvatar = photonView.isMine;
        MNG_GameManager.PlayerAvatar = this;
        textname.GetComponent<MeshRenderer>().enabled = photonView.isMine;
        if (photonView.owner != null) gameObject.name = photonView.owner.NickName;
        textname.text = gameObject.name;
        InvokeRepeating("UpdateChar", 0, 2f);
    }

    private void FixedUpdate()
    {
        if(textname.gameObject.activeInHierarchy) textname.transform.LookAt(Camera.main.transform.position);
    }

    private void UpdateChar()
    {
        updatecolor();
        updatename();
    }
    void updatename()
    {
        if (photonView.isMine || photonView.owner.GetAttribute<int>(PlayerAttributes.TEAM, 0) == 3) textname.gameObject.SetActive(false);
        else textname.gameObject.SetActive(photonView.owner?.getTeamID() == PhotonNetwork.player.getTeamID());
    }
    void updatecolor()
    {
        if (transform.GetComponentInChildren<SkinnedMeshRenderer>().materials.Length != 3) return;
        Color _color = Colors[0];
        Color _colorsec = Colors[1];
        switch (photonView.owner?.GetAttribute<int>(PlayerAttributes.TEAM, 0))
        {
            case 1:
                _color = Colors[2];
                _colorsec = Colors[3];
                break;
            case 2:
                _color = Colors[4];
                _colorsec = Colors[5];
                break;
        }
        transform.GetComponentInChildren<SkinnedMeshRenderer>().materials[0].color = _colorsec;
        transform.GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = _color;
    }

}
