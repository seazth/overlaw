using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidObstacle : ClientEntityLogic {

    public Rigidbody _body;

    void OnCollisionEnter(Collision other)
    {
        //_body.AddForce(other.impulse*100f);
    }

    protected override void Start_LocalPlayer(){}

    protected override void Update_LocalPlayer(){}
    protected override void Update_Server(){}
    protected override void Update_Client(){}
}
