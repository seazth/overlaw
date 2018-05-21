using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestion du joueur spectateur
/// </summary>
public class SpectatorController : Photon.MonoBehaviour {

    public Rigidbody _rigidbody;

    PhotonPlayer followedPlayer;
    GameObject followedGO;
    bool isFollowing;

    public float
        speed = 1.0f;

    private const float
        inputThreshold = 0.01f;



    void Start () {
        if (photonView.owner != null) gameObject.name = photonView.owner.NickName;

    }
    void Update () {
	}


    /// <summary>
    /// Ce sont des classes non opérationnels qui permettent de suivre un joueur dans une session en cours
    /// </summary>
    /// <param name="player"></param>
    /// <param name="go"></param>
    public void followPlayer(PhotonPlayer player, GameObject go) { StartCoroutine(followPlayerGO(player, go)); }
    public IEnumerator followPlayerGO(PhotonPlayer player, GameObject go)
    {
        followedPlayer = player;
        followedGO = go;
        while (player != null && player.GetPlayerState()== PlayerState.inGame && go!=null)
        {
            transform.position = Vector3.Lerp(transform.position, go.transform.position, 0.05f);
            yield return null;
        }
        followedPlayer = null;
        followedGO = null;
    }

    /// <summary>
    /// Gestion des déplacements
    /// </summary>
    private void FixedUpdate()
    {
        if (isFollowing) return;
        //horizontal
        Vector3 movement =
            Input.GetAxis("Vertical") * _rigidbody.transform.forward +
            Input.GetAxis("Horizontal") * _rigidbody.transform.right +
            (Input.GetButton("Jump") ? 1 : 0) * _rigidbody.transform.up +
            (Input.GetButton("Crouch") ? -1 : 0) * _rigidbody.transform.up;

        if (movement.magnitude > inputThreshold)
        // Only apply movement if we have sufficient input
        {
            _rigidbody.AddForce(movement.normalized * speed, ForceMode.VelocityChange);
        }
        else
        // If we are grounded and don't have significant input, just stop horizontal movement
        {
            _rigidbody.AddForce ( new Vector3 (0.0f, 0.0f, 0.0f),ForceMode.VelocityChange);
        }
    }
    
}
