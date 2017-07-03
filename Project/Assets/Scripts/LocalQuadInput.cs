using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * LocalQuadInput: queries keyboard and either applies force/torque directly to a QuadController, or to a QRKeyboardTeleop.
 * If using the Teleop, check 'useTeleop' in the inspector, and there must be a QRKeyboardTeleop component assigned.
 * If controlling a local controller otherwise, uncheck 'useTeleop' and there must be a QuadController component assigned.
 */

public class LocalQuadInput : MonoBehaviour
{
	public QuadController droneController;
	public QRKeyboardTeleop teleop;
	public bool useTeleop;

	bool motorEnabled;
	float thrust = 0;

	// Update is called once per frame
	void LateUpdate ()
	{
		float thrustInput = Input.GetAxis ( "Thrust" );
		if ( thrustInput != 0 )
			thrust = thrust += thrustInput * Time.deltaTime / 3;
		if ( Input.GetKeyDown ( KeyCode.Semicolon ) )
			thrust = 0;
		thrust = Mathf.Clamp ( thrust, -1f, 1f );

		Vector3 input = new Vector3 ( Input.GetAxis ( "Horizontal" ), thrust, Input.GetAxis ( "Vertical" ) );
		Vector3 force = new Vector3 ( 0, input.y, 0 );
		float x = input.z / 2 + input.x / 2;
		float z = input.z / 2 - input.x / 2;
		Vector3 torque = new Vector3 ( x, Input.GetAxis ( "Yaw" ), z );

		if ( useTeleop )
		{
			motorEnabled = droneController.MotorsEnabled;

			teleop.SendWrench ( force, torque );

		} else
		{
			droneController.ApplyMotorForce ( force );
			droneController.ApplyMotorTorque ( torque );
			motorEnabled = droneController.MotorsEnabled;
		}

		if ( Input.GetKeyDown ( KeyCode.R ) )
		{
			if ( useTeleop )
				teleop.TriggerReset ();
			else
				droneController.ResetOrientation ();
		}
	}
}