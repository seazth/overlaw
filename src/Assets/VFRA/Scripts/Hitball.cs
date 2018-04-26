using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Hitball : Photon.MonoBehaviour
{
    private bool appliedInitialUpdate;
    const float duration = 6000f;
    private int timestampStart;

    void Awake()
    {
        //correctPlayerPos = transform.position;
    }
    void Start()
    {
        gameObject.name = gameObject.name + photonView.viewID;
        timestampStart = PhotonNetwork.ServerTimestamp;
    }
    void Update()
    {
        if ( photonView.isMine &&  PhotonNetwork.ServerTimestamp >  timestampStart + duration)
        {
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }
    }

    

    void OnPhotonInstantiate(PhotonMessageInfo info){}
    private void OnCollisionEnter(Collision collision)
    {
    }
}
