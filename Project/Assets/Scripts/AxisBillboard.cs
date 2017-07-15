using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisBillboard : MonoBehaviour
{
	public Transform xMove;
	public Transform xRot;
	public Transform yMove;
	public Transform yRot;
	public Transform zMove;
	public Transform zRot;
	public Transform cam;

	Transform tr;
	QuadController quad;

	Quaternion xmInitial;

	void Awake ()
	{
		tr = transform;
		xmInitial = Quaternion.Inverse ( xMove.rotation );
	}

	void Start ()
	{
		cam = FollowCamera.ActiveCamera.transform;
	}

	void LateUpdate ()
	{
		quad = QuadController.ActiveController;
		tr.position = quad.Position;

		// x move arrow
		Vector3 toCamera = Vector3.ProjectOnPlane ( cam.position - xMove.position, Vector3.forward ).normalized;
//		Debug.DrawRay ( xMove.position, xMove.forward, Color.blue );
//		Debug.DrawRay ( xMove.position, toCamera, Color.magenta );
		Quaternion look = Quaternion.FromToRotation ( xMove.forward, toCamera );
		if ( Vector3.Angle ( xMove.forward, toCamera ) > 0 )
			xMove.rotation = look * xMove.rotation;

		// y move arrow
		toCamera = Vector3.ProjectOnPlane ( cam.position - yMove.position, -Vector3.right ).normalized;
		look = Quaternion.FromToRotation ( yMove.forward, toCamera );
		if ( Vector3.Angle ( yMove.forward, toCamera ) > 0 )
			yMove.rotation = look * yMove.rotation;

		// z move arrow
		toCamera = Vector3.ProjectOnPlane ( cam.position - zMove.position, Vector3.up ).normalized;
		look = Quaternion.FromToRotation ( zMove.forward, toCamera );
		if ( Vector3.Angle (zMove.forward, toCamera ) > 0 )
			zMove.rotation = look * zMove.rotation;
	}
}