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
using Image = Messages.sensor_msgs.Image;
using Path = Messages.nav_msgs.Path;
using GetPlan = Messages.nav_msgs.GetPlan;
using SetBool = Messages.std_srvs.SetBool;
using Empty = Messages.std_srvs.Empty;
using SetPose = Messages.geometry_msgs.SetPose;


/*
 * QuadDrone: receives messages from a QRKeyboardTeleop, and applies force/torque to a QuadController
 */

public class QuadDrone : MonoBehaviour
{
	public QuadController droneController;
	public bool active;

	// node to link everything up to ros
	NodeHandle nh;
	// service to set drone orientation. still working on getting it to work
	ServiceServer setOrientSrv;
	// service to turn drone motor on/off. still working on it..
	ServiceServer enableMotorSrv;
	// publishers for drone and camera info
	Publisher<PoseStamped> posePub;
	Publisher<Imu> imuPub;
	Publisher<Image> imgPub;
	// service for the drone's current path
	ServiceServer pathSrv;
	// subscribers for drone movement
	Subscriber<TwistStamped> twistSub;
	Subscriber<Wrench> wrenchSub;
	// thread to run the publishers on
	Thread pubThread;

	// services to enable/disable gravity and constrain movement
	ServiceServer gravitySrv;
	ServiceServer constrainForceXSrv;
	ServiceServer constrainForceYSrv;
	ServiceServer constrainForceZSrv;
	ServiceServer constrainTorqueXSrv;
	ServiceServer constrainTorqueYSrv;
	ServiceServer constrainTorqueZSrv;
	ServiceServer TriggerResetSrv;
	ServiceServer setPoseSrv;

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
		pathSrv = nh.advertiseService<GetPlan.Request, GetPlan.Response> ( "quad_rotor/path", PathService );
//		setOrientSrv = nh.advertiseService<Messages.std_srvs.Empty.Request>
//		enableMotorSrv = nh.advertiseService<EnableMotors.Request, EnableMotors.Response> ( "enable_motors", OnEnableMotors );
		nh.setParam ( "control_mode", "wrench" ); // for now force twist mode
//		twistSub = nh.subscribe<TwistStamped> ( "command/twist", 10, TwistCallback );
		wrenchSub = nh.subscribe<Wrench> ( "quad_rotor/cmd_force", 10, WrenchCallback );
		posePub = nh.advertise<PoseStamped> ( "quad_rotor/pose", 10, false );
		imuPub = nh.advertise<Imu> ( "quad_rotor/imu", 10, false );
		imgPub = nh.advertise<Image> ( "quad_rotor/image", 10, false );
		pubThread = new Thread ( PublishAll );
		pubThread.Start ();

		gravitySrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/gravity", GravityService );
		constrainForceXSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/x_force_constrained", ConstrainForceX );
		constrainForceYSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/y_force_constrained", ConstrainForceY );
		constrainForceZSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/z_force_constrained", ConstrainForceZ );
		constrainTorqueXSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/x_torque_constrained", ConstrainTorqueX );
		constrainTorqueYSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/y_torque_constrained", ConstrainTorqueY );
		constrainTorqueZSrv = nh.advertiseService<SetBool.Request, SetBool.Response> ( "quad_rotor/z_torque_constrained", ConstrainTorqueZ );
		TriggerResetSrv = nh.advertiseService<Empty.Request, Empty.Response> ( "quad_rotor/reset_orientation", TriggerReset );
		setPoseSrv = nh.advertiseService<SetPose.Request, SetPose.Response> ( "quad_rotor/set_pose", SetPoseService );
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
		Vector3 linear = msg.twist.linear.ToUnityVector ();
		Vector3 angular = msg.twist.angular.ToUnityVector ();
		if ( droneController != null )
		{
			droneController.ApplyMotorForce ( linear, true );
			droneController.ApplyMotorTorque ( angular, true );
//			droneController.ApplyMotorForce ( (float) linear.x, (float) linear.y, (float) linear.z, true, true );
//			droneController.ApplyMotorTorque ( (float) angular.x, (float) angular.y, (float) angular.z, true, true );
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
			droneController.ApplyMotorForce ( force, true );
			droneController.ApplyMotorTorque ( torque, true );
//			droneController.ApplyMotorForce ( force.x, force.y, force.z, true, true );
//			droneController.ApplyMotorTorque ( torque.x, torque.y, torque.z, true, true );
		}
	}

	void PublishAll ()
	{
		// pose info
		PoseStamped ps = new PoseStamped ();
		ps.header = new Messages.std_msgs.Header ();
		ps.header.stamp = ROS.GetTime ();
		ps.header.frame_id = "";
		ps.pose = new Messages.geometry_msgs.Pose ();
		Imu imu = new Imu ();
		// imu info
		imu.header = new Messages.std_msgs.Header ( ps.header );
		imu.angular_velocity_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		imu.linear_acceleration_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		imu.orientation_covariance = new double[9] { -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		// image info
		Image img = new Image ();
		img.header = new Messages.std_msgs.Header ( ps.header );
		img.width = (uint) QuadController.ImageWidth;
		img.height = (uint) QuadController.ImageHeight;
		img.encoding = "mono16"; // "rgba8";
		img.step = img.width * 2; // * 4
		img.is_bigendian = 1;


		int sleep = 1000 / 60;
		Vector3 testPos = Vector3.zero;
		while ( ROS.ok && !ROS.shutting_down )
		{
			// publish pose
			ps.header.frame_id = "";
			ps.header.seq = frameSeq;
			ps.header.stamp = ROS.GetTime ();
			ps.pose.position = new Messages.geometry_msgs.Point ( droneController.Position.ToRos () );
//			ps.pose.position = new Messages.geometry_msgs.Point ( droneController.Position, true, true );
			ps.pose.orientation = new Messages.geometry_msgs.Quaternion ( droneController.Rotation.ToRos () );
			posePub.publish ( ps );

			// publish imu
			imu.header.frame_id = "";
			imu.header.seq = frameSeq++;
			imu.header.stamp = ps.header.stamp;
			imu.angular_velocity = new GVector3 ( droneController.AngularVelocity.ToRos () );
//			imu.angular_velocity = new GVector3 ( droneController.AngularVelocity, true, true );
			imu.linear_acceleration = new GVector3 ( droneController.LinearAcceleration.ToRos () );
//			imu.linear_acceleration = new GVector3 ( droneController.LinearAcceleration, true, true );
			imu.orientation = new Messages.geometry_msgs.Quaternion ( droneController.Rotation.ToRos () );
			imuPub.publish ( imu );

			// publish image
			img.data = droneController.GetImageData ();
			imgPub.publish ( img );
			
			Thread.Sleep ( sleep );
		}
	}

	bool PathService (GetPlan.Request req, ref GetPlan.Response resp)
	{
		Debug.Log ( "path service called!" );
		Path path = new Path ();
		path.header = new Messages.std_msgs.Header ();
		path.header.frame_id = "global";
		path.header.stamp = ROS.GetTime ();
		path.header.seq = 0;
		PathSample[] samples = PathPlanner.GetPath ();
		int count = samples.Length;
		path.poses = new PoseStamped[ count ];
		Debug.Log ( "sending " + count + " samples" );
		for ( int i = 0; i < count; i++ )
		{
			PoseStamped pst = new PoseStamped ();
			pst.header = new Messages.std_msgs.Header ();
			pst.header.frame_id = "local";
			pst.header.stamp = ROS.GetTime ();
			pst.header.seq = (uint) i;
			pst.pose = new Messages.geometry_msgs.Pose ();
			pst.pose.position = new Messages.geometry_msgs.Point ( samples [ i ].position.ToRos () );
			pst.pose.orientation = new Messages.geometry_msgs.Quaternion ( samples [ i ].orientation.ToRos () );
			path.poses [ i ] = pst;
		}
		resp.plan = path;
		return true;
	}

	bool GravityService (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "gravity service!" );
		droneController.UseGravity = req.data;
		resp.message = droneController.UseGravity.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool ConstrainForceX (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_force_x service!" );
		droneController.ConstrainForceX = req.data;
		resp.message = droneController.ConstrainForceX.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool ConstrainForceY (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_force_y service!" );
		droneController.ConstrainForceY = req.data;
		resp.message = droneController.ConstrainForceY.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool ConstrainForceZ (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_force_z service!" );
		droneController.ConstrainForceZ = req.data;
		resp.message = droneController.ConstrainForceZ.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool ConstrainTorqueX (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_torque_x service!" );
		droneController.ConstrainTorqueX = req.data;
		resp.message = droneController.ConstrainTorqueX.ToString ();
		resp.success = true;
		return true;
	}

	bool ConstrainTorqueY (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_torque_y service!" );
		droneController.ConstrainTorqueY = req.data;
		resp.message = droneController.ConstrainTorqueY.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool ConstrainTorqueZ (SetBool.Request req, ref SetBool.Response resp)
	{
		Debug.Log ( "constrain_torque_z service!" );
		droneController.ConstrainTorqueZ = req.data;
		resp.message = droneController.ConstrainTorqueZ.ToString ();
		resp.success = true;

//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );

		return true;
	}

	bool TriggerReset (Empty.Request req, ref Empty.Response resp)
	{
		Debug.Log ( "reset orientation service!" );
//		droneController.TriggerReset ();
		droneController.ApplyMotorForce ( Vector3.zero );
		droneController.ApplyMotorTorque ( Vector3.zero );
		return true;
	}

	bool SetPoseService (SetPose.Request req, ref SetPose.Response resp)
	{
		Debug.Log ( "setpose service!" );
		// first ToUnity goes from PointQuaternion to Vector3/Quaternion, and 2nd goes from Ros' coord system to Unity's
		droneController.SetPositionAndOrientation ( req.pose.position.ToUnity ().ToUnity (), req.pose.orientation.ToUnity ().ToUnity () );
		resp.message = "";
		resp.success = true;
		return true;
	}
}