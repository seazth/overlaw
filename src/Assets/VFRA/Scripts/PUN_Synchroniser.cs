using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PUN_Synchroniser : Photon.MonoBehaviour
{
    public MonoBehaviour[] ComponentsDisableForRemote;

    private new Rigidbody rigidbody;
    private bool appliedInitialUpdate;

    private Vector3     stream_position = Vector3.zero; //We lerp towards this
    private Quaternion  stream_rotation = Quaternion.identity; //We lerp towards this
    private Vector3     stream_rigidbodyvelocity = Vector3.zero; //We lerp towards this
    private Vector3     stream_rigidbodyangular = Vector3.zero; //We lerp towards this

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // stream.SendNext((int)controllerScript._characterState);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rigidbody.velocity);
            stream.SendNext(rigidbody.angularVelocity);
        }
        else
        {
            //controllerScript._characterState = (CharacterState)(int)stream.ReceiveNext();
            stream_position = (Vector3)stream.ReceiveNext();
            stream_rotation = (Quaternion)stream.ReceiveNext();
            rigidbody.velocity = (Vector3)stream.ReceiveNext();
            rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();

        }
    }

    
    // Use this for initialization
    void Awake () {
        stream_position = transform.position;
        rigidbody = GetComponent<Rigidbody>();
        if(!photonView.isMine) foreach (MonoBehaviour comp in ComponentsDisableForRemote) comp.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, stream_position, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, stream_rotation, 0.1f);
        }
    }

}
