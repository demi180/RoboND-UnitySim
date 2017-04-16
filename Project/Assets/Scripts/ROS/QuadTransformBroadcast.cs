using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using tf.net;

public class QuadTransformBroadcast : MonoBehaviour
{
	Transformer tft;

	void Awake ()
	{
		emTransform emt = new emTransform ( transform );
		emt.origin = new emVector3 ( Vector3.zero );
		emt.UnityRotation = Quaternion.identity;
		Messages.std_msgs.Time t = ROS.GetTime (System.DateTime.Now);

	}
}