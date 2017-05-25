using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using hector_uav_msgs;
using Messages;
using TwistStamped = Messages.geometry_msgs.TwistStamped;
using GVector3 = Messages.geometry_msgs.Vector3;

/*
 * HectorDrone: receives messages from a QRKeyboardTeleop, and applies force/torque to a HectorQuadController
 */

public class HectorDrone : MonoBehaviour
{
	public HectorQuadController droneController;
	public bool active;

	NodeHandle nh;
	ServiceServer enableMotorSrv;
	Subscriber<TwistStamped> twistSub;

	void Awake ()
	{
		if ( !active )
		{
			enabled = false;
			return;
		}
		ROSController.StartROS ( OnRosInit );
	}

	void OnRosInit ()
	{
		nh = new NodeHandle ( "~" );
		enableMotorSrv = nh.advertiseService<EnableMotors.Request, EnableMotors.Response> ( "enable_motors", OnEnableMotors );
		nh.setParam ( "control_mode", "twist" ); // for now force twist mode
		twistSub = nh.subscribe<TwistStamped> ( "command/twist", 10, TwistCallback );
	}

	bool OnEnableMotors (EnableMotors.Request req, ref EnableMotors.Response resp)
	{
		if ( droneController != null )
		{
			droneController.MotorsEnabled = req.enable;
			resp.success = true;
			return true;
		}

		resp.success = false;
		return false;
	}

	void TwistCallback (TwistStamped msg)
	{
		GVector3 linear = msg.twist.linear;
		GVector3 angular = msg.twist.angular;
		if ( droneController != null )
		{
			droneController.ApplyMotorForce ( (float) linear.x, (float) linear.y, (float) linear.z );
			droneController.ApplyMotorTorque ( (float) angular.x, (float) angular.y, (float) angular.z );
		}
	}
}