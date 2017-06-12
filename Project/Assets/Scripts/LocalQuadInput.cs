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
		float x = input.x / 2 - input.z / 2;
		float z = input.x / 2 + input.z / 2;
//		float x = input.z / 2 - input.x / 2;
//		float z = input.z / 2 + input.x / 2;
		Vector3 torque = new Vector3 ( x, Input.GetAxis ( "Yaw" ), z );

		torque = new Vector3 ( Input.GetAxis ( "Roll" ), Input.GetAxis ( "Yaw" ), Input.GetAxis ( "Pitch" ) );

		if ( useTeleop )
		{
//			if ( Input.GetKeyDown ( KeyCode.Return ) )
//				teleop.enableMotors ( !motorEnabled );
			motorEnabled = droneController.MotorsEnabled;

			teleop.SendWrench ( force, torque );
//			teleop.SendTwist ( force, torque );

		} else
		{
			if ( Input.GetKeyDown ( KeyCode.Return ) )
				droneController.MotorsEnabled = !motorEnabled;
			
			droneController.ApplyMotorForce ( force.x, force.y, force.z );
			droneController.ApplyMotorTorque ( torque.x, torque.y, torque.z );
			motorEnabled = droneController.MotorsEnabled;
		}
	}


/*	void OnGUI ()
	{
		Rect r = new Rect ( 10, 10, 180, 200 );
		GUI.Box ( r, "" );
		GUI.Box ( r, "" );
		r.x = 15;
		r.height = 20;
		GUI.Label ( r, "Motors enabled: <color=yellow>" + motorEnabled + "</color>" );
		r.y += r.height;
		Vector3 force = droneController.Force;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Force: " + force.ToString () );
		r.y += r.height;
		force = droneController.Torque;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Torque: " + force.ToString () );
		if ( useTeleop )
		{
			r.y += r.height;
			GUI.Label ( r, "Position: " + teleop.Position.ToString () );
			r.y += r.height;
			GUI.Label ( r, "PRY: " + teleop.Rotation.eulerAngles.ToString () );
		}
		r.y += r.height;
		force = droneController.AngularVelocity;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Angular Vel.: " + force.ToString () );
		r.y += r.height;
		force = droneController.LinearAcceleration;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Linear Accel.: " + force.ToString () );
	}*/
}