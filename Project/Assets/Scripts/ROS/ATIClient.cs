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

	int a, b;

	IEnumerator Start ()
	{
		a = Random.Range ( 0, 100 );
		b = Random.Range ( 0, 100 );
		yield return new WaitForSeconds ( 1f );
		ROSController.StartROS ( OnRosInit );
	}

	void OnRosInit ()
	{
		nh = new NodeHandle ( "" );
		cli = nh.serviceClient<AddTwoInts.Request, AddTwoInts.Response> ( "/add_two_ints" );

		Debug.Log ( "calling client" );
		thread = new Thread ( Add );
		thread.Start ();
	}

	void Add ()
	{
		AddTwoInts.Request req = new AddTwoInts.Request() { a = this.a, b = this.b };
		AddTwoInts.Response resp = new AddTwoInts.Response();
		bool res = cli.call ( req, ref resp );
//		bool res = nh.serviceClient<AddTwoInts.Request, AddTwoInts.Response> ( "/add_two_ints" ).call ( req, ref resp );
		if ( res )
		{
			Debug.Log ( "Addition called (client)! " + req.a + " + " + req.b + " = " + resp.sum );
		} else
		{
			Debug.Log ( "Call failed :(" );
		}
	}
}