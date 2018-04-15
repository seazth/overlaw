using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public delegate void JumpDelegate ();

public class ThirdPersonControllerNET : Photon.MonoBehaviour
{
    float timeCantMove;
    float timeCantShoot;
    float timeHoldingShoot;

    public float durationCantMove = 5f;
    public float durationCantShoot = 1f;
    public float durationCatch = 2f;
    bool isCapturingThief = false;
    bool _isPrepareToThrow = false;
    public bool isPrepareToThrow { get { return _isPrepareToThrow; } }
    float maxForce = 10f;
    float minForce = 1f;
    public float throwForceMult = 2f;

    public Rigidbody target;
	public float speed = 1.0f,
        walkSpeedDownscale = 2.0f,
        jumpSpeed = 1.0f;
	public LayerMask groundLayers = -1;
    public bool showGizmos = true;
	public JumpDelegate onJump = null;
    public float inputThreshold = 0.001f
        , groundDrag = 5.0f
        , directionalJumpFactor = 0.7f
        , airDrag = 1f;
    public float groundedDistance = 0.25f; //0.6f
    public float groundedCheckOffset = 1.05f; //0.7f



    public bool grounded {get {return Physics.CheckSphere(target.transform.position + target.transform.up * -groundedCheckOffset, groundedDistance, groundLayers);}}

    void Reset ()
	{
        Setup ();
	}
    void Setup ()
	{
		if (target == null) { target = GetComponent<Rigidbody> (); }
    }


