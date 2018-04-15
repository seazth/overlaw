using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ThirdPersonControllerNET))]
public class AnimationController : Photon.MonoBehaviour
{
	enum CharacterState
	{
		Normal,
		Jumping,
		Falling,
		Landing
	}
	
	
	public Animation target;
		// The animation component being controlled
	new public Rigidbody rigidbody;
		// The rigidbody movement is read from
	public Transform root, spine, hub;
		// The animated transforms used for lower body rotation
	public float
		walkSpeed = 0.2f,
		runSpeed = 1.0f,
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
	private Vector3 lastRootForward;


    public Transform rightArmRig;
    public Vector3 pos_ThrowRightArm = new Vector3(-0.125f,-4.77f,0f);
    public Vector3 rot_ThrowRightArm =  new Vector3(-1.57f,40.58f,69f);
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
	// Run setup on component attach, so it is visually more clear which references are used
	{
		Setup ();
	}
	
	
	void Setup ()
	// If target or rigidbody are not set, try using fallbacks
	{
		if (target == null) target = GetComponent<Animation> ();
		if (rigidbody == null) rigidbody = GetComponent<Rigidbody> ();
        charpointed = null;
        isPointed = false;
        print("isPointed = false FOR " + gameObject.name);
    }
	
	
	void Start ()
	// Verify setup, configure
	{
		Setup ();
			// Retry setup if references were cleared post-add
			
		if (VerifySetup ())
		{
			controller = GetComponent<ThirdPersonControllerNET> ();
			controller.onJump += OnJump;
				// Have OnJump invoked when the ThirdPersonController starts a jump
			currentRotation = 0.0f;
			lastRootForward = root.forward;
		}
	}
	bool VerifySetup ()
	{
		return VerifySetup (target, "target") &&
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
		
		Invoke ("Fall", target["Jump"].length);
	}
	void OnLand ()
	// Start a landing
	{
		canLand = false;
		state = CharacterState.Landing;
		
		Invoke (
			"Land",
			target["Land"].length * (HorizontalMovement.magnitude < walkSpeed ? 1.0f : runningLandingFactor)
				// Land quicker if we're moving enough horizontally to start walking after landing
		);
	}
	void Fall ()
	// End a jump and transition to a falling state (ignore if already grounded)
	{
		if (controller.grounded)
		{
			return;
		}
		state = CharacterState.Falling;
	}
	void Land ()
	// End a landing and transition to normal animation state (ignore if not currently landing)
	{
		if (state != CharacterState.Landing)
		{
			return;
		}
		state = CharacterState.Normal;
	}
	void FixedUpdate ()
	// Handle changes in groundedness
	{
		if (controller.grounded)
		{
			if (state == CharacterState.Falling || (state == CharacterState.Jumping && canLand))
			{
				OnLand ();
			}
		}
		else if (state == CharacterState.Jumping)
		{
			canLand = true;
		}

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
                    print("Pointing now >> " + charpointed.name);
                }
            }
            //else if (charpointed != null) charpointed.isPointed = false;
        }
        else if (charpointed != null) { charpointed.isPointed = false; charpointed = null; }
    }


	void Update ()
	// Animation control
	{
		switch (state)
		{
			case CharacterState.Normal:
				Vector3 movement = HorizontalMovement; 
			
				if (movement.magnitude < walkSpeed)
				{
					if (Vector3.Angle (lastRootForward, root.forward) > 1.0f)
					// If the character has rotated on the spot, shuffle his feet a bit
					{
						target.CrossFade ("Shuffle");
						
						lastRootForward = Vector3.Slerp (lastRootForward, root.forward, Time.deltaTime * shuffleSpeed);
					}
					else
					{
						target.CrossFade ("Idle");
					}
				}
				else
				{
					target["Walk"].speed = target["Run"].speed =
						Vector3.Angle (root.forward, movement) > 91.0f ? -1.0f : 1.0f;
						// If the direction if backwards, play the animations backwards
					
					if (movement.magnitude < runSpeed)
					{
						target.CrossFade ("Walk");
					}
					else
					{
						target.CrossFade ("Run");
					}
					
					lastRootForward = root.forward;
				}
			break;
			case CharacterState.Jumping:
				target.CrossFade ("Jump");
			break;
			case CharacterState.Falling:
				target.CrossFade ("Fall");
			break;
			case CharacterState.Landing:
				target.CrossFade ("Land");
			break;
		}

    
    }
	void LateUpdate ()
	// Apply directional rotation of lower body
	{
        if (controller.isPrepareToThrow)
        {
            //rightArmRig.localPosition = pos_ThrowRightArm;
            rightArmRig.localRotation = Quaternion.Euler(rot_ThrowRightArm);

            ball.SetActive(true);
        }
        else {
            ball.SetActive(false);
        }
        float targetAngle = 0.0f;
		
		Vector3 movement = HorizontalMovement;
		
		if (movement.magnitude >= walkSpeed)
		// Only calculate the target angle if we're moving sufficiently
		{
			targetAngle = Vector3.Angle (movement, new Vector3 (root.forward.x, 0.0f, root.forward.z));
			
			if (Vector3.Angle (movement, root.right) > Vector3.Angle (movement, root.right * -1))
			// Negative rotation if shortest route is counter-clockwise
			{
				targetAngle *= -1.0f;
			}
			
			if (Mathf.Abs (targetAngle) > 91.0f)
			// When walking backwards, don't rotate over 90 degrees and rotate opposite
			{
				targetAngle = targetAngle + (targetAngle > 0 ? -180.0f : 180.0f);
			}
		}
		
		currentRotation = Mathf.Lerp (currentRotation, targetAngle, Time.deltaTime * rotationSpeed);
			// Update our current rotation score
		
		hub.RotateAround (hub.position, root.up, currentRotation);
			// Rotate the dude
		spine.RotateAround (spine.position, root.up, currentRotation * -1.0f);
			// Rotate the upper-body to face forward
	}


}
