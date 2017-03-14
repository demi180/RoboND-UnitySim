using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBehavior : MonoBehaviour
{
	public IRobotController thisRobot;
	public IRobotController followTarget;
	public float minFollowDistance = 3;

	Transform followTransform;
	Transform thisTransform;

	void Awake ()
	{
		if ( followTarget == null )
			enabled = false;
	}

	void Start ()
	{
		followTransform = followTarget.robotBody;
		thisTransform = thisRobot.robotBody;
		thisRobot.camera.enabled = false;
	}

	void Update ()
	{
		Vector3 toTarget = followTransform.position - thisTransform.position;
		toTarget.y = 0;
		if ( toTarget.magnitude > minFollowDistance )
		{
			toTarget = toTarget.normalized;
			Vector3 forward = thisTransform.forward;
			forward.y = 0;
			forward = forward.normalized;
			float angleToTarget = Vector3.Angle ( forward, toTarget );
			Vector3 localForward = thisTransform.InverseTransformDirection ( toTarget );
			thisRobot.Rotate ( angleToTarget / thisRobot.hRotateSpeed * Mathf.Sign ( localForward.x ) );
//			if ( angleToTarget > 0.1f )
//				thisRobot.Rotate ( Mathf.Min ( angleToTarget, thisRobot.hRotateSpeed ) * Time.deltaTime * Mathf.Sign ( localForward.x ) );
//			Debug.Log ( "angle is " + angleToTarget + " localforward x is " + localForward.x );
			Debug.DrawRay ( thisTransform.position, forward, Color.blue );
			Debug.DrawRay ( thisTransform.position, toTarget, Color.red );
		}


	}
}