using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages;
using Messages.geometry_msgs;
using Messages.tf2_msgs;
using Messages.sensor_msgs;
using hector_uav_msgs;
using LandingClient = actionlib.SimpleActionClient<hector_uav_msgs.LandingAction>;
using TakeoffClient = actionlib.SimpleActionClient<hector_uav_msgs.TakeoffAction>;
using PoseClient = actionlib.SimpleActionClient<hector_uav_msgs.PoseAction>;

/*
 * QRKeyboardTeleop: a basic teleop class for a quadrotor drone using Hector_Quadrotor commands
 */

public class QRKeyboardTeleop : MonoBehaviour
{
	public bool active;

	NodeHandle nh;
	Subscriber<Joy> joySubscriber;
	Publisher<TwistStamped> velocityPublisher;
	Publisher<AttitudeCommand> attitudePublisher;
	Publisher<YawRateCommand> yawRatePublisher;
	Publisher<ThrustCommand> thrustPublisher;
	ServiceClient<EnableMotors> motorEnableService;
	LandingClient landingClient;
	TakeoffClient takeoffClient;
	PoseClient poseClient;

	PoseStamped pose;
	double yaw;
	double slowFactor;
	string baseLinkFrame, baseStabilizedFrame, worldFrame;

	// attitude control
//	double maxPitch;
//	double maxRoll;
//	double maxThrust;
//	double thrustOffset;
	// twist (/position) control
	double xVelocityMax;
	double yVelocityMax;
	double zVelocityMax;

	void Awake ()
	{
		if ( !active )
		{
			enabled = false;
			return;
		}
		ROSController.StartROS ( OnRosInit );
	}

	void OnDestroy ()
	{
		stop ();
	}

	void OnRosInit ()
	{
		NodeHandle privateNH = new NodeHandle("~");


		// TODO dynamic reconfig
		string control_mode = "";
		privateNH.param<string>("control_mode", ref control_mode, "twist");

		NodeHandle robot_nh = new NodeHandle ();

		// TODO factor out
		robot_nh.param<string>("base_link_frame", ref baseLinkFrame, "base_link");
		robot_nh.param<string>("world_frame", ref worldFrame, "world");
		robot_nh.param<string>("base_stabilized_frame", ref baseStabilizedFrame, "base_stabilized");

		if (control_mode == "attitude")
		{
//			privateNH.param<double>("pitch_max", ref sAxes.x.factor, 30.0);
//			privateNH.param<double>("roll_max", ref sAxes.y.factor, 30.0);
//			privateNH.param<double>("thrust_max", ref sAxes.thrust.factor, 10.0);
//			privateNH.param<double>("thrust_offset", ref sAxes.thrust.offset, 10.0);
			attitudePublisher = robot_nh.advertise<AttitudeCommand> ( "command/attitude", 10 );
			yawRatePublisher = robot_nh.advertise<YawRateCommand> ( "command/yawrate", 10 );
			thrustPublisher = robot_nh.advertise<ThrustCommand> ( "command/thrust", 10 );
		}
		else if (control_mode == "velocity" || control_mode == "twist")
		{
			// Gazebo uses Y=forward and Z=up
			privateNH.param<double>("x_velocity_max", ref xVelocityMax, 2.0);
			privateNH.param<double>("y_velocity_max", ref zVelocityMax, 2.0);
			privateNH.param<double>("z_velocity_max", ref yVelocityMax, 2.0);

			velocityPublisher = robot_nh.advertise<TwistStamped> ( "command/twist", 10 );
		}
		else if (control_mode == "position")
		{
			// Gazebo uses Y=forward and Z=up
			privateNH.param<double>("x_velocity_max", ref xVelocityMax, 2.0);
			privateNH.param<double>("y_velocity_max", ref zVelocityMax, 2.0);
			privateNH.param<double>("z_velocity_max", ref yVelocityMax, 2.0);

			pose.pose.position.x = 0;
			pose.pose.position.y = 0;
			pose.pose.position.z = 0;
			pose.pose.orientation.x = 0;
			pose.pose.orientation.y = 0;
			pose.pose.orientation.z = 0;
			pose.pose.orientation.w = 1;
		}
		else
		{
			ROS.Error("Unsupported control mode: " + control_mode);
		}

		motorEnableService = robot_nh.serviceClient<EnableMotors> ( "enable_motors" );
		takeoffClient = new TakeoffClient ( robot_nh, "action/takeoff" );
		landingClient = new LandingClient ( robot_nh, "action/landing" );
		poseClient = new PoseClient ( robot_nh, "action/pose" );
	}

	public bool enableMotors (bool enable)
	{
		EnableMotors srv = new EnableMotors ();
		srv.req.enable = enable;
		return motorEnableService.call(srv);
	}

	public void stop ()
	{
		if ( velocityPublisher != null && velocityPublisher.getNumSubscribers () > 0 )
		{
			velocityPublisher.publish ( new TwistStamped () );
		}
		if ( attitudePublisher != null && attitudePublisher.getNumSubscribers () > 0 )
		{
			attitudePublisher.publish ( new AttitudeCommand () );
		}
		if ( thrustPublisher != null && thrustPublisher.getNumSubscribers () > 0 )
		{
			thrustPublisher.publish ( new ThrustCommand () );
		}
		if ( yawRatePublisher != null && yawRatePublisher.getNumSubscribers () > 0 )
		{
			yawRatePublisher.publish ( new YawRateCommand () );
		}
	}

	public void SendTwist (UnityEngine.Vector3 linear, UnityEngine.Vector3 angular)
	{
		TwistStamped twist = new TwistStamped ();
		twist.twist = new Twist ();
		twist.header = new Messages.std_msgs.Header ();
		var lin = new Messages.geometry_msgs.Vector3 ();
		var ang = new Messages.geometry_msgs.Vector3 ();

		lin.x = linear.x;
		lin.y = linear.z;
		lin.z = linear.y;
		ang.x = angular.x;
		ang.y = angular.z;
		ang.z = angular.y;
		twist.twist.linear = lin;
		twist.twist.angular = ang;
		twist.header.frame_id = baseLinkFrame;
		twist.header.stamp = ROS.GetTime ();
		velocityPublisher.publish ( twist );
	}
}