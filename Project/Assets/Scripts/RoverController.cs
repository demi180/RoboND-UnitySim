using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects ;

public class RoverController : IRobotController
{
	Vector3 GroundVelocity { get { return new Vector3 ( rb.velocity.x, 0, rb.velocity.z ); } }
	Vector3 VerticalVelocity { get { return new Vector3 ( 0, rb.velocity.y, 0 ); } }
//	public override float SteerAngle { get { return lastAngle; } }
	public override float Zoom { get { return cameraDefaultFOV / camera.fieldOfView; } }

//	public Transform robotBody;
//	public Transform cameraHAxis;
//	public Transform cameraVAxis;
//	public Transform fpsPosition;
//	public Transform tpsPosition;
//	public Transform actualCamera;
//	public Camera camera;
	public Rigidbody rb;
	public WheelCollider[] wheels;
//	public WheelCollider wheels[0];
//	public WheelCollider wheels[1];
//	public WheelCollider wheels[2];
//	public WheelCollider wheels[3];
	public Transform[] wheelMeshes;

	public float cameraMinAngle;
	public float cameraMaxAngle;
	public float cameraDefaultFOV = 60;
	public float cameraMinFOV;
	public float cameraMaxFOV;

	public float acceleration = 2.5f;
	public float motorTorque;
	public float brakeTorque;
	public float maxSteering = 25;
	public float antiSteeringForce = 50;
	public float downForce = 50;

	int curCamera;

	DepthOfField depthOfField;
	Quaternion[] localRotations = new Quaternion[4];
	float lastMoveInput;
	[SerializeField]
	float lastAngle;
	float lastSteerTime;

	bool isMotorInput;
	bool isSteeringInput;

	void Awake ()
	{
		actualCamera.SetParent ( fpsPosition );
		rb.centerOfMass = new Vector3 ( 0, -1, 0 );
		for ( int i = 0; i < 4; i++ )
			localRotations [ i ] = wheelMeshes [ i ].transform.localRotation;
		depthOfField = camera.GetComponent<DepthOfField> ();
		ResetZoom ();
	}

	void FixedUpdate ()
	{
		if ( !isSteeringInput )
		{
			lastAngle = 0;
		}
//		if ( Time.time - lastSteerTime > 0.2f )
//		{
//			lastAngle = Mathf.MoveTowards ( lastAngle, 0, antiSteeringForce * Time.deltaTime );
//			isSteeringInput = false;
//		}

		for (int i = 0; i < 4; i++)
		{
			Quaternion quat;
			Vector3 position;
			wheels[i].GetWorldPose(out position, out quat);
			wheelMeshes[i].transform.position = position;
//			if ( isSteeringInput && !isMotorInput )
//				wheelMeshes [ i ].transform.rotation = Quaternion.LookRotation ( robotBody.forward, Vector3.up );
//			else
				wheelMeshes [ i ].transform.rotation = quat * localRotations [ i ];
		}

		if ( isMotorInput )
		{
			
			wheels[0].motorTorque = wheels[1].motorTorque = wheels[2].motorTorque = wheels[3].motorTorque = lastMoveInput * motorTorque;
			wheels[0].brakeTorque = wheels[1].brakeTorque = wheels[2].brakeTorque = wheels[3].brakeTorque = 0;

		} else
		{
			if ( isSteeringInput )
			{
				
//				wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = -motorTorque * moveSpeed / 2;
//				wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = -motorTorque * moveSpeed / 2;
//				wheels[0].motorTorque = wheels[1].motorTorque = wheels[2].motorTorque = wheels[3].motorTorque = -motorTorque * speed;
//				wheels[0].brakeTorque = wheels[1].brakeTorque = wheels[2].brakeTorque = wheels[3].brakeTorque = 0;
				
			} else
			{
				wheels[0].brakeTorque = wheels[1].brakeTorque = wheels[2].brakeTorque = wheels[3].brakeTorque = brakeTorque;
				wheels[0].motorTorque = wheels[1].motorTorque = wheels[2].motorTorque = wheels[3].motorTorque = 0;
			}
		}

		float speedPercent = GroundVelocity.magnitude / moveSpeed;
//		rb.AddForce ( Vector3.down * downForce * speedPercent );
		rb.AddForce ( -robotBody.up * downForce * speedPercent );
		if ( GroundVelocity.magnitude > moveSpeed )
		{
			Vector3 velo = GroundVelocity;
			velo = velo.normalized * moveSpeed;
			velo += VerticalVelocity;
			rb.velocity = velo;
		}


		if ( isSteeringInput && !isMotorInput )
//		if ( GroundVelocity.sqrMagnitude < 0.1f )
		{
			wheels [ 0 ].steerAngle = wheels [ 1 ].steerAngle = -lastAngle * 2; //  45 * Mathf.Sign ( lastAngle );
			wheels [ 2 ].steerAngle = wheels [ 3 ].steerAngle = lastAngle * 2; // -45 * Mathf.Sign ( lastAngle );
			
		} else
		{
			wheels[0].steerAngle = wheels[1].steerAngle = lastAngle;
			wheels[2].steerAngle = wheels[3].steerAngle = 0;
		}

//		if ( isSteeringInput && GroundVelocity.sqrMagnitude < 0.1f )
//			robotBody.Rotate ( robotBody.up * lastAngle * Time.deltaTime );
//			rb.AddRelativeTorque ( Vector3.up * lastAngle );
//		else
//			rb.angularVelocity = Vector3.zero;

		Speed = new Vector3 ( rb.velocity.x, 0, rb.velocity.z ).magnitude;
//		isSteeringInput = false;
	}

