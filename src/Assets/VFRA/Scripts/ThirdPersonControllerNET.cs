using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public delegate void JumpDelegate ();

public class ThirdPersonControllerNET : Photon.MonoBehaviour
{
    float timeCantMove;
    float timeCantShoot;
    float timeCantPunch;
    float timeHoldingShoot;
    float timeCantClimbGrab;
    public bool _climbing = false;
    public bool _grounded = false;

    public float durationCantMove = 5f;
    public float durationCantShoot = 1.6f;
    public float durationCantPunch = 1f;
    public float durationCantClimbGrab = 0.1f;

    public float durationCatch = 2f;
    bool isCapturingThief = false;
    bool _isPrepareToThrow = false;
    public bool isPrepareToThrow { get { return _isPrepareToThrow; } }
    public float maxThrowForce =20f;
    public float minThrowForce = 1f;
    public float throwForceMult = 2f;

    public Rigidbody target;
	public float speed = 1.0f,
        walkSpeedDownscale = 2.0f,
        jumpforce = 1.0f;
	public LayerMask groundLayers = -1;
    public bool showGizmos = true;
	public JumpDelegate onJump = null;
    public float 
        inputThreshold = 0.001f
        , groundDrag = 5.0f
        , directionalJumpFactor = 0.7f
        , airDrag = 0f
        , groundedDistance = 0.25f //0.6f
        , groundedCheckOffset = 1.05f //0.7f
        , airmove_mult= 0.05f
        , checkclimbforward = 1f
        , checkclimbtop = 1f
        ;
    AnimationController CTRL_Animation;

    public bool grounded {
        get {
            _grounded = Physics.CheckSphere(target.transform.position + target.transform.up * -groundedCheckOffset, groundedDistance, groundLayers);
            if (_grounded) _climbing = false; return _grounded;
        } }

    void Reset ()
	{
        Setup ();
	}
    void Setup ()
    {
        if (target == null) { target = GetComponent<Rigidbody>(); }
        if (CTRL_Animation == null) { CTRL_Animation = GetComponent<AnimationController>(); }
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
        if (collision.gameObject.tag == "Ball" && photonView.owner != null)
        {
            GetComponent<PhotonView>().RPC("rpc_immobilize", PhotonTargets.All);
            ChatVik.SendRoomMessage(collision.gameObject.GetComponent<PhotonView>().owner.NickName + " knocked out " + photonView.owner.NickName);
        }
    }


