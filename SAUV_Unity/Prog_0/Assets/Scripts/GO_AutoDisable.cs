using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_AutoDisable : MonoBehaviour {

    public float duration = 1f;
    public float timeLeft;
    void Start()
    {
            timeLeft = duration;
    }

    // Update is called once per frame
    void Update () {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            gameObject.SetActive(false);
            timeLeft = duration;
        }
    }
}
