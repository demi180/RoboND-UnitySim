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

		if ( Input.GetKeyDown ( KeyCode.R ) )
		{
			ResetOrientation ();
		}

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



		return;

		// draw the axis arrows
		float arrowSize = arrowScreenSize * Screen.height / 1080f / 2;
		Vector3 upAxis;
		float rotAngle;
		transform.rotation.ToAngleAxis ( out rotAngle, out upAxis );
		Vector2 centerScreen = new Vector2 ( Screen.width/2, Screen.height/2 );
		Vector2 arrow1Pos = centerScreen - Vector2.up * Screen.height / 4;
		Vector2 arrow2Pos = centerScreen - Vector2.right * Screen.width / 4 - Vector2.up * Screen.height / 4;
		Vector2 arrow3Pos = centerScreen + Vector2.right * Screen.width / 4 - Vector2.up * Screen.height / 4;

		// begin rotation to drone axis
		GUIUtility.RotateAroundPivot ( -rotAngle, centerScreen );

		// top (z) arrow
		GUIUtility.RotateAroundPivot ( -90, arrow1Pos );
//		Vector2 pos = centerScreen - Vector2.up * Screen.height / 4;
		Rect arrowRect = new Rect ( arrow1Pos.x - arrowSize, arrow1Pos.y - arrowSize, arrowSize * 2, arrowSize * 2 );
//		GUI.color = Color.white;
//		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUI.color = axisColors [ 2 ];
		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUIUtility.RotateAroundPivot ( 90, arrow1Pos );

		// y arrow
		GUIUtility.RotateAroundPivot ( -135, arrow2Pos );
//		pos = centerScreen - Vector2.right * Screen.width / 4 - Vector2.up * Screen.height / 4;
		arrowRect = new Rect ( arrow2Pos.x - arrowSize, arrow2Pos.y - arrowSize, arrowSize * 2, arrowSize * 2 );
		GUI.color = Color.white;
		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUI.color = axisColors [ 1 ];
		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUIUtility.RotateAroundPivot ( 135, arrow2Pos );

		// x arrow
		GUIUtility.RotateAroundPivot ( -45, arrow3Pos );
//		pos = centerScreen + Vector2.right * Screen.width / 4 - Vector2.up * Screen.height / 4;
		arrowRect = new Rect ( arrow3Pos.x - arrowSize, arrow3Pos.y - arrowSize, arrowSize * 2, arrowSize * 2 );
		GUI.color = Color.white;
		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUI.color = axisColors [ 0 ];
		GUI.DrawTexture ( arrowRect, axisArrows [ 0 ] );
		GUIUtility.RotateAroundPivot ( 45, arrow3Pos);

		// reset rotation
		GUI.matrix = Matrix4x4.identity;
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