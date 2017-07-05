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
	public Vector3 LinearVelocity { get; protected set; }
	public Vector3 LinearAcceleration { get; protected set; }
	public Vector3 Forward { get; protected set; }
	public Vector3 Right { get; protected set; }
	public Vector3 Up { get; protected set; }
	public Vector3 YAxis { get; protected set; }
	public Vector3 XAxis { get; protected set; }
	public bool UseGravity { get; set; }
	public bool ConstrainForceX { get; set; }
	public bool ConstrainForceY { get; set; }
	public bool ConstrainForceZ { get; set; }
	public bool ConstrainTorqueX { get; set; }
	public bool ConstrainTorqueY { get; set; }
	public bool ConstrainTorqueZ { get; set; }

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

	public Texture2D[] axisArrows;
	public Color[] axisColors;
	public float arrowScreenSize = 100f;

	public bool rotateWithTorque;

	public bool spinRotors = true;
	public float maxRotorRPM = 3600;
	public float maxRotorSpeed = 360;
	[SerializeField]
	float curRotorSpeed;

	// recording vars
	public float pathRecordFrequency = 3;
	[System.NonSerialized]
	public bool isRecordingPath;
	float nextNodeTime;

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
	bool resetFlag;
	bool setPoseFlag;
	bool useTwist;

	Vector3 posePosition;
	Quaternion poseOrientation;

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
		transform.position = Vector3.up * 10;
		UseGravity = rb.useGravity;
		UpdateConstraints ();
		rb.maxAngularVelocity = Mathf.Infinity;
	}

	void Update ()
	{
		if ( resetFlag )
		{
			ResetOrientation ();
			resetFlag = false;
		}
		CheckSetPose ();

		Position = transform.position;
		Rotation = transform.rotation;
		Forward = forward.forward;
		Right = right.forward;
		Up = transform.up;
		XAxis = xAxis.forward;
		YAxis = yAxis.forward;

		if ( isRecordingPath && Time.time > nextNodeTime )
		{
			PathPlanner.AddNode ( Position, Rotation );
			nextNodeTime = Time.time + pathRecordFrequency;
		}
	}

	void LateUpdate ()
	{
		if ( resetFlag )
		{
			ResetOrientation ();
			resetFlag = false;
		}
		CheckSetPose ();

		if ( Input.GetKeyDown ( KeyCode.Escape ) )
			Application.Quit ();

//		if ( Input.GetKeyDown ( KeyCode.R ) )
//		{
//			ResetOrientation ();
//		}

		if ( Input.GetKeyDown ( KeyCode.P ) )
		{
			PathPlanner.AddNode ( Position, Rotation );
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

		// spin rotors if we need
		if ( spinRotors )
		{
			float rps = maxRotorRPM / 60f;
			float degPerSec = rps * 360;
			curRotorSpeed = degPerSec * force.y / thrustForce;

			// use forward for now because rotors are rotated -90x
			Vector3 rot = Vector3.forward * ( force.y / thrustForce ) * degPerSec * Time.deltaTime;
//			Vector3 rot = Vector3.forward * ( force.y / thrustForce ) * maxRotorSpeed * Time.deltaTime;
			frontLeftRotor.Rotate ( -rot );
			frontRightRotor.Rotate ( rot );
			rearLeftRotor.Rotate ( rot );
			rearRightRotor.Rotate ( -rot );
		}

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
		if ( resetFlag )
		{
			ResetOrientation ();
			resetFlag = false;
		}
		CheckSetPose ();

		rb.useGravity = UseGravity;
		CheckConstraints ();
		if ( MotorsEnabled )
		{
			if ( useTwist )
			{
				// just set linear and angular velocities, ignoring forces
				rb.velocity = LinearVelocity;
				rb.angularVelocity = AngularVelocity;

			} else
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
				
				// update acceleration
				LinearAcceleration = ( rb.velocity - lastVelocity ) / Time.deltaTime;
				lastVelocity = rb.velocity;
				LinearVelocity = rb.velocity;
				AngularVelocity = rb.angularVelocity;
			}
		}
	}

	void OnGUI ()
	{
		// background box
		Rect r = new Rect ( 10, 10, 180, 200 );
		GUI.Box ( r, "" );
		GUI.Box ( r, "" );

		// motor status
		r.x = 15;
		r.height = 20;
		GUI.Label ( r, "Motors enabled: <color=yellow>" + MotorsEnabled + "</color>" );

		// input force
		r.y += r.height;
		Vector3 force = Force.ToRos ();
		GUI.Label ( r, "Force: " + force.ToString () );

		// input torque
		r.y += r.height;
		force = Torque.ToRos ();
		force = new Vector3 ( -force.x, force.z, force.y );
		GUI.Label ( r, "Torque: " + force.ToString () );

		// position
		r.y += r.height;
		GUI.Label ( r, "Position: " + Position.ToRos ().ToString () );

		// orientation
		r.y += r.height;
		GUI.Label ( r, "PRY: " + Rotation.eulerAngles.ToRos ().ToString () );

		// linear velocity
		r.y += r.height;
		force = LinearVelocity.ToRos ();
		GUI.Label ( r, "Linear Vel.:" + force.ToString () );

		// angular velocity
		r.y += r.height;
		force = AngularVelocity.ToRos ();
		GUI.Label ( r, "Angular Vel.: " + force.ToString () );

		// linear acceleration
//		r.y += r.height;
//		force = LinearAcceleration.ToRos ();
//		GUI.Label ( r, "Linear Accel.: " + force.ToString () );
	}

	public void ApplyMotorForce (Vector3 v, bool convertFromRos = false)
	{
		useTwist = false;
		force = v;
		if ( convertFromRos )
			force = force.ToUnity ();
		force *= thrustForce;
	}

	public void ApplyMotorTorque (Vector3 v, bool convertFromRos = false)
	{
		useTwist = false;
		torque = v;
		if ( convertFromRos )
			torque = torque.ToUnity ();
		torque *= convertFromRos ? -torqueForce : torqueForce;
	}

	public void SetLinearVelocity (Vector3 v, bool convertFromRos = false)
	{
		useTwist = true;
		force = torque = Vector3.zero;
		LinearVelocity = convertFromRos ? v.ToUnity () : v;
	}

	public void SetAngularVelocity (Vector3 v, bool convertFromRos = false)
	{
		useTwist = true;
		force = torque = Vector3.zero;
		AngularVelocity = convertFromRos ? v.ToUnity () : v;
	}

	public void TriggerReset ()
	{
		resetFlag = true;
	}

	public void ResetOrientation ()
	{
		transform.rotation = Quaternion.identity;
		force = Vector3.zero;
		torque = Vector3.zero;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		LinearAcceleration = Vector3.zero;
		LinearVelocity = Vector3.zero;
		AngularVelocity = Vector3.zero;
		rb.isKinematic = true;
		rb.isKinematic = false;
	}

	void CheckSetPose ()
	{
		if ( setPoseFlag )
		{
			transform.position = posePosition;
			transform.rotation = poseOrientation;
			force = Vector3.zero;
			torque = Vector3.zero;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			LinearAcceleration = Vector3.zero;
			setPoseFlag = false;
		}
	}

	public void SetPositionAndOrientation (Vector3 pos, Quaternion orientation, bool convertFromRos = false)
	{
		setPoseFlag = true;
		if ( convertFromRos )
		{
			posePosition = posePosition.ToUnity ();
			poseOrientation = poseOrientation.ToUnity ();
		} else
		{
			posePosition = pos;
			poseOrientation = orientation;
		}
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

	public void BeginRecordPath ()
	{
		isRecordingPath = true;
		PathPlanner.AddNode ( Position, Rotation );
		nextNodeTime = Time.time + pathRecordFrequency;
	}

	public void EndRecordPath ()
	{
		PathPlanner.AddNode ( Position, Rotation );
		isRecordingPath = false;
	}

	void CheckConstraints ()
	{
		RigidbodyConstraints c = RigidbodyConstraints.None;
		if ( ConstrainForceX )
			c |= RigidbodyConstraints.FreezePositionZ;
		if ( ConstrainForceY )
			c |= RigidbodyConstraints.FreezePositionX;
		if ( ConstrainForceZ )
			c |= RigidbodyConstraints.FreezePositionY;
		if ( ConstrainTorqueX )
			c |= RigidbodyConstraints.FreezeRotationZ;
		if ( ConstrainTorqueY )
			c |= RigidbodyConstraints.FreezeRotationX;
		if ( ConstrainTorqueZ )
			c |= RigidbodyConstraints.FreezeRotationY;
		rb.constraints = c;
	}

	public void UpdateConstraints ()
	{
		ConstrainForceX = ( rb.constraints & RigidbodyConstraints.FreezePositionZ ) != 0;
		ConstrainForceY = ( rb.constraints & RigidbodyConstraints.FreezePositionX ) != 0;
		ConstrainForceZ = ( rb.constraints & RigidbodyConstraints.FreezePositionY ) != 0;
		ConstrainTorqueX = ( rb.constraints & RigidbodyConstraints.FreezeRotationZ ) != 0;
		ConstrainTorqueY = ( rb.constraints & RigidbodyConstraints.FreezeRotationX ) != 0;
		ConstrainTorqueZ = ( rb.constraints & RigidbodyConstraints.FreezeRotationY ) != 0;
	}
}