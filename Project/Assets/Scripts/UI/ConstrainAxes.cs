using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainAxes : MonoBehaviour
{
	/// <summary>
	/// Normal of the plane to constrain movement to. Use Rox axes and the appropriate Unity axes will be constrained
	/// </summary>
	public Vector3 planeNormal = Vector3.forward;

	RigidbodyConstraints constraints;


	void Awake ()
	{
		if ( planeNormal != Vector3.forward && planeNormal != Vector3.right && planeNormal != Vector3.up )
		{
			Debug.LogError ( "Please use only one normalized axis for plane normal. (0,0,1), (0,1,0) or (1,0,0)." );
			constraints = RigidbodyConstraints.None;
			return;
		}

		if ( planeNormal == Vector3.forward )
			constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
		if ( planeNormal == Vector3.right )
			constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ;
		if ( planeNormal == Vector3.up )
			constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX;
	}

	void Update ()
	{
		
	}

	public void OnClick ()
	{
		if ( QuadController.ActiveController != null )
		{
			Rigidbody rb = QuadController.ActiveController.rb;
			RigidbodyConstraints rbc = rb.constraints;
			if ( rbc == constraints )
				rbc = RigidbodyConstraints.None;
			else
				rbc = constraints;
			rb.constraints = rbc;
			QuadController.ActiveController.ResetOrientation ();
			QuadController.ActiveController.ApplyMotorForce ( Vector3.zero );
			QuadController.ActiveController.ApplyMotorTorque ( Vector3.zero );
		}
	}
}