using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MonoBehaviourExtended : MonoBehaviour
{
    NES _networkState;
    public static string str_name { get; protected set; }

    // Propriété de destruction
    public float HealthPoint;
    public float HealthPoint_Max { get; protected set; }
    public void takeDamage(float damageAmount) { HealthPoint -= damageAmount; if (HealthPoint <= 0) { Die(); } }
    private void Die() { }

    // Propriété d'initialisation
    public bool isInitialized { get; private set; }
    public virtual void Initialise()
    {
        _networkState = GetComponent<NES>();
        str_name = this.GetType().Name;
        gameObject.name = str_name + "[local:Initialised]";
        isInitialized = true;
    }
    protected virtual void OnEnable()
    {
        if (!isInitialized)
        {
            gameObject.SetActive(false);
            this.enabled = false;
            throw new System.Exception("[" + gameObject.name + "] ne peut être activé tant qu'il n'est pas initialisé !");
        }
        this.enabled = true;
    }

}


public class Actor : MonoBehaviourExtended
{


    protected override void OnEnable()
    {
        HealthPoint = HealthPoint_Max;
        if (HealthPoint_Max <= 0) {gameObject.SetActive(false);this.enabled = false;}
        Initialise();
        base.OnEnable();
    }
    public virtual void UpdateInput() { }
}

public static class MNG_InputController
{
    //Input parameters
    public static float sensitivityX = 10F;
    public static float sensitivityY = 5F;
    public static float minimumX = -360F;
    public static float maximumX = 360F;
    public static float minimumY = -60F;
    public static float maximumY = 60F;
}

