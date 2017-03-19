using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmActuator : MonoBehaviour
{
	public Transform baseTransform;
	public Transform shoulder;
	public Transform upperArm;
	public Transform elbow;
	public Transform forearm;
	public Transform wrist;
	public Transform target;
	public float rotationSpeed = 360;

	public Quaternion[] storedRotations;
	public Vector3[] storedPositions;

	void OnEnable ()
	{
		

	}

	void OnDisable ()
	{
		
	}

	void Awake ()
	{
		storedRotations = new Quaternion[6];
		storedPositions = new Vector3[6];
		storedRotations [ 0 ] = baseTransform.rotation;
		storedPositions [ 0 ] = baseTransform.position;
		storedRotations [ 1 ] = shoulder.rotation;
		storedPositions [ 1 ] = shoulder.position;
		storedRotations [ 2 ] = upperArm.rotation;
		storedPositions [ 2 ] = upperArm.position;
		storedRotations [ 3 ] = elbow.rotation;
		storedPositions [ 3 ] = elbow.position;
		storedRotations [ 4 ] = forearm.rotation;
		storedPositions [ 4 ] = forearm.position;
		storedRotations [ 5 ] = wrist.rotation;
		storedPositions [ 5 ] = wrist.position;
	}

	void Update ()
	{
		if ( target == null )
			return;

		// shoulder
		Vector3 toTarget = ( target.position - shoulder.position ).normalized;
		toTarget = Vector3.ProjectOnPlane ( toTarget, Vector3.up ).normalized;
		Quaternion targetRot = Quaternion.LookRotation ( toTarget ) * storedRotations [ 1 ];
		shoulder.rotation = Quaternion.RotateTowards ( shoulder.rotation, targetRot, rotationSpeed * Time.deltaTime );
		Debug.DrawLine ( shoulder.position, shoulder.position + toTarget, Color.blue );

		// upper arm
//		toTarget = ( target.position - upperArm.position ).normalized;
//		toTarget = Vector3.ProjectOnPlane ( toTarget, upperArm.right ).normalized;
//		targetRot = Quaternion.LookRotation ( toTarget ) * storedRotations [ 2 ] * shoulder.rotation;
//		upperArm.rotation = Quaternion.RotateTowards ( upperArm.rotation, targetRot, rotationSpeed * Time.deltaTime );
//		Debug.DrawLine ( upperArm.position, upperArm.position + toTarget, Color.blue );
	}
}