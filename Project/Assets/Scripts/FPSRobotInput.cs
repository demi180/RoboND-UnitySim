using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRobotInput : MonoBehaviour
{
	public IRobotController controller;

//	public bool allowStrafe;
//	public float hRotateSpeed;
//	public float vRotateSpeed;
//	public float moveSpeed;
//	public float cameraZoomSpeed;

	public bool controllable;

	void Start ()
	{
		Cursor.lockState = CursorLockMode.Locked;
		controllable = true;
	}

	void LateUpdate ()
	{	
		// check if we're not focused on our robot
		if ( controllable )
		{
			// check for rotation input
			float mouseX = Input.GetAxis ( "Mouse X" );
			float mouseY = Input.GetAxis ( "Mouse Y" );
			controller.Rotate ( mouseX * Time.deltaTime );
			controller.RotateCamera ( 0, mouseY );

			// check for camera zoom
			float wheel = Input.GetAxis ( "Mouse ScrollWheel" );
			if ( wheel != 0 )
				controller.ZoomCamera ( -wheel );

			// check to reset zoom
			if ( Input.GetMouseButtonDown ( 2 ) )
				controller.ResetZoom ();

			// check for movement input
			if ( controller.allowStrafe )
			{
				Vector3 move = new Vector3 ( Input.GetAxis ( "Horizontal" ), 0, Input.GetAxis ( "Vertical" ) ) * Time.deltaTime;
				move = controller.TransformDirection ( move );
//				move = controller.robotBody.TransformDirection ( move );
				controller.Move ( move );
				
			} else
			{
				float forward = Input.GetAxis ( "Vertical" );
				controller.Move ( forward );
			}

			// check for camera switch key
			if ( Input.GetKeyDown ( KeyCode.Tab ) )
				controller.SwitchCamera ();

			// check for unfocus input
			if ( Input.GetKeyDown ( KeyCode.Escape ) )
			{
				controllable = false;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				controller.Move ( 0 );
				controller.Rotate ( 0 );
				return;
			}
		}
		// check for focus input
		if ( Input.GetMouseButtonDown ( 0 ) )
		{
			Cursor.lockState = CursorLockMode.Locked;
			controllable = true;
		}

		// check for close app
		if ( Input.GetKeyDown ( KeyCode.F12 ) )
		{
			Application.Quit ();
		}
	}
}