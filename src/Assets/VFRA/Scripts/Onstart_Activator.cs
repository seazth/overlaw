using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activateur d'objet sur initialisation de cette classe
/// </summary>
public class Onstart_Activator : MonoBehaviour {

    public GameObject[] GO_toInit;
    private void Start()
    {
        foreach (GameObject go in GO_toInit)
        {
            go.SetActive(true);
        }
    }
}
