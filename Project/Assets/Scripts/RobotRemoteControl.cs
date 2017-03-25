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
		useFixedUpdate = robot.GetType () == typeof (RoverController);
		if ( manualInput.controllable )
		{
			enabled = false;
			return;
		}
	}

	void LateUpdate ()
	{
//		if ( !useFixedUpdate )
//		{
			float throttle = ThrottleInput;
			float steer = SteeringAngle;
			robot.Move ( throttle );
			robot.Rotate ( steer );
			robot.RotateCamera ( 0, VerticalAngle );
//		}
	}

//	void FixedUpdate ()
//	{
//		if ( useFixedUpdate )
//		{
//			float throttle = ThrottleInput;
//			float steer = SteeringAngle;
//			robot.Move ( throttle );
//			robot.Rotate ( steer );
//			robot.RotateCamera ( 0, VerticalAngle );
//		}
//	}
}