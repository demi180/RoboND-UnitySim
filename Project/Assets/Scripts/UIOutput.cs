using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOutput : MonoBehaviour
{
	public IRobotController controller;
	public Text infoText;

	System.Text.StringBuilder sb = new System.Text.StringBuilder ();

	void Update ()
	{
		if ( infoText == null )
			return;
		
		sb.Length = 0;
		sb.Capacity = 16;
		float speed = controller.GroundVelocity.magnitude;
		float steer = controller.SteerAngle;

		sb.Append ( "Ground speed: " + speed.ToString ( "F1" ) + "m/s\n" );
		sb.Append ( "Steer angle: " + steer.ToString ( "F1" ) + "\n" );
		sb.Append ( "Camera zoom: " + controller.Zoom.ToString ( "F1" ) + "x" );
		infoText.text = sb.ToString ();

	}
}