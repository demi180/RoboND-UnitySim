using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages.ServiceTest;
using System.Threading;

public class ATIClient : MonoBehaviour
{
	NodeHandle nh;
	ServiceClient<AddTwoInts.Request, AddTwoInts.Response> cli;
	Thread thread;

	int a, b, sum;
	public bool callFinished = false;
	public bool callResponse;

	IEnumerator Start ()
	{
		a = Random.Range ( 0, 100 );
		b = Random.Range ( 0, 100 );
		yield return new WaitForSeconds ( 1f );
		ROSController.StartROS ( OnRosInit );
	}

	void OnRosInit ()
	{
		nh = ROS.GlobalNodeHandle;
//		nh = new NodeHandle ( "" );
		cli = nh.serviceClient<AddTwoInts.Request, AddTwoInts.Response> ( "/add_two_ints" );

		Debug.Log ( "calling client" );
		callFinished = false;
		thread = new Thread ( Add );
		thread.Start ();
	}

	void Update ()
	{
		if ( thread != null )
			Debug.Log ( thread.ThreadState );
		if ( callFinished )
		{
//			OnCallFinished ();
		}
	}

	void Add ()
	{
		AddTwoInts.Request req = new AddTwoInts.Request () { a = this.a, b = this.b };
		AddTwoInts.Response resp = new AddTwoInts.Response ();
		a = req.a;
		b = req.b;
		sum = 0;
		callResponse = cli.call ( req, ref resp );
		sum = resp.sum;
		if ( callResponse )
			Debug.Log ( "response added " + sum );
		else
			Debug.Log ( "response failed" );
		callFinished = true;
//		Thread.CurrentThread.Join ( 200 );
	}

	void OnCallFinished ()
	{
		Debug.Log ( "thread state is " + thread.ThreadState );
//		thread.Join ();
		callFinished = false;
		if ( callResponse )
		{
			Debug.Log ( "Addition called (client)! " + a + " + " + b + " = " + sum );
		} else
		{
			Debug.Log ( "Call failed :(" );
		}
	}
}