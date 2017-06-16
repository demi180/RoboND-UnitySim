using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class QuadController : MonoBehaviour
{
	public static QuadController ActiveController;
	public static int ImageWidth = 640;
	public static int ImageHeight = 480;

	public bool MotorsEnabled { get; set; }
	public Vector3 Force { get { return force; } }
	public Vector3 Torque { get { return torque; } }
	public Vector3 Position { get; protected set; }
	public Quaternion Rotation { get; protected set; }
	public Vector3 AngularVelocity { get; protected set; }
	public Vector3 LinearAcceleration { get; protected set; }
	public Vector3 Forward { get; protected set; }
	public Vector3 Right { get; protected set; }
	public Vector3 Up { get; protected set; }
	public Vector3 YAxis { get; protected set; }
	public Vector3 XAxis { get; protected set; }

	public Transform frontLeftRotor;
	public Transform frontRightRotor;
	public Transform rearLeftRotor;
	public Transform rearRightRotor;
	public Transform yAxis;
	public Transform xAxis;
	public Transform forward;
	public Transform right;

	public Camera droneCam1;

	public float thrustForce = 2000;
	public float torqueForce = 500;
	public ForceMode forceMode = ForceMode.Force;
	public ForceMode torqueMode = ForceMode.Force;

	public bool rotateWithTorque;

	[System.NonSerialized]
	public Rigidbody rb;
	Transform[] rotors;
	Vector3 force;
	Vector3 torque;
	Vector3 lastVelocity;
	bool inverseFlag;
	Ray ray;
	RaycastHit rayHit;
	BinarySerializer b = new BinarySerializer ( 1000 );

	byte[] cameraData;
//	RenderTexture cameraTex;

	void Awake ()
	{
		if ( ActiveController == null )
			ActiveController = this;
		rb = GetComponent<Rigidbody> ();
		rotors = new Transform[4] {
			frontLeftRotor,
			frontRightRotor,
			rearLeftRotor,
			rearRightRotor
		};
		MotorsEnabled = true;
		Forward = forward.forward;
		Right = right.forward;
		Up = transform.up;
		CreateCameraTex ();
	}

	void Update ()
	{
		Position = transform.position;
		Rotation = transform.rotation;
		Forward = forward.forward;
		Right = right.forward;
		Up = transform.up;
		XAxis = xAxis.forward;
		YAxis = yAxis.forward;
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


		// use this to have a follow camera rotate with the quad. not proper torque!
		if ( rotateWithTorque )
		{
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
		}

//		Position = transform.position;
//		Rotation = transform.rotation;
//		Forward = forward.forward;
//		Right = right.forward;
//		Up = transform.up;
//		XAxis = xAxis.forward;
//		YAxis = yAxis.forward;

		Profiler.BeginSample ( "Raycast" );
		ray = droneCam1.ViewportPointToRay ( new Vector3 ( 0.5f, 0.5f, droneCam1.nearClipPlane ) );
		bool didHit;
		int count = 1000;
		b.Clear ();
//		Profiler.EndSample ();
		short value;
		for ( int i = 0; i < count; i++ )
		{
			didHit = Physics.Raycast ( ray, out rayHit, 400 );
			if ( didHit )
				value = (short) rayHit.distance;
			else
				value = short.MaxValue;
			byte b1 = (byte) ( value >> 8 );
			byte b2 = (byte) ( value & 255 );
			b.WriteByte ( b1 );
			b.WriteByte ( b2 );
//			Debug.Log ( "dist " + rayHit.distance );
//			Debug.Log ( "short is " + ( (short) ( b1 << 8 ) + b2 ) );
		}
		cameraData = b.GetBytes ();
		Profiler.EndSample ();
	}

	void FixedUpdate ()
	{
		if ( MotorsEnabled )
		{
			// add force
			rb.AddRelativeForce ( force * Time.deltaTime, forceMode );

			// add torque
			if ( inverseFlag )
			{
				inverseFlag = false;
				torque = transform.InverseTransformDirection ( torque ) * torqueForce;
			}
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
		Vector3 force = Force.ToRos ();
//		force = new Vector3 ( -force.x, force.z, force.y );
		GUI.Label ( r, "Force: " + force.ToString () );
		r.y += r.height;
		force = Torque.ToRos ();
		force = new Vector3 ( -force.x, force.z, force.y );
		GUI.Label ( r, "Torque: " + force.ToString () );
//		if ( useTeleop )
//		{
			r.y += r.height;
		GUI.Label ( r, "Position: " + Position.ToRos ().ToString () );
			r.y += r.height;
		GUI.Label ( r, "PRY: " + Rotation.eulerAngles.ToRos ().ToString () );
//		}
		r.y += r.height;
		force = AngularVelocity.ToRos ();
//		force = new Vector3 ( -force.x, force.z, force.y );
		GUI.Label ( r, "Angular Vel.: " + force.ToString () );
		r.y += r.height;
		force = LinearAcceleration.ToRos ();
//		force = new Vector3 ( -force.x, force.z, force.y );
		GUI.Label ( r, "Linear Accel.: " + force.ToString () );
	}

	public void ApplyMotorForce (Vector3 v, bool convertFromRos = false)
	{
		force = v;
		if ( convertFromRos )
			force = force.ToUnity ();
		force *= thrustForce;
	}

	public void ApplyMotorTorque (Vector3 v, bool convertFromRos = false)
	{
		torque = v;
		if ( convertFromRos )
			torque = torque.ToUnity ();
		torque *= convertFromRos ? -torqueForce : torqueForce;
	}

/*	public void ApplyMotorForce (float x, float y, float z, bool swapAxes = false, bool invertAxes = false)
	{
		force.x = x;
		force.y = swapAxes ? z : y;
		force.z = swapAxes ? y : z;
		force *= thrustForce;
		if ( invertAxes )
			force *= -1;
	}

	public void ApplyMotorTorque (float x, float y, float z, bool swapAxes = false, bool invertAxes = false)
	{
		torque = XAxis * x;
		torque += YAxis * ( swapAxes ? y : z );
		torque += Up * ( swapAxes ? z : y );
		if ( invertAxes )
			torque *= -1;

		inverseFlag = true;
//		torque = transform.InverseTransformDirection ( torque ) * torqueForce;
		return;

		torque.x = x; // don't invert because, the rotation will already get inversed as the intended axis is inversed
		torque.y = swapAxes ? z : y;
		torque.z = swapAxes ? y : z;
		torque *= torqueForce;
	}*/

	public void ResetOrientation ()
	{
		transform.rotation = Quaternion.identity;
		force = Vector3.zero;
		torque = Vector3.zero;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		LinearAcceleration = Vector3.zero;
	}

	void CreateCameraTex ()
	{
		// for now, just prep a byte[] that we can put raycast data into


//		cameraTex = new RenderTexture ( ImageWidth, ImageHeight, 0, RenderTextureFormat.RHalf );
//		cameraTex.enableRandomWrite = true;
//		cameraTex.Create ();
	}

	public byte[] GetImageData ()
	{
		return cameraData;
	}
}