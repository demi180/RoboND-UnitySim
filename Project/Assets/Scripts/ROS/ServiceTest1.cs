using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages.nav_msgs;
using Plan = Messages.nav_msgs.GetPlan;
using PoseStamped = Messages.geometry_msgs.PoseStamped;
using Pose = Messages.geometry_msgs.Pose;
using SetBool = Messages.std_srvs.SetBool;
using System.Threading;

public class ServiceTest1 : MonoBehaviour
{
	ServiceClient<Plan.Request, Plan.Response> pathClient;
	ServiceClient<SetBool.Request, SetBool.Response> cfx;
	ServiceClient<SetBool.Request, SetBool.Response> cfy;
	ServiceClient<SetBool.Request, SetBool.Response> cfz;
	ServiceClient<SetBool.Request, SetBool.Response> ctx;
	ServiceClient<SetBool.Request, SetBool.Response> cty;
	ServiceClient<SetBool.Request, SetBool.Response> ctz;
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

		if ( isCalling && Time.time > lastCallTime + 10 )
		{
			Debug.Log ( "Aborting.." );
			if (  srvThread != null  )
			{
				Debug.Log ( "thread state is " + srvThread.ThreadState );
				srvThread.Abort ();
			}
			isCalling = false;
		}

		if ( !isCalling )
		{
			if ( Input.GetKeyDown ( KeyCode.F1 ) )
				CallConstrain ( 0 );
			if ( Input.GetKeyDown ( KeyCode.F2 ) )
				CallConstrain ( 1 );
			if ( Input.GetKeyDown ( KeyCode.F3 ) )
				CallConstrain ( 2 );
			if ( Input.GetKeyDown ( KeyCode.F4 ) )
				CallConstrain ( 3 );
			if ( Input.GetKeyDown ( KeyCode.F5 ) )
				CallConstrain ( 4 );
			if ( Input.GetKeyDown ( KeyCode.F6 ) )
				CallConstrain ( 5 );
		}
	}
	
	void OnRosInit ()
	{
		nh = new NodeHandle ( "~" );
		pathClient = nh.serviceClient<Plan.Request, Plan.Response> ( "/quad_rotor/path" );
		cfx = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/x_force_constrained" );
		cfy = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/y_force_constrained" );
		cfz = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/z_force_constrained" );
		ctx = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/x_torque_constrained" );
		cty = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/y_torque_constrained" );
		ctz = nh.serviceClient<SetBool.Request, SetBool.Response> ( "/quad_rotor/z_torque_constrained" );
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

	void CallConstrain (int constraint)
	{
		Debug.Log ( "Calling constraint: " + constraint );
		isCalling = true;
		lastCallTime = Time.time;

		bool call = false;
		switch ( constraint )
		{
		case 0:
			srvThread = new Thread ( Call0 );
			break;

		case 1:
			srvThread = new Thread ( Call1 );
			break;

		case 2:
			srvThread = new Thread ( Call2 );
			break;

		case 3:
			srvThread = new Thread ( Call3 );
			break;

		case 4:
			srvThread = new Thread ( Call4 );
			break;

		case 5:
			srvThread = new Thread ( Call5 );
			break;
		}

		srvThread.Start ();

	}

	void Call0 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainForceX;
		bool call = cfx.call ( req, ref resp );

		if ( call )
			Debug.Log ( "call succeeded!" );
		else
			Debug.Log ( "call failed?" );

		isCalling = false;
	}

	void Call1 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainForceY;
		bool call = cfy.call ( req, ref resp );

		if ( call )
			Debug.Log ( "call succeeded!" );
		else
			Debug.Log ( "call failed?" );

		isCalling = false;
	}

	void Call2 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainForceZ;
		bool call = cfz.call ( req, ref resp );

		if ( call )
			Debug.Log ( "call succeeded!" );
		else
			Debug.Log ( "call failed?" );

		isCalling = false;
	}

	void Call3 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainTorqueX;
		bool call = ctx.call ( req, ref resp );

		if ( call )
			Debug.Log ( "call succeeded!" );
		else
			Debug.Log ( "call failed?" );

		isCalling = false;
	}

	void Call4 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainTorqueY;
		bool call = cty.call ( req, ref resp );

		if ( call )
			Debug.Log ( "call succeeded!" );
		else
			Debug.Log ( "call failed?" );

		isCalling = false;
	}

	void Call5 ()
	{
		QuadController q = QuadController.ActiveController;
		SetBool.Request req = new SetBool.Request ();
		SetBool.Response resp = new SetBool.Response ();

		req.data = !q.ConstrainTorqueZ;
		bool call = ctz.call ( req, ref resp );


//		if ( call )
//			Debug.Log ( "call succeeded!" );
//		else
//			Debug.Log ( "call failed?" );

		isCalling = false;
	}
}