    void Start ()
	{
        Setup (); // Retry setup if references were cleared post-add
		if (target == null)
		{
			Debug.LogError ("No target assigned. Please correct and restart.");
			enabled = false;
			return;
		}
		//target.freezeRotation = true;
	}


    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.isMine && collision.gameObject.tag == "Ball")
        {
            GetComponent<PhotonView>().RPC("rpc_immobilize", PhotonTargets.All);
            ChatVik.SendRoomMessage(collision.gameObject.GetComponent<PhotonView>().owner.NickName + " knocked out " + photonView.owner.NickName);
        }
    }


    void Update ()
	// Handle rotation here to ensure smooth application.
	{
        if (!photonView.isMine) return;

        if (Input.GetMouseButtonDown(0) 
            && PhotonNetwork.player.getTeamID()==1
            && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBLIZED, false)) // you can only give a slap when you're a thief
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(transform.forward * 0.3f + transform.position, transform.forward, out hitInfo, 1.2f, LayerMask.GetMask("NetEntity"));
            if (hit && hitInfo.transform.gameObject.tag == "Player")
            {
                hitInfo.transform.GetComponent<PhotonView>().RPC("rpc_immobilize", PhotonTargets.All);
                ChatVik.SendRoomMessage(photonView.owner.NickName + " kick the ass of " + hitInfo.transform.GetComponent<PhotonView>());

            }
        }

        if (Input.GetMouseButton(1) 
            && PhotonNetwork.player.getTeamID() == 2
            && Time.timeSinceLevelLoad - timeCantShoot > durationCantShoot && !_isPrepareToThrow
            && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBLIZED, false)) // you can only throw a ball when you're a cop
        {
            timeHoldingShoot = Time.timeSinceLevelLoad;
            StartCoroutine(prepareToThrow());
        }

        if (Input.GetKeyDown(KeyCode.F) 
            && PhotonNetwork.player.getTeamID() == 2 
            && !isCapturingThief
            && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBLIZED, false) ) // you can only capture when you're a cop
        {

            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(transform.forward * 0.3f + transform.position, transform.forward, out hitInfo, 1.2f, LayerMask.GetMask("NetEntity"));
            if (hit && hitInfo.transform.gameObject.tag == "Player")
            {
                
                PhotonPlayer thiefInCatch = hitInfo.transform.GetComponent<PhotonView>().owner;
                if (thiefInCatch != null && thiefInCatch.getTeamID() == 1)
                {
                    print("try capturing !");
                    StartCoroutine(catchThief(thiefInCatch,hitInfo.transform));
                }
            }
        }
    }


    IEnumerator catchThief(PhotonPlayer thiefTarget, Transform t_thief)
    {
        isCapturingThief = true;
        float timeCatching = Time.timeSinceLevelLoad;
        bool waitCatching = true;

        while (waitCatching && isCapturingThief)
        {
            yield return new WaitForSeconds(0.5f);
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(
                transform.forward * 0.2f + transform.position
                , transform.forward
                , out hitInfo
                , 1.2f
                , LayerMask.GetMask("NetEntity"));
            if (Input.GetKey(KeyCode.F) && hit  && hitInfo.transform==t_thief)
            {
                if (Time.timeSinceLevelLoad - timeCatching > durationCatch) waitCatching = false;
                else print("will be captured in "+(durationCatch - (Time.timeSinceLevelLoad - timeCatching))+"ms");
            }
            else
            {
                isCapturingThief = false;
                yield break;
            }
        }
        if (isCapturingThief)
        {
            ChatVik.SendRoomMessage(PhotonNetwork.player.NickName + " has captured " + thiefTarget.NickName);
            thiefTarget.SetAttribute(PlayerAttributes.ISCAPTURED, true);
            t_thief.GetComponent<PhotonView>().RPC("rpc_capture", PhotonTargets.All);
            isCapturingThief = false;
        }
    }

    IEnumerator prepareToThrow()
    {
        _isPrepareToThrow = true;
        float force;
        
        while (_isPrepareToThrow)
        {
            if (!Input.GetMouseButton(1) && _isPrepareToThrow)
            {
                force =  Mathf.Clamp(minForce+((Time.timeSinceLevelLoad - timeHoldingShoot) * throwForceMult), minForce, maxForce);
                print("Send ball with force = " + force);
                PhotonNetwork.Instantiate("Hitball"
                , transform.position + transform.forward * 1f + transform.up * 0.2f + transform.right * 0.1f
                , Quaternion.identity, 0).GetComponent<Rigidbody>()
                .AddForce(GetComponentInChildren<Camera>().transform.forward
                    * force
                    , ForceMode.Impulse);
                timeCantShoot = Time.timeSinceLevelLoad;
                _isPrepareToThrow = false;
            }
            // else = ThrowCanceled
            yield return null;
        }

    }



    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] objs = photonView.instantiationData; //The instantiate data..
        bool[] mybools = (bool[])objs[0];   //Our bools!
        MeshRenderer[] rens = GetComponentsInChildren<MeshRenderer>();
        // enforce disable of vik equipment
        //rens[0].enabled = false;// mybools[0];//Axe
        //rens[1].enabled = false;// mybools[1];//Shield

    }

    void FixedUpdate()
    // Handle movement here since physics will only be calculated in fixed frames anyway
    {
        if (!photonView.isMine) return;



        if (grounded)
        {
            if (!PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBLIZED,false)) // immobilization gestion 
            {

                target.drag = groundDrag;
                // Apply drag when we're grounded

                if (Input.GetButton("Jump"))
                // Handle jumping
                {
                    target.AddForce(
                        jumpSpeed * target.transform.up +
                            target.velocity.normalized * directionalJumpFactor,
                        ForceMode.VelocityChange
                    );
                    // When jumping, we set the velocity upward with our jump speed
                    // plus some application of directional movement

                    onJump?.Invoke();
                }
                else
                // Only allow movement controls if we did not just jump
                {
                    //horizontal
                    float sidestep = -(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
                    float horizontal = Input.GetAxis("Horizontal");
                    float SidestepAxisInput = Mathf.Abs(sidestep) > Mathf.Abs(horizontal) ? sidestep : horizontal;

                    Vector3 movement = Input.GetAxis("Vertical") * target.transform.forward +
                        SidestepAxisInput * target.transform.right;

                    float appliedSpeed =  speed;
                    // Scale down applied speed if in walk mode

                    if (Input.GetAxis("Vertical") < 0.0f)
                    // Scale down applied speed if walking backwards
                    {
                        appliedSpeed /= walkSpeedDownscale;
                    }

                    if (movement.magnitude > inputThreshold)
                    // Only apply movement if we have sufficient input
                    {
                        target.AddForce(movement.normalized * appliedSpeed, ForceMode.VelocityChange);
                    }
                    else
                    // If we are grounded and don't have significant input, just stop horizontal movement
                    {
                        //target.velocity = new Vector3(0.0f, target.velocity.y, 0.0f);
                        return;
                    }
                }
            }
            else if (Time.timeSinceLevelLoad > timeCantMove + durationCantMove && !PhotonNetwork.room.GetAttribute(RoomAttributes.IMMOBILIZEALL, false))
            {
                PhotonNetwork.player.SetAttribute(PlayerAttributes.ISIMMOBLIZED, false);
            }
        }
        else
        {
            target.drag = airDrag;
        }
    }
	


    [PunRPC]
    public void rpc_immobilize()
    {
        if (PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED,false) && PhotonNetwork.player.GetPlayerState() == PlayerState.inGame) 
        {
            isCapturingThief = false;
            _isPrepareToThrow = false;
            PhotonNetwork.player.SetAttribute(PlayerAttributes.ISIMMOBLIZED, true);
            timeCantMove = Time.timeSinceLevelLoad;
        }
    }

    [PunRPC]
    public void rpc_capture()
    {
        if (PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED, false) && PhotonNetwork.player.GetPlayerState() == PlayerState.inGame) // can't be capture if you're a spectator
        {
            Vector2 randpos = UnityEngine.Random.insideUnitCircle * 6f;
            transform.position = MNG_GameManager.getPrisonLocation + new Vector3(randpos.x,0f, randpos.y);// ZONE PRISON A FAIRE
        }
    }


}