	void Update ()
	{
		isSteeringInput = false;
		lastAngle = 0;
		SteerAngle = 0;
		ThrottleInput = 0;
		isMotorInput = false;
		lastMoveInput = 0;
	}

	public override void Move (float input)
	{
		ThrottleInput = input;
		lastMoveInput = input * acceleration;
		isMotorInput = input != 0;
	}

	public override void Move (Vector3 direction)
	{
//		direction.y = rb.velocity.y;
//		rb.velocity = direction;
	}

	public override void Rotate (float angle)
	{
		SteerAngle = angle * maxSteering;
		if ( angle != 0 )
		{
			lastSteerTime = Time.time;
			lastAngle = angle * maxSteering;
//			lastAngle += angle;
//			lastAngle = Mathf.Clamp ( lastAngle, -hRotateSpeed, hRotateSpeed );
			lastAngle = Mathf.Clamp ( lastAngle, -maxSteering, maxSteering );
			isSteeringInput = true;

		} else
		{
			lastAngle = 0;
			SteerAngle = 0;
			isSteeringInput = false;
//			if ( Time.time - lastSteerTime > 0.2f )
//			{
//				lastAngle = Mathf.MoveTowards ( lastAngle, 0, antiSteeringForce * Time.deltaTime );
//				isSteeringInput = false;
//			}
		}
	}

	public override void RotateCamera (float horizontal, float vertical)
	{
//		cameraHAxis.Rotate ( Vector3.up * horizontal, Space.World );
//		cameraVAxis.Rotate ( Vector3.right * -vertical, Space.Self );
		Vector3 euler = cameraVAxis.localEulerAngles;
		euler.x -= vertical;
		if ( euler.x > 270 )
			euler.x -= 360;
		euler.x = Mathf.Clamp ( euler.x, cameraMinAngle, cameraMaxAngle );
		cameraVAxis.localEulerAngles = euler;
	}

	public override void ZoomCamera (float amount)
	{
		camera.fieldOfView += amount * cameraZoomSpeed;
		camera.fieldOfView = Mathf.Clamp ( camera.fieldOfView, cameraMinFOV, cameraMaxFOV );
	}

	public override void ResetZoom ()
	{
		camera.fieldOfView = cameraDefaultFOV;
		// this has issues in standalone for some reason...
//		camera.ResetFieldOfView ();
	}

	public override void SwitchCamera ()
	{
		if ( curCamera == 0 )
		{
			curCamera = 1;
			actualCamera.SetParent ( tpsPosition, false );
			if ( depthOfField != null )
				depthOfField.focalTransform = robotBody;

		} else
		{
			curCamera = 0;
			actualCamera.SetParent ( fpsPosition, false );
			if ( depthOfField != null )
				depthOfField.focalTransform = null;
		}
	}

	public override Vector3 TransformDirection (Vector3 localDirection)
	{
		return robotBody.TransformDirection ( localDirection );
	}
}