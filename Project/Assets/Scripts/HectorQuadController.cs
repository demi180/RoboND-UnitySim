using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HectorQuadController : MonoBehaviour
{
	public bool MotorsEnabled { get; set; }
	public Vector3 Force { get { return force; } }
	public Vector3 Torque { get { return torque; } }
	public Vector3 Position { get; protected set; }
	public Quaternion Rotation { get; protected set; }
	public Vector3 AngularVelocity { get; protected set; }
	public Vector3 LinearAcceleration { get; protected set; }

	public Transform frontLeftRotor;
	public Transform frontRightRotor;
	public Transform rearLeftRotor;
	public Transform rearRightRotor;

	public float thrustForce = 2000;
	public float torqueForce = 500;
	public ForceMode forceMode = ForceMode.Force;
	public ForceMode torqueMode = ForceMode.Force;

	public Transform cameraOrientationTarget;

	Rigidbody rb;
	Transform[] rotors;
	Vector3 force;
	Vector3 torque;
	Vector3 lastVelocity;

	void Awake ()
	{
		rb = GetComponent<Rigidbody> ();
		rotors = new Transform[4] {
			frontLeftRotor,
			frontRightRotor,
			rearLeftRotor,
			rearRightRotor
		};
		MotorsEnabled = true;
	}

	void LateUpdate ()
	{
		if ( Input.GetKeyDown ( KeyCode.Escape ) )
			Application.Quit ();

		if ( Input.GetKeyDown ( KeyCode.R ) )
		{
			ResetOrientation ();
		}

		// update acceleration
//		LinearAcceleration = ( rb.velocity - lastVelocity ) / Time.deltaTime;
//		lastVelocity = rb.velocity;


		float zAngle = 0;
		Vector3 up = transform.up;
		if ( up.y >= 0 )
			zAngle = transform.localEulerAngles.z;
		else
			zAngle = -transform.localEulerAngles.z;
		while ( zAngle > 180 )
			zAngle -= 360;
		while ( zAngle < -360 )
			zAngle += 360;
		transform.Rotate ( Vector3.up * -zAngle * Time.deltaTime, Space.World );
		Position = transform.position;
		Rotation = transform.rotation;
	}

	void FixedUpdate ()
	{
		if ( MotorsEnabled )
		{
			// add force
			rb.AddRelativeForce ( force * Time.deltaTime, forceMode );

			// add torque
			rb.AddRelativeTorque ( torque * Time.deltaTime, torqueMode );
//			rb.AddTorque ( torque * Time.deltaTime, torqueMode );

			// update acceleration
			LinearAcceleration = ( rb.velocity - lastVelocity ) / Time.deltaTime;
			lastVelocity = rb.velocity;
			AngularVelocity = rb.angularVelocity;
		}
	}

	void OnGUI ()
	{
		Rect r = new Rect ( 10, 10, 180, 200 );
		GUI.Box ( r, "" );
		GUI.Box ( r, "" );
		r.x = 15;
		r.height = 20;
		GUI.Label ( r, "Motors enabled: <color=yellow>" + MotorsEnabled + "</color>" );
		r.y += r.height;
		Vector3 force = Force;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Force: " + force.ToString () );
		r.y += r.height;
		force = Torque;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Torque: " + force.ToString () );
//		if ( useTeleop )
//		{
			r.y += r.height;
			GUI.Label ( r, "Position: " + Position.ToString () );
			r.y += r.height;
			GUI.Label ( r, "PRY: " + Rotation.eulerAngles.ToString () );
//		}
		r.y += r.height;
		force = AngularVelocity;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Angular Vel.: " + force.ToString () );
		r.y += r.height;
		force = LinearAcceleration;
		force = new Vector3 ( force.x, force.z, force.y );
		GUI.Label ( r, "Linear Accel.: " + force.ToString () );
	}

	public void ApplyMotorForce (float x, float y, float z, bool swapAxes = false)
	{
		force.x = x;
		force.y = swapAxes ? z : y;
		force.z = swapAxes ? y : z;
		force *= thrustForce;
	}

	public void ApplyMotorTorque (float x, float y, float z, bool swapAxes = false)
	{
		torque.x = x;
		torque.y = swapAxes ? z : y;
		torque.z = swapAxes ? y : z;
		torque *= torqueForce;
	}

	public void ResetOrientation ()
	{
		transform.rotation = Quaternion.identity;
		force = Vector3.zero;
		torque = Vector3.zero;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		LinearAcceleration = Vector3.zero;
	}
}