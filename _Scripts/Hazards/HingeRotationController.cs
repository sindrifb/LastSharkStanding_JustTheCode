using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeRotationController : MonoBehaviour
{
    public float Force;
    private Vector3 Target;
    private Rigidbody Rb;

	// Use this for initialization
	void Start ()
    {
        Rb = GetComponent<Rigidbody>();
        Target = transform.forward;
	}
	
	// Update is called once per frame
	void Update ()
    {
        var angle = Vector3.SignedAngle(Target, transform.forward, transform.right);
        //print(angle);
        Rb.AddTorque(transform.forward * angle * Time.deltaTime, ForceMode.Acceleration);
	}
}
