using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;
/// <summary>
/// Premier personnage jouable
/// </summary>
public class ent_Character : ClientEntityLogic
{
    /* On veut plusieurs paramètres principaux pour gérer le déplacement d'un joueur. 
     *  - La gestion de la gravité avec une acceleration et une vitesse max
     *  - Un modificateur de vitesse de déplacement en étant en hauteur
     *  - Une acceleration et une deceleration du joueur sur une plateforme
     *  - Reconnaitre quand le joueur quitte une plateforme
     *  - le joueur doit pouvoir se déplacer sur des plateformes incliné, tel des escaliers
     *  - Il nous faut aussi une gestion de sprint, sans endurance pour l'instant
     * 
     * PS: A mon avis tu devras utiliser un Collider spécifique à la detection d'une plateforme
    */
    public float mov_jump = 300f;
    public float mov_acceleration = 50f;
    public float mov_gravity = 60f;
    public float mov_aircontrol = 0.15f;
    public float mov_maxspeed = 100f;
    public float mov_deceleration = 60f;
    public bool inAir;


    float prev_speed;

    public Rigidbody _body;

    // On ajoute une Caméra quand un joueur local est créé
    //protected override void Start(){base.Start();}

    // On ajoute un comportement après la desactivation de l'entité du joueur local : 
    // on active la caméra de la scene par défaut.
    protected override void OnDisable()
    {
        base.OnDisable();
        if (_NES._entityType == EntityType.Player && _NES.isLocalPlayer )
            MNG_GameManager.setStdCameraAsActive(false);
    }

    /// <summary>
    /// Fonction Update qui execute du code local sur l'entité du joueur local.
    /// </summary>
    protected override void Update_LocalPlayer()
    {
        //
        if (MNG_GameManager.captureInput)// ici     
        {


                _body.AddForce(
                    ((transform.rotation * Vector3.forward * mov_acceleration * mov_maxspeed  * (Input.GetKey(KeyCode.Z) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f))
                    + (transform.rotation * Vector3.left * mov_acceleration * mov_maxspeed * (Input.GetKey(KeyCode.Q) ? 1f : Input.GetKey(KeyCode.D) ? -1f : 0f)))
                    * ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.S)) && (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.D)) ? 0.7f : 1f)
                    * (inAir ? mov_aircontrol : 1f) * (mov_maxspeed-_body.velocity.magnitude)/ mov_maxspeed * Time.deltaTime
                );
            
            //if ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.D))&& ! inAir) _body.AddForce(Vector3.up * mov_gravity*0.5f);
            if (Input.GetKeyDown(KeyCode.Space) && !inAir)
            {
                _body.AddForce(Vector3.up * mov_jump);
                //inAir = true;
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)), out hitInfo);
                if (hit && hitInfo.transform.gameObject.tag == "MP_Interactable")
                {
                    //Debug.DrawRay(go_weaponRaycastStart.transform.position, (destHit - go_weaponRaycastStart.transform.position).normalized, Color.red, 2f);
                    //target._NES._data.activated = true;
                    print("HIT = "+ hitInfo.transform.parent.parent.name);
                    _NES.Interact(hitInfo.transform.parent.parent.GetComponentInChildren<NES>()); 
                }
            }

            float roty = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * MNG_InputController.sensitivityX;
            transform.localEulerAngles = new Vector3(0, roty, 0);
            Transform camTrans = transform.GetComponentInChildren<Camera>().transform;
            float rotx = camTrans.localEulerAngles.x - Input.GetAxis("Mouse Y") * MNG_InputController.sensitivityY;
            //rotx = Mathf.Clamp(rotx, -20f, 20f);
            camTrans.localEulerAngles = new Vector3(rotx, 0f, 0f);

        }
        _body.velocity = new Vector3(
              _body.velocity.x * mov_deceleration
            , _body.velocity.y - mov_gravity
            , _body.velocity.z * mov_deceleration);
    }

    protected override void Update()
    {
        base.Update();
        
    }

    // on vérifie si la collision entrante est une plateforme en dessous du joueur.
    void OnCollisionEnter(Collision other)
    {
        if (inAir)
        {
            Vector3 q;
            for (int i = 0; i < other.contacts.Length; i++)
            {
                q = Quaternion.LookRotation(other.contacts[i].normal).eulerAngles;
                if (q.x % 180 > 90-45 && q.x % 180 < 90+45) inAir = false;
            }
        }
    }
    protected override void Start()
    {
        base.Start();
        inAir = false;
        if (_NES._networkSide==NetworkSide.Server) { _body.isKinematic = true; }
    }

    protected override void Start_Server() 
    {
        base.Start_Server();
         _body.isKinematic = true;
        //Instantiate<NavMeshAgent>(GlobalAssets.mainInstance.gop_defaultNavMesh);
        InvokeRepeating("chooseDestination", 0, 3f);
    }

    protected override void Start_LocalPlayer()
    {
        //GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<NavMeshAgent>().enabled = false;
    }

    protected void chooseDestination()
    {
        Vector3 random2D;
        random2D = (20 * UnityEngine.Random.insideUnitCircle);
        GetComponent<NavMeshAgent>().SetDestination(transform.position + new Vector3(random2D.x, 0f, random2D.y));
    }

    protected override void Update_Server(){
        if(_NES._entityType != EntityType.Player)
            _body.velocity = new Vector3(
              _body.velocity.x * mov_deceleration
            , _body.velocity.y - mov_gravity
            , _body.velocity.z * mov_deceleration);
    }
    protected override void Update_Client(){}

    void OnGUI()
    {
        if (_NES.isLocalPlayer)
        {
            prev_speed = (prev_speed + Vector3.Project(_body.velocity, transform.forward).magnitude) / 2f;
            GUI.Label(new Rect(5, Screen.height - 20, 150, 20), "Player Speed : " + (int)prev_speed);
        }
    }

}