    void Update ()
	// Handle rotation here to ensure smooth application.
	{
        if (!photonView.isMine) return;


        if (!PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false)) // immobilization gestion 
        {


            if (Input.GetMouseButtonDown(0)
                && Time.timeSinceLevelLoad - timeCantPunch > durationCantPunch
                && !_climbing
                && !_isPrepareToThrow) // you can only give a slap when you're a thief
            {
                timeCantPunch = Time.timeSinceLevelLoad;
                CTRL_Animation.call_anim_trigger("Punch", layer: 1);

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
                && Time.timeSinceLevelLoad - timeCantShoot > durationCantShoot
                && !_isPrepareToThrow
                && !_climbing)  // you can only throw a ball when you're a cop

            {
                timeHoldingShoot = Time.timeSinceLevelLoad;
                StartCoroutine(prepareToThrow());
            }

            if (Input.GetKeyDown(KeyCode.E)
                && PhotonNetwork.player.getTeamID() == 2
                && !isCapturingThief
                && !_climbing) // you can only capture when you're a cop

            {

                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(transform.forward * 0.3f + transform.position, transform.forward, out hitInfo, 1.2f, LayerMask.GetMask("NetEntity"));
                if (hit && hitInfo.transform.gameObject.tag == "Player")
                {

                    PhotonPlayer thiefInCatch = hitInfo.transform.GetComponent<PhotonView>().owner;
                    if (thiefInCatch != null && thiefInCatch.getTeamID() == 1)
                    {
                        print("try capturing !");
                        StartCoroutine(catchThief(thiefInCatch, hitInfo.transform));
                    }
                }
            }


            if (Input.GetButtonDown("Jump") 
                && (grounded || (_climbing)))
            // Handle jumping
            {
                    target.AddForce( jumpforce * target.transform.up + target.velocity.normalized * directionalJumpFactor, ForceMode.VelocityChange );
                    onJump();
                    _climbing = false;
                    CTRL_Animation.call_anim_trigger("Jump");
                    timeCantClimbGrab = Time.timeSinceLevelLoad;

            }
            else if (Input.GetKeyDown(KeyCode.LeftControl)
                && _climbing)
            // Handle releasing
            {
                CTRL_Animation.call_anim_trigger("Jump");
                _climbing = false;
                timeCantClimbGrab = Time.timeSinceLevelLoad;
            }
            else if (Input.GetKey(KeyCode.LeftControl)
                && !_climbing 
                && Time.timeSinceLevelLoad - timeCantClimbGrab > durationCantClimbGrab)
            // Handle climbing
            {
                bool canclimb = Physics.CheckSphere(target.transform.position + target.transform.up * checkclimbtop + target.transform.forward * checkclimbforward, 0.2f, groundLayers);

                if (canclimb)
                {
                    CTRL_Animation.call_anim_trigger("Climb");
                    _climbing = true;
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
        CTRL_Animation.call_anim_trigger("PrepareThrow",layer:1);

        while (_isPrepareToThrow)
        {
            if (!Input.GetMouseButton(1) && _isPrepareToThrow)
            {
                force =  Mathf.Clamp(minThrowForce+((Time.timeSinceLevelLoad - timeHoldingShoot) * throwForceMult), minThrowForce, maxThrowForce);
                //print("Send ball with force = " + force);
                PhotonNetwork.Instantiate("Hitball"
                , transform.position + transform.forward * 1.3f + transform.up * 0.2f + transform.right * 0.1f
                , Quaternion.identity, 0).GetComponent<Rigidbody>()
                .AddForce(GetComponentInChildren<Camera>().transform.forward
                    * force
                    , ForceMode.Impulse);
                timeCantShoot = Time.timeSinceLevelLoad;
                _isPrepareToThrow = false;
                CTRL_Animation.call_anim_trigger("Throw", layer: 1);
            }
            // else = ThrowCanceled
            yield return null;
        }

    }



    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //object[] objs = photonView.instantiationData; //The instantiate data..
        //bool[] mybools = (bool[])objs[0];   //Our bools!
    }

    void FixedUpdate()
    // Handle movement here since physics will only be calculated in fixed frames anyway
    {
        if (!photonView.isMine) return;

        // ? QUESAT
        if (Time.timeSinceLevelLoad > timeCantMove + durationCantMove && !PhotonNetwork.room.GetAttribute(RoomAttributes.IMMOBILIZEALL, false))
        {
            PhotonNetwork.player.SetAttribute(PlayerAttributes.ISIMMOBILIZED, false);
        }

        if (_climbing && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false))
        {
            target.drag = 999f;
        }
        else if (grounded)
        {
            target.drag = groundDrag;
        }
        else
        {
            target.drag = airDrag;
        }

        // MOVE
        if (!PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false))
        {
            move();
        }
       
    }
	
    void move()
    {
        //horizontal
        float sidestep = -(Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float horizontal = Input.GetAxis("Horizontal");
        float SidestepAxisInput = Mathf.Abs(sidestep) > Mathf.Abs(horizontal) ? sidestep : horizontal;

        Vector3 movement = Input.GetAxis("Vertical") * target.transform.forward +
            SidestepAxisInput * target.transform.right;

        // Scale down applied speed if in walk mode
        // Scale down applied speed if walking backwards
        // + INAIR
        float appliedSpeed = speed
            * (_grounded ? 1f : airmove_mult)
            / (Input.GetAxis("Vertical") < 0.0f ? walkSpeedDownscale : 1);

        if (movement.magnitude > inputThreshold)
        // Only apply movement if we have sufficient input
        {
            
            target.AddForce(movement.normalized * appliedSpeed, ForceMode.VelocityChange);
        }
        else
        // If we are grounded and don't have significant input, just stop horizontal movement
        {
            //target.velocity = new Vector3(0.0f, target.velocity.y, 0.0f);
        }
    }

    [PunRPC]
    public void rpc_immobilize()
    {
        if (PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED,false) && PhotonNetwork.player.GetPlayerState() == PlayerState.inGame) 
        {
            isCapturingThief = false;
            _isPrepareToThrow = false;
            PhotonNetwork.player.SetAttribute(PlayerAttributes.ISIMMOBILIZED, true);
            timeCantMove = Time.timeSinceLevelLoad;
            CTRL_Animation = GetComponent<AnimationController>();
            CTRL_Animation.call_anim_trigger("Confuse");
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
