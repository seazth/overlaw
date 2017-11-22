using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotScript : MonoBehaviour {
    // Use this for initialization
    Vector3 dest;


    void Start () {
        gotoNewDestination();
    }
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(transform.position, dest) < 2f)
        {
            //Debug.Log("NewDest");
            gotoNewDestination();
        }
	}

    public void gotoNewDestination()
    {
        dest = new Vector3(Random.Range(-49f, 49f), 0f, Random.Range(-49f, 49f));
        GetComponent<NavMeshAgent>().SetDestination(dest);
    }
}
