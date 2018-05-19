using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
