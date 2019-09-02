using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyManipulator : MonoBehaviour {

	Rigidbody body;
	void Start ()
	{
		GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Infinity;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
