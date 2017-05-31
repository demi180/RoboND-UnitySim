using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Ros_CSharp;
using hector_uav_msgs;
using Messages;
using TwistStamped = Messages.geometry_msgs.TwistStamped;
using GVector3 = Messages.geometry_msgs.Vector3;
using PoseStamped = Messages.geometry_msgs.PoseStamped;
using Wrench = Messages.geometry_msgs.Wrench;

/*
 * HectorDrone: receives messages from a QRKeyboardTeleop, and applies force/torque to a HectorQuadController
 */

public class HectorDrone : MonoBehaviour
{
	public HectorQuadController droneController;
	public bool active;

	NodeHandle nh;
	ServiceServer enableMotorSrv;
	Publisher<PoseStamped> posePub;
	Subscriber<TwistStamped> twistSub;
	Subscriber<Wrench> wrenchSub;
	Thread pubThread;

	uint frameSeq = 0;

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
//		enableMotorSrv = nh.advertiseService<EnableMotors.Request, EnableMotors.Response> ( "enable_motors", OnEnableMotors );
		nh.setParam ( "control_mode", "wrench" ); // for now force twist mode
//		twistSub = nh.subscribe<TwistStamped> ( "command/twist", 10, TwistCallback );
		wrenchSub = nh.subscribe<Wrench> ( "quad_rotor/cmd_force", 10, WrenchCallback );
		posePub = nh.advertise<PoseStamped> ( "quad_rotor/pose", 10, false );
		pubThread = new Thread ( PublishPose );
		pubThread.Start ();
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

	void WrenchCallback (Wrench msg)
	{
		Vector3 force = msg.force.ToUnityVector ();
		Vector3 torque = msg.torque.ToUnityVector ();
		if ( droneController != null )
		{
			if ( !droneController.MotorsEnabled )
				droneController.MotorsEnabled = true;
			droneController.ApplyMotorForce ( force.x, force.y, force.z );
			droneController.ApplyMotorTorque ( torque.x, torque.y, torque.z );
		}
	}

	void PublishPose ()
	{
		PoseStamped ps = new PoseStamped ();
		ps.header = new Messages.std_msgs.Header ();
		ps.pose = new Messages.geometry_msgs.Pose ();


		int sleep = 1000 / 60;
		Vector3 testPos = Vector3.zero;
		while ( ROS.ok && !ROS.shutting_down )
		{
			ps.header.frame_id = "";
			ps.header.seq = frameSeq++;
			ps.header.stamp = ROS.GetTime ();
			ps.pose.position = new Messages.geometry_msgs.Point ( droneController.Position, true );
			ps.pose.orientation = new Messages.geometry_msgs.Quaternion ( droneController.Rotation );
			posePub.publish ( ps );
			
			Thread.Sleep ( sleep );
		}
	}
}