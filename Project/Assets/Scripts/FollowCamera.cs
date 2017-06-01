using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	public Transform target;
	public Transform orientTarget;
	public float followDistance = 5;
	public float height = 4;

	Vector3 toTarget;
	Vector3 targetToTarget;
//	Vector3 targetForward;
	float initialDistance;

	void Awake ()
	{
		toTarget = target.position - transform.position;
		initialDistance = toTarget.magnitude;
		toTarget = toTarget.normalized;
//		targetToTarget = target.InverseTransformDirection (  )
//		targetForward = transform.InverseTransformDirection ( target.forward ).normalized;
//		initialDistance = ( target.position - transform.position ).magnitude;
	}

	void LateUpdate ()
	{
//		Vector3 forward = target.forward;
//		if ( forward.y < 0 )
//			forward.y = -forward.y;
		Vector3 forward = Vector3.ProjectOnPlane ( target.forward, Vector3.up ).normalized;
		Vector3 localForward = Vector3.ProjectOnPlane ( transform.forward, Vector3.up ).normalized;
		if ( Vector3.Angle ( forward, localForward ) > 90 )
		{
//			forward = -forward;
//			forward = transform.InverseTransformDirection ( forward );
//			forward.z = -forward.z;
//			forward = transform.TransformDirection ( forward );
		}

		transform.position = target.position - forward * followDistance + Vector3.up * height;
		Quaternion q = Quaternion.FromToRotation ( localForward, forward );
		transform.rotation =  Quaternion.RotateTowards ( transform.rotation, q * transform.rotation, 90 * Time.deltaTime );
	}
}