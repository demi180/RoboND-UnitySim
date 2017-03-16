using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	public Canvas canvas;
	public FPSRobotInput playerInput;
	public IRobotController[] allRobots;

	public GameObject socketObject;
	public GameObject serverObject;

	RobotRemoteControl remoteControl;

	void Awake ()
	{
		remoteControl = playerInput.GetComponent<RobotRemoteControl> ();
	}

	void Start ()
	{
		EnableCanvas ();
	}

	void EnableCanvas ()
	{
		canvas.enabled = true;
		playerInput.DisableFocus = true;
		playerInput.Unfocus ();
	}

	public void OnModeSelect (int mode)
	{
		// training
		if ( mode == 0 )
		{
			socketObject.SetActive ( false );
			serverObject.SetActive ( false );

			playerInput.DisableFocus = false;
			playerInput.Focus ();
			
//			foreach ( IRobotController robot in allRobots )
//			{
//				FollowBehavior follow = robot.GetComponent<FollowBehavior> ();
//				if ( follow != null )
//					follow.enabled = false;
//			}

			remoteControl.enabled = false;
		}

		// autonomous
		if ( mode == 1 )
		{
			socketObject.SetActive ( true );
			serverObject.SetActive ( true );

			playerInput.controllable = false;

//			foreach ( IRobotController robot in allRobots )
//			{
//				FollowBehavior follow = robot.GetComponent<FollowBehavior> ();
//				if ( follow != null )
//					follow.enabled = true;
//			}

			remoteControl.enabled = true;
		}

		canvas.enabled = false;
	}

	public void OnExitButton ()
	{
		Application.Quit ();
	}
}