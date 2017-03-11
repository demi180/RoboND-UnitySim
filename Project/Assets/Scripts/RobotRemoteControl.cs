using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotRemoteControl : MonoBehaviour
{
	public float ThrottleInput { get; set; }
	public float SteeringAngle { get; set; }
	public float VerticalAngle { get; set; }

	public FPSRobotInput manualInput;
	public IRobotController robot;
	bool useFixedUpdate;

	void Awake ()
	{
		if ( manualInput.controllable )
		{
			enabled = false;
			return;
		}
		useFixedUpdate = robot.GetType () == typeof (RoverController);
	}

	void Lateupdate ()
	{
		if ( !useFixedUpdate )
		{
			robot.Move ( ThrottleInput );
			robot.Rotate ( SteeringAngle );
			robot.RotateCamera ( 0, VerticalAngle );
		}
	}

	void FixedUpdate ()
	{
		if ( useFixedUpdate )
		{
			robot.Move ( ThrottleInput );
			robot.Rotate ( SteeringAngle );
			robot.RotateCamera ( 0, VerticalAngle );
		}
	}
}