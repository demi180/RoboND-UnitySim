using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	public QuadController target;
//	public Transform target;
	public float followDistance = 5;
	public float height = 4;

	public bool autoAlign = false;
	public Vector3 forward;


	void Start ()
	{
		forward = target.Forward;
	}

	void LateUpdate ()
	{
		if ( autoAlign )
		{
			Vector3 forward = Vector3.ProjectOnPlane ( target.Force, Vector3.up ).normalized;
			Vector3 localForward = Vector3.ProjectOnPlane ( transform.forward, Vector3.up ).normalized;
			if ( Vector3.Angle ( forward, localForward ) > 90 )
			{
				//			forward = -forward;
				//			forward = transform.InverseTransformDirection ( forward );
				//			forward.z = -forward.z;
				//			forward = transform.TransformDirection ( forward );
			}
			
			transform.position = target.Position - forward * followDistance + Vector3.up * height;
			Quaternion q = Quaternion.FromToRotation ( localForward, forward );
			transform.rotation =  Quaternion.RotateTowards ( transform.rotation, q * transform.rotation, 360 * Time.deltaTime );
		} else
		{
			transform.position = target.Position - forward * followDistance + Vector3.up * height;
		}
	}
}