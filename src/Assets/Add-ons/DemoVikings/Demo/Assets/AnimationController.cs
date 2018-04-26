using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ThirdPersonControllerNET))]
public class AnimationController : Photon.MonoBehaviour
{
    public Animator _animator;
    enum CharacterState
    {
        Normal,
        Jumping,
        Falling,
        Landing
    }
	
		// The animation component being controlled
	new public Rigidbody rigidbody;
		// The rigidbody movement is read from
	public Transform root, spine, hub;
		// The animated transforms used for lower body rotation
	public float
		walkSpeed = 0.5f,
            // xx
		runSpeed = 2.0f,
			// Walk and run speed dictate at which rigidbody velocity, the animation should blend
		rotationSpeed = 6.0f,
			// The speed at which the lower body should rotate
		shuffleSpeed = 7.0f,
			// The speed at which the character shuffles his feet back into place after an on-the-spot rotation
		runningLandingFactor = 0.2f;
			// Reduces the duration of the landing animation when the rigidbody has hoizontal movement
	
	
	private ThirdPersonControllerNET controller;
	private CharacterState state = CharacterState.Falling;
	private bool canLand = true;
	private float currentRotation;


    public GameObject ball;

    private Vector3 HorizontalMovement
	{
		get
		{
			return new Vector3 (rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
		}
	}


    private bool _isPointed = false;
    public bool isPointed { get { return _isPointed; } set { _isPointed = value; outline_script.enabled = value; } }
    public cakeslice.Outline outline_script;
    public AnimationController charpointed;

    void Reset ()
	{
		Setup ();
	}
	
	
	void Setup ()
	// If target or rigidbody are not set, try using fallbacks
	{
		if (rigidbody == null) rigidbody = GetComponent<Rigidbody> ();
        charpointed = null;
        isPointed = false;
        print("isPointed = false FOR " + gameObject.name);
    }


    void Start()
    // Verify setup, configure
    {

        Setup();
        // Retry setup if references were cleared post-add

        if (VerifySetup())
        {
            controller = GetComponent<ThirdPersonControllerNET>();
            controller.onJump += OnJump;
            // Have OnJump invoked when the ThirdPersonController starts a jump
            currentRotation = 0.0f;
        }
    }
	bool VerifySetup ()
	{
		return 
			VerifySetup (rigidbody, "rigidbody") &&
			VerifySetup (root, "root") &&
			VerifySetup (spine, "spine") &&
			VerifySetup (hub, "hub");
	}
	bool VerifySetup (Component component, string name)
	{
		if (component == null)
		{
			Debug.LogError ("No " + name + " assigned. Please correct and restart.");
			enabled = false;
			return false;
		}
		return true;
	}
	
	
	void OnJump ()
	// Start a jump
	{
		canLand = false;
		state = CharacterState.Jumping;
    }
	void OnLand ()
	// Start a landing
	{
		canLand = false;
		state = CharacterState.Landing;
        Land();
	}



    void Land ()
	// End a landing and transition to normal animation state (ignore if not currently landing)
	{
		if (state != CharacterState.Landing) return;
		state = CharacterState.Normal;
        call_anim_trigger("Land");
    }

    void FixedUpdate()
    // Handle changes in groundedness
    {
        if (controller.grounded)
        {
            if (state == CharacterState.Falling || (state == CharacterState.Jumping && canLand)) { OnLand(); }
        }
        else if (state == CharacterState.Jumping)
        {
            canLand = true;
        }

        if (state == CharacterState.Normal && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false))
        { 
            Vector3 movement = HorizontalMovement;
            float angle = Vector3.Angle(movement, transform.forward);
            print(angle);
            if (movement.magnitude < walkSpeed)
                SetAnimTrigger("Idle");
            else if (angle > 45f && angle < 135f)
                SetAnimTrigger("Strafe");
            else if (movement.magnitude < runSpeed )
                SetAnimTrigger("Walk");
            else
                SetAnimTrigger("Run"); 
        }

        checkPointingChar();
    }

    void checkPointingChar()
    {
        if (!photonView.isMine) return;

        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(transform.forward * 0.3f + transform.position, transform.forward, out hitInfo, 3f, LayerMask.GetMask("NetEntity"));
        if (hit
            && hitInfo.transform.gameObject.tag == "Player"
            && hitInfo.transform.GetComponent<PhotonView>().owner != null)
        {
            int target_tid = hitInfo.transform.GetComponent<PhotonView>().owner.getTeamID();
            if (//target_tid != PhotonNetwork.player.getTeamID() && 
                (target_tid == 1 || target_tid == 2))
            {
                if (charpointed != hitInfo.transform.GetComponent<AnimationController>())
                {
                    if (charpointed != null) charpointed.isPointed = false;
                    charpointed = hitInfo.transform.GetComponent<AnimationController>();
                    charpointed.isPointed = true;
                }
            }
            //else if (charpointed != null) charpointed.isPointed = false;
        }
        else if (charpointed != null) { charpointed.isPointed = false; charpointed = null; }
    }

    //--------------------//

    string lasttrigger="";
    void SetAnimTrigger( string trigger,string statename = null, int layer=0)
    {
        _animator.ResetTrigger(lasttrigger); // SUPER IMPORTANT
        if (!_animator.GetCurrentAnimatorStateInfo(layer).IsName(statename==null?trigger: statename))
        {
            //print("ANIM SET : "+ trigger);
            _animator.SetTrigger(trigger);
            lasttrigger = trigger;
        }
    }

    public void call_anim_trigger(string trigger, string statename = null, int layer = 0)
    {
        gameObject.transform.GetComponent<PhotonView>().RPC("rpc_anim_trigger", PhotonTargets.All, new object[] { trigger ,statename,layer});
    }

    [PunRPC]
    public void rpc_anim_trigger(object[] parameters)
    {
        string triggername = (string)parameters[0];
        string statename = (string)parameters[1];
        int layer = (int)parameters[2];
        if (PhotonNetwork.player.GetAttribute(PlayerAttributes.HASSPAWNED, false) && PhotonNetwork.player.GetPlayerState() == PlayerState.inGame)
        {
            SetAnimTrigger(triggername, statename, layer);
        }
    }

    //----------------//


    void LateUpdate ()
	// Apply directional rotation of lower body
	{
        ball.SetActive(_animator.GetCurrentAnimatorStateInfo(1).IsName("PrepareThrow"));
	}

}
