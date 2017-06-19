using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages.nav_msgs;
using Plan = Messages.nav_msgs.GetPlan;
using PoseStamped = Messages.geometry_msgs.PoseStamped;
using Pose = Messages.geometry_msgs.Pose;
using System.Threading;

public class ServiceTest1 : MonoBehaviour
{
	ServiceClient<Plan.Request, Plan.Response> pathClient;
//	ServiceClient<Plan> pathClient;
	NodeHandle nh;
//	Plan path;
	Thread srvThread;


	public float lastCallTime;
	public bool isCalling;

	void Start ()
	{
		ROSController.StartROS ( OnRosInit );
	}

	void LateUpdate ()
	{
		if ( Input.GetKeyDown ( KeyCode.O ) && !isCalling )
		{
			isCalling = true;
			lastCallTime = Time.time;
			CallService ();
//			srvThread = new Thread ( CallService );
//			srvThread.Start ();
		}

		if ( isCalling && Time.time > lastCallTime + 30 )
		{
			Debug.Log ( "Aborting.." );
//			srvThread.Abort ();
			isCalling = false;
		}
	}
	
	void OnRosInit ()
	{
		nh = new NodeHandle ( "~" );
		pathClient = nh.serviceClient<Plan.Request, Plan.Response> ( "/quad_rotor/path" );
//		pathClient = nh.serviceClient<Plan> ( "/quad_rotor/path" );
//		path = new Plan ();
	}

	void CallService ()
	{
		Debug.Log ( "calling!" );
		Plan.Request req = new Plan.Request ();
		Plan.Response resp = new Plan.Response ();
		req.Randomize ();

		if ( pathClient.call ( req, ref resp ) )
//		if ( pathClient.call ( path ) )
		{
			Debug.Log ( "Received a path!" );
			var poses = resp.plan.poses;
//			var poses = path.resp.plan.poses;
			int i = 0;
			foreach ( PoseStamped ps in poses )
			{
				Debug.Log ( "Pose " + ( i++ ) + "position: " + ps.pose.position.ToUnity () + " orientation: " + ps.pose.orientation.ToUnity () );
			}
		} else
			Debug.Log ( "Path call failed" );

		isCalling = false;
	}
}