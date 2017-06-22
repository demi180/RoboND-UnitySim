using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages.ServiceTest;

public class ATIServer : MonoBehaviour
{
	NodeHandle nh;
	ServiceServer srv;

	void Start ()
	{
		ROSController.StartROS ( OnRosInit );
	}

	void OnRosInit ()
	{
		nh = ROS.GlobalNodeHandle;
//		nh = new NodeHandle ( "" );
		srv = nh.advertiseService<AddTwoInts.Request, AddTwoInts.Response> ( "/add_two_ints", Addition );
	}

	bool Addition (AddTwoInts.Request req, ref AddTwoInts.Response resp)
	{
		resp.sum = req.a + req.b;
		Debug.Log ( "Addition called (server)! " + req.a + " + " + req.b + " = " + ( req.a + req.b ) );
		return true;
	}
}