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
	Vector3 toCamera;
	Vector3 targetForward;
	Quaternion initialRot;

	void Awake ()
	{
		toTarget = target.InverseTransformDirection ( target.position - transform.position ).normalized;
		toCamera = target.InverseTransformDirection ( transform.position - target.position ).normalized;
		targetForward = transform.InverseTransformDirection ( target.forward ).normalized;
		initialRot = Quaternion.Inverse ( transform.rotation );
	}

	void LateUpdate ()
	{
		Vector3 followPoint = target.position + Vector3.up * height;
		Vector3 forward = target.position - transform.position;
		forward.y = 0;
		forward = forward.normalized * followDistance;
//		Vector3 forward = target.up.y >= 0 ? target.forward : Vector3.Reflect ( target.forward, Vector3.up );
//		forward *= followDistance;
//		Vector3 forward = orientTarget.forward * followDistance;
//		Vector3 forward = target.up.y > 0 ? transform.TransformDirection ( targetForward ) : Vector3.ProjectOnPlane ( target.up, Vector3.up );
//		forward *= followDistance;
//		forward = new Vector3 ( forward.x, 0, forward.z ).normalized * followDistance;
//		Vector3 forward = transform.TransformDirection (
//			transform.up.y > 0 ? targetForward :
//			transform.up.y < 0 ? target.up :
//			target.up
//		) * followDistance;
//		Vector3 forward = target.TransformDirection ( toTarget ) * followDistance;
		transform.position = target.position - forward + Vector3.up * height;
//		transform.position = followPoint - forward;
//		transform.rotation = Quaternion.LookRotation ( forward.normalized );
//		transform.LookAt ( target, Vector3.up );
		transform.LookAt ( followPoint, Vector3.up );
//		transform.rotation = orientTarget.rotation;
//		Vector3 euler = transform.eulerAngles;
//		euler.y = target.eulerAngles.y;
//		transform.eulerAngles = euler;
		Debug.DrawRay ( target.position, forward, Color.red );
		Debug.DrawRay ( followPoint, Vector3.up, Color.green );
	}
}