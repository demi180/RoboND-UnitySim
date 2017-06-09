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
using Imu = Messages.sensor_msgs.Imu;


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
	Publisher<Imu> imuPub;
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
		imuPub = nh.advertise<Imu> ( "quad_rotor/imu", 10, false );
		pubThread = new Thread ( PublishAll );
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
			droneController.ApplyMotorForce ( (float) linear.x, (float) linear.y, (float) linear.z, true, true );
			droneController.ApplyMotorTorque ( (float) angular.x, (float) angular.y, (float) angular.z, true, true );
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
			droneController.ApplyMotorForce ( force.x, force.y, force.z, true, true );
			droneController.ApplyMotorTorque ( torque.x, torque.y, torque.z, true, true );
		}
	}

	void PublishAll ()
	{
		PoseStamped ps = new PoseStamped ();
		ps.header = new Messages.std_msgs.Header ();
		ps.pose = new Messages.geometry_msgs.Pose ();
		Imu imu = new Imu ();
		imu.header = new Messages.std_msgs.Header ( ps.header );
		imu.angular_velocity_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		imu.linear_acceleration_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		imu.orientation_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };


		int sleep = 1000 / 60;
		Vector3 testPos = Vector3.zero;
		while ( ROS.ok && !ROS.shutting_down )
		{
			// publish pose
			ps.header.frame_id = "";
			ps.header.seq = frameSeq++;
			ps.header.stamp = ROS.GetTime ();
			ps.pose.position = new Messages.geometry_msgs.Point ( droneController.Position, true, true );
			ps.pose.orientation = new Messages.geometry_msgs.Quaternion ( droneController.Rotation );
			posePub.publish ( ps );

			// publish imu
			imu.header.frame_id = "";
			imu.header.seq = frameSeq;
			imu.header.stamp = ps.header.stamp;
			imu.angular_velocity = new GVector3 ( droneController.AngularVelocity, true, true );
			imu.linear_acceleration = new GVector3 ( droneController.LinearAcceleration, true, true );
			imu.orientation = new Messages.geometry_msgs.Quaternion ( droneController.Rotation );
			imuPub.publish ( imu );
			
			Thread.Sleep ( sleep );
		}
	}
}