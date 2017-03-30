using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOutput : MonoBehaviour
{
	public IRobotController controller;
	public Text infoText;
	public GameObject progressParent;
	public Image progressBar;

	System.Text.StringBuilder sb = new System.Text.StringBuilder ();

	void Start ()
	{
		if ( infoText == null )
		{
			enabled = false;
			return;
		}
//		infoText.gameObject.SetActive ( false );
//		infoText.gameObject.SetActive ( true );
		infoText.text = "";
		infoText.transform.parent.gameObject.SetActive ( false );
		infoText.transform.parent.gameObject.SetActive ( true );
	}

	void Update ()
	{
		if ( infoText == null )
			return;
		
		sb.Length = 0;
		sb.Capacity = 16;
		float speed = controller.Speed;
		float steer = controller.SteerAngle;
		float vAngle = controller.VerticalAngle;
		float throttle = controller.ThrottleInput;
		Vector3 position = controller.Position;
		float orientation = controller.Orientation;

		sb.Append ( "Throttle: " + throttle.ToString ( "F1" ) + "\n" );
		sb.Append ( "Steer angle: " + steer.ToString ( "F4" ) + "\n" );
		sb.Append ( "Vertical angle: " + vAngle.ToString ( "F4" ) + "\n" );
		sb.Append ( "Ground speed: " + speed.ToString ( "F1" ) + "m/s\n" );
		sb.Append ( "Position: " + position.ToString () + "\n" );
		sb.Append ( "Yaw angle: " + orientation.ToString ( "F2" ) + "\n" );
		sb.Append ( "Camera zoom: " + controller.Zoom.ToString ( "F1" ) + "x\n" );
		sb.Append ( "Is near objective: " + ( controller.IsNearObjective ? "Yes" : "No" ) );
		infoText.text = sb.ToString ();

		if ( controller.PickupProgress != -1 )
		{
			if ( !progressParent.activeSelf )
				progressParent.SetActive ( true );
			progressBar.fillAmount = controller.PickupProgress;
		} else
		{
			if ( progressParent.activeSelf )
				progressParent.SetActive ( false );
		}
	}
}