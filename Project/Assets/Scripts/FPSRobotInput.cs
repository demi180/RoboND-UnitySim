using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRobotInput : MonoBehaviour
{
	public bool DisableFocus { get; set; }

	public IRobotController controller;
	public IRobotController follower;
	public bool controllable;



	RaycastHit rayHit;

	void Start ()
	{
//		Cursor.lockState = CursorLockMode.Locked;
//		controllable = true;
	}

	void LateUpdate ()
	{
		// check for close app
		if ( Input.GetKeyDown ( KeyCode.F12 ) )
		{
			Application.Quit ();
		}

		if ( DisableFocus )
			return;
		
		// check if we're not focused on our robot
		if ( controllable )
		{
			// check for rotation input
			float mouseX = Input.GetAxis ( "Mouse X" );
			float mouseY = Input.GetAxis ( "Mouse Y" );
//			if ( mouseX != 0 )
//				controller.Rotate ( mouseX );
			if ( Input.GetAxis ( "Horizontal" ) != 0 )
				controller.Rotate ( Input.GetAxis ( "Horizontal" ) );
//			controller.Rotate ( mouseX * Time.deltaTime * controller.hRotateSpeed );
			controller.RotateCamera ( mouseX, mouseY );

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
				if ( controller.allowSprint )
				{
//					if ( Input.GetButton ( "Sprint" ) )
						
						forward *= Mathf.Lerp ( 1, controller.sprintMultiplier, Input.GetAxis ( "Sprint" ) );
				}
				controller.Move ( forward );
			}

			// check for camera switch key
			if ( Input.GetKeyDown ( KeyCode.Tab ) )
				controller.SwitchCamera ();

			// check for unfocus input
			if ( Input.GetKeyDown ( KeyCode.Escape ) )
			{
				Unfocus ();
				return;
			}

//			Ray ray = controller.camera
//			Physics.Raycast (  )
		}
		// check for focus input
		if ( Input.GetMouseButtonDown ( 0 ) )
		{
			if ( controllable && controller.IsNearObjective )
			{
				controller.PickupObjective ( OnPickedUpObjective );
			}

			Focus ();
		}
	}

	public void Focus ()
	{
		Cursor.lockState = CursorLockMode.Locked;
		controllable = true;
	}

	public void Unfocus ()
	{
		controllable = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		controller.Move ( 0 );
		controller.Rotate ( 0 );
	}

	void OnPickedUpObjective (GameObject objective)
	{
		follower.CarryObjective ( objective );
	}
}