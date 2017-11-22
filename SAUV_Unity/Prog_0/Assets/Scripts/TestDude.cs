using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDude : Actor {

    
    public AudioSource as_main;

    //weapon parameters
    public GameObject go_Weapon;
    public GameObject go_WeaponFlash;
    public GameObject go_weaponRaycastStart;
    public Animator anim_Weapon;

    private Vector3 destHit;
    public GameObject pf_particleImpact;
    private float timeLeft_FireRate;

    public float const_moveSPD = 6f;
    public float const_fireDelay = 0.15f;
    float varf_rotationY = 0F;

    void Start()
    {

        timeLeft_FireRate = 0f;

    }

    void Update()
    {
        timeLeft_FireRate -= Time.deltaTime;
    }

    protected override void OnEnable()
    {
        HealthPoint_Max = 100f;
        HealthPoint = HealthPoint_Max;
        base.OnEnable();
    }
    public void playShootSound()
    {
        //as_main.Stop();
        as_main.pitch = Random.Range(0.7f, 1f);
        as_main.Play();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(destHit, Vector3.one * (0.5f));
    }

    public void Shoot(bool hit, RaycastHit hitInfo)
    {
        if (timeLeft_FireRate > 0f) { return; }
        GetComponent<TestDude>().playShootSound();
        timeLeft_FireRate = const_fireDelay;
        anim_Weapon.SetTrigger("Shoot");
        go_WeaponFlash.SetActive(true);
        if (hit)
        {
            destHit = hitInfo.point;
            hitInfo = new RaycastHit();
            hit = Physics.Raycast(go_weaponRaycastStart.transform.position, (destHit - go_weaponRaycastStart.transform.position).normalized, out hitInfo);
            if (hit)
            {
                GameObject.Instantiate(pf_particleImpact, hitInfo.point, new Quaternion(), MNG_GameManager.mainInstance.transform);
            }
            if (hit && hitInfo.transform.gameObject.tag == "Char")
            {
                Debug.DrawRay(go_weaponRaycastStart.transform.position, (destHit - go_weaponRaycastStart.transform.position).normalized, Color.red, 2f);
                TestDude charac = hitInfo.transform.gameObject.GetComponent<TestDude>();
                charac.takeDamage(20f);
            }
            else
            {
                Debug.DrawRay(go_weaponRaycastStart.transform.position, (destHit - go_weaponRaycastStart.transform.position).normalized, Color.cyan, 0.3f);
            }
        }

    }
    private void Update_move()
    {
        if (Input.GetKey("z")) { transform.Translate(Vector3.forward * Time.deltaTime * const_moveSPD * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1), Space.Self); }
        if (Input.GetKey("q")) { transform.Translate(Vector3.left * Time.deltaTime * const_moveSPD * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1), Space.Self); }
        if (Input.GetKey("s")) { transform.Translate(-Vector3.forward * Time.deltaTime * const_moveSPD * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1), Space.Self); }
        if (Input.GetKey("d")) { transform.Translate(-Vector3.left * Time.deltaTime * const_moveSPD * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1), Space.Self); }
    }
    private void Update_look(bool hit, RaycastHit hitInfo)
    {
        if (hit)
        {
            go_Weapon.transform.LookAt(hitInfo.point);
        }

        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * MNG_InputController.sensitivityX;
        varf_rotationY += Input.GetAxis("Mouse Y") * MNG_InputController.sensitivityY;
        varf_rotationY = Mathf.Clamp(varf_rotationY, MNG_InputController.minimumY, MNG_InputController.maximumY);
        transform.localEulerAngles = new Vector3(0, rotationX, 0);
        CTRL_Player.cam_PlayerCamera.transform.localEulerAngles = new Vector3(-varf_rotationY, 0, 0);
    }
    public override void UpdateInput()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)), out hitInfo);
        //
        Update_look(hit, hitInfo);
        Update_move();
        if (Input.GetMouseButton(0)) { Shoot(hit, hitInfo); }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.1f, 0.6f, 1.1f), 0.5f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1f, 1f, 1f), 0.5f);
        }
    }



}
