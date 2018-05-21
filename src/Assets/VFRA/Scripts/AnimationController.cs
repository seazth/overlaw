using UnityEngine;
using System.Collections;

/// <summary>
/// Cette classe est repris d'un exemple de photon.
/// Entièrement recustomisé, cette classe n'est pas assez solide, il faut encore changer les triggers en float pour le contrôleur d'animation Unity.
/// En effet, les triggers sont très mal développé chez Unity et les transitions d'animations fait bugger toutes les animations rapide de notre jeu.
/// </summary>
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
	private MNG_CameraController cameracontroller;
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
		if (cameracontroller == null) cameracontroller = GetComponent<MNG_CameraController> ();
        charpointed = null;
        isPointed = false;
    }

    /// <summary>
    /// On configure les comportements de saut et les variables
    /// </summary>
    void Start()
    {
        Setup(); // Retry setup if references were cleared post-add
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
    // Handle  Animation states
    {
        if (controller.grounded)
        {
            if (state == CharacterState.Falling || (state == CharacterState.Jumping && canLand)) { OnLand(); }
        }
        else if (state == CharacterState.Jumping)
        {
            canLand = true;
        }
        if (photonView.isMine && photonView.owner !=null && photonView.owner.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false))
        {
            call_anim_trigger("Confuse");
        }
        else if (state == CharacterState.Normal && !PhotonNetwork.player.GetAttribute(PlayerAttributes.ISIMMOBILIZED, false))
        { 
            Vector3 movement = HorizontalMovement;
            float angle = Vector3.Angle(movement, transform.forward);
            if (movement.magnitude < walkSpeed)
                call_anim_trigger("Idle");
            else if (angle > 45f && angle < 135f)
                call_anim_trigger("Strafe");
            else if (movement.magnitude < runSpeed )
                call_anim_trigger("Walk");
            else
                call_anim_trigger("Run"); 
        } 
        checkPointingChar();

        ball.SetActive(_animator.GetCurrentAnimatorStateInfo(1).IsName("PrepareThrow"));
        if (photonView.isMine)
        {
            if (_animator.GetCurrentAnimatorStateInfo(2).IsName("Climb")) cameracontroller.lockTransfomRotX = true;
            else cameracontroller.lockTransfomRotX = false;
        }
    }


    /// <summary>
    /// Ici on regarde si le joueur pointe un autre joueur de son viseur.
    /// Si c'est le cas, le joueur visé.
    /// </summary>
    void checkPointingChar()
    {
        if (!photonView.isMine) return;

        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(transform.forward * 0.3f + transform.position, transform.forward, out hitInfo, 3f, LayerMask.GetMask("NetEntity"));
        if (hit && hitInfo.transform.gameObject.tag == "Player"
            && hitInfo.transform.GetComponent<PhotonView>().owner != null)
        {
            int target_tid = hitInfo.transform.GetComponent<PhotonView>().owner.getTeamID();
            if (target_tid == 1 || target_tid == 2)
            {
                if (charpointed != hitInfo.transform.GetComponent<AnimationController>())
                {
                    if (charpointed != null) charpointed.isPointed = false;
                    charpointed = hitInfo.transform.GetComponent<AnimationController>();
                    charpointed.isPointed = true;
                }
            }
        }
        else if (charpointed != null) { charpointed.isPointed = false; charpointed = null; }
    }

    //--------------------//

    string lasttrigger="";
    /// <summary>
    /// Cette méthode permet de faire la gestion de l'animator d'Unity
    /// et de gérer correctement les couches d'animations sur l'avatar du joueur.
    /// Ici on remet zero le trigger concerné  a chaque nouvelle animation afin d'éviter
    /// des bugs liés aux doubles appels d'animations.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="statename"></param>
    /// <param name="layer"></param>
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

    /// <summary>
    /// Appelle la synchronisation des animations dans le réseau.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="statename"></param>
    /// <param name="layer"></param>
    public void call_anim_trigger(string trigger, string statename = null, int layer = 0)
    {
        photonView.RPC("rpc_anim_trigger", PhotonTargets.All, new object[] { trigger ,statename,layer});
    }

    /// <summary>
    /// Applique l'animation appelé sur le réseau.
    /// </summary>
    /// <param name="parameters"></param>
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
}
