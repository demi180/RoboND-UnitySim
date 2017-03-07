using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
	public Transform robotBody;
	public Transform cameraHAxis;
	public Transform cameraVAxis;
	public Transform fpsPosition;
	public Transform tpsPosition;
	public Transform actualCamera;
	public Camera camera;
	public CharacterController controller;

	public float cameraMinAngle;
	public float cameraMaxAngle;
	public float cameraMinFOV;
	public float cameraMaxFOV;

	int curCamera;

	void Awake ()
	{
		actualCamera.SetParent ( fpsPosition );
	}

	public void Move (float speed)
	{
		controller.SimpleMove ( robotBody.forward * speed );
	}

	public void Move (Vector3 direction)
	{
		direction.y = Physics.gravity.y * Time.deltaTime;
		controller.Move ( direction );
	}

	public void Rotate (float angle)
	{
		robotBody.Rotate ( Vector3.up * angle );
	}

	public void RotateCamera (float horizontal, float vertical)
	{
//		cameraHAxis.Rotate ( Vector3.up * horizontal, Space.World );
//		cameraVAxis.Rotate ( Vector3.right * -vertical, Space.Self );
		Vector3 euler = cameraVAxis.localEulerAngles;
		euler.x -= vertical;
		if ( euler.x > 270 )
			euler.x -= 360;
		euler.x = Mathf.Clamp ( euler.x, cameraMinAngle, cameraMaxAngle );
		cameraVAxis.localEulerAngles = euler;
	}

	public void ZoomCamera (float amount)
	{
		camera.fieldOfView += amount;
		camera.fieldOfView = Mathf.Clamp ( camera.fieldOfView, cameraMinFOV, cameraMaxFOV );
	}

	public void ResetZoom ()
	{
		camera.fieldOfView = 60;
//		camera.ResetFieldOfView ();
	}

	public void SwitchCamera ()
	{
		if ( curCamera == 0 )
		{
			curCamera = 1;
			actualCamera.SetParent ( tpsPosition, false );
		} else
		{
			curCamera = 0;
			actualCamera.SetParent ( fpsPosition, false );
		}
	}
}