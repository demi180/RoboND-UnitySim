using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects ;

public class RoverController : IRobotController
{
	Vector3 GroundVelocity { get { return new Vector3 ( rb.velocity.x, 0, rb.velocity.z ); } }
	Vector3 VerticalVelocity { get { return new Vector3 ( 0, rb.velocity.y, 0 ); } }
	public override float Zoom { get { return cameraDefaultFOV / camera.fieldOfView; } }

	public Transform flatbed;
	public Rigidbody rb;
	public WheelCollider[] wheels;
	public Transform[] wheelMeshes;
	public Transform robotArmShoulder;
	public Transform robotGrabPoint;
	public SphereCollider cameraCollider;
	public RobotArmActuator armActuator;
	public List<Transform> storedLocations;

	public float cameraMinAngle;
	public float cameraMaxAngle;
	public float cameraDefaultFOV = 60;
	public float cameraMinFOV;
	public float cameraMaxFOV;

	public float maxReverseSpeed = 2f;
	public float acceleration = 2.5f;
	public float motorTorque;
	public float brakeTorque;
	public float maxSteering = 25;
	public float downForce = 50;

	int curCamera;

	DepthOfField depthOfField;
	Quaternion[] localRotations = new Quaternion[4];
	float lastMoveInput;
	float lastBrakeInput;
	[SerializeField]
	float lastAngle;
	float lastSteerTime;
	float lastSteerInput;

	bool isMotorInput;
	bool isSteeringInput;
	bool isPickingUp;
	bool grabbingObject;
	bool isTurningInPlace;
	System.Action<GameObject> pickupCallback;

	// gizmo drawing vars
	Vector3 point1, point2;
	float radius;

	void Awake ()
	{
		recordTime = 1f / recordRate;
		actualCamera.SetParent ( fpsPosition );
		rb.centerOfMass = new Vector3 ( 0, -1, 0 );
		for ( int i = 0; i < 4; i++ )
			localRotations [ i ] = wheelMeshes [ i ].transform.localRotation;
		depthOfField = camera.GetComponent<DepthOfField> ();
		ResetZoom ();

		armActuator.onArrive = OnPickup;
	}

	void Start ()
	{
		armActuator.Fold ();
	}

	void FixedUpdate ()
	{
		if ( !isSteeringInput )
		{
			lastAngle = 0;
			lastSteerInput = 0;
		}

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
			isTurningInPlace = false;
			wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = lastMoveInput * motorTorque;
			wheels [ 0 ].brakeTorque = wheels [ 1 ].brakeTorque = wheels [ 2 ].brakeTorque = wheels [ 3 ].brakeTorque = lastBrakeInput * brakeTorque;
//			Debug.Log ( "lmi " + lastMoveInput + " lbi " + lastBrakeInput );
/*			if ( Vector3.Angle ( robotBody.forward * lastMoveInput, GroundVelocity ) < 90 )
			{
				wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = lastMoveInput * motorTorque;
				wheels [ 0 ].brakeTorque = wheels [ 1 ].brakeTorque = wheels [ 2 ].brakeTorque = wheels [ 3 ].brakeTorque = lastBrakeInput * brakeTorque;
//				wheels [ 0 ].brakeTorque = wheels [ 1 ].brakeTorque = wheels [ 2 ].brakeTorque = wheels [ 3 ].brakeTorque = 0;

			} else
			{
				wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = 0;
				wheels [ 0 ].brakeTorque = wheels [ 1 ].brakeTorque = wheels [ 2 ].brakeTorque = wheels [ 3 ].brakeTorque = Mathf.Abs ( lastMoveInput ) * brakeTorque;
			}*/
		} else
		{
			if ( isTurningInPlace )
			{
				// turning in place now just literally turns in place
//				wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = Mathf.Abs ( lastSteerInput ) * motorTorque * 2;
//				wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = Mathf.Abs ( lastSteerInput ) * motorTorque * 2;

//				wheels [ 0 ].motorTorque = wheels [ 1 ].motorTorque = -motorTorque * moveSpeed / 2;
//				wheels [ 2 ].motorTorque = wheels [ 3 ].motorTorque = -motorTorque * moveSpeed / 2;
//				wheels[0].motorTorque = wheels[1].motorTorque = wheels[2].motorTorque = wheels[3].motorTorque = -motorTorque * speed;
				wheels[0].brakeTorque = wheels[1].brakeTorque = wheels[2].brakeTorque = wheels[3].brakeTorque = 0;
				
			} else
			{
				wheels[0].brakeTorque = wheels[1].brakeTorque = wheels[2].brakeTorque = wheels[3].brakeTorque = 0;
				wheels[0].motorTorque = wheels[1].motorTorque = wheels[2].motorTorque = wheels[3].motorTorque = 0;
			}
		}

		float maxSpeed = lastMoveInput >= 0 ? moveSpeed : maxReverseSpeed;
		float speedPercent = GroundVelocity.magnitude / maxSpeed;
		rb.AddForce ( Vector3.down * downForce * speedPercent );
//		rb.AddForce ( -robotBody.up * downForce * speedPercent );
		if ( GroundVelocity.magnitude > maxSpeed )
		{
			Vector3 velo = GroundVelocity;
			velo = velo.normalized * maxSpeed;
			velo += VerticalVelocity;
			rb.velocity = velo;
		}

		if ( isSteeringInput && !isMotorInput )
		{
			if ( GroundVelocity.sqrMagnitude < 0.1f )
				isTurningInPlace = true;
		}

		if ( isTurningInPlace )
		{
//			SteerAngle = lastSteerInput * hRotateSpeed;
			robotBody.Rotate ( Vector3.up * lastSteerInput * hRotateSpeed * Time.deltaTime );
			wheels [ 0 ].steerAngle = wheels [ 3 ].steerAngle = 45 * Mathf.Abs ( lastSteerInput );
			wheels [ 1 ].steerAngle = wheels [ 2 ].steerAngle = -45 * Mathf.Abs ( lastSteerInput );
//			wheels [ 0 ].steerAngle = wheels [ 1 ].steerAngle = lastAngle * 2; //  45 * Mathf.Sign ( lastAngle );
//			wheels [ 2 ].steerAngle = wheels [ 3 ].steerAngle = -lastAngle * 2; // -45 * Mathf.Sign ( lastAngle );
		} else
		{
			wheels[0].steerAngle = wheels[1].steerAngle = lastAngle;
			wheels[2].steerAngle = wheels[3].steerAngle = 0;
		}
	}

	void Update ()
	{
		if ( IsRecording )
			GetSample ();
		
		if ( !isPickingUp )
		{
			// check for objectives. only if not already picking one up
			// start by building a capsule of the player's size
			radius = 0.5f;
//			point1 = robotArmShoulder.position + Vector3.up * radius;
//			point2 = robotArmShoulder.position;
			point1 = robotArmShoulder.position + robotBody.forward * 0.5f + Vector3.up * radius;
			point2 = robotArmShoulder.position + robotBody.forward * 0.5f;
//			point1 = robotBody.position + robotBody.forward + Vector3.up * radius;
//			point2 = point1 + Vector3.up * ( 1 - radius * 2 );
			Debug.DrawLine ( point2, point2 + robotBody.forward * radius, Color.green );
			Debug.DrawLine ( point1, point1 + robotBody.forward * radius, Color.blue );
//			Debug.DrawLine ( point1, point1 + robotBody.right * radius, Color.red );
			// then check for a capsule ahead of the player.
			Collider[] objectives = Physics.OverlapCapsule ( point2, point1, radius, objectiveMask );
			if ( objectives != null && objectives.Length > 0 )
			{
//				Vector3 localShoulder = robotBody.InverseTransformPoint ( robotArmShoulder.position );
//				Vector3 localTarget = robotBody.InverseTransformPoint ( objectives[0].transform.position );
//				Debug.DrawLine ( robotArmShoulder.position, robotArmShoulder.position + localShoulder, Color.blue );
//				Debug.DrawLine ( robotArmShoulder.position, robotArmShoulder.position + localTarget, Color.green );
//				if ( localTarget.z > localShoulder.z )
//				if ( (objectives[0].transform.position - robotArmShoulder.position).sqrMagnitude > 0.1f )
//				{
					IsNearObjective = true;
					curObjective = objectives [ 0 ].gameObject;
//				}
				if ( objectives.Length > 1 )
					Debug.Log ( "Near " + objectives.Length + " objectives." );
			} else
			{
				IsNearObjective = false;
				curObjective = null;
			}
//			Speed = GroundVelocity.magnitude;
			float spd = GroundVelocity.magnitude;
			// if moving forward
			if ( spd >= 0.01f && Vector3.Angle ( robotBody.forward, GroundVelocity ) > 90 )
				spd *= -1;
			else
			if ( spd < 0.01f )
				spd = 0;
			Speed = spd;
		} else
		{
			Speed = 0;
		}


		isSteeringInput = false;
		lastAngle = 0;
		SteerAngle = 0;
		ThrottleInput = 0;
		isMotorInput = false;
		lastMoveInput = 0;
		lastSteerInput = 0;
//		isTurningInPlace = false;
	}

	public override void Move (float throttle, float brake)
	{
		ThrottleInput = throttle;
		BrakeInput = brake;
		lastMoveInput = throttle;
		lastBrakeInput = brake;
//		Debug.Log ( "throt: " + lastMoveInput + " brake: " + lastBrakeInput );
		isMotorInput = throttle != 0 || brake != 0;
		if ( isMotorInput && !isPickingUp )
			armActuator.enabled = false;
	}

	public override void Move (float input)
	{
		ThrottleInput = input;
		lastMoveInput = input; // * acceleration;
		isMotorInput = input != 0;
	}

	public override void Move (Vector3 direction)
	{
//		direction.y = rb.velocity.y;
//		rb.velocity = direction;
	}

	public override void Rotate (float angle)
	{
		angle = Mathf.Clamp ( angle, -1f, 1f );
		SteerAngle = angle * maxSteering;
		if ( angle != 0 )
		{
			lastSteerInput = angle;
			lastSteerTime = Time.time;
			lastAngle = angle * maxSteering;
			lastAngle = Mathf.Clamp ( lastAngle, -maxSteering, maxSteering );
			isSteeringInput = true;
			if ( !isPickingUp )
				armActuator.enabled = false;

		} else
		{
			lastAngle = 0;
			SteerAngle = 0;
			isSteeringInput = false;
		}
	}

	public override void RotateCamera (float horizontal, float vertical)
	{
		if ( curCamera == 0 )
		{
			cameraHAxis.Rotate ( Vector3.up * horizontal );
			Vector3 euler = cameraVAxis.localEulerAngles;
			euler.x -= vertical;
			if ( euler.x > 270 )
				euler.x -= 360;
			euler.x = Mathf.Clamp ( euler.x, cameraMinAngle, cameraMaxAngle );
			cameraVAxis.localEulerAngles = euler;
		} else
		{
			tpsPosition.RotateAround ( robotBody.position, Vector3.up, horizontal );
			tpsPosition.RotateAround ( robotBody.position, tpsPosition.right, -vertical );
			Vector3 euler = tpsPosition.localEulerAngles;
			euler.z = 0;
//			euler.y = euler.z = 0;
			tpsPosition.localEulerAngles = euler;
		}

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

	public override void PickupObjective (System.Action<GameObject> onPickup)
	{
		// step 1: find pickup position
		// step 2: actuate arm toward pickup
		// 2a. rotate shoulder, arm, piston
		// 2b. open claw to open position
		// step 3: close claw on pickup
		rb.velocity = Vector3.zero;
		rb.isKinematic = true;
		pickupCallback = onPickup;
		isPickingUp = true;
		armActuator.enabled = true;
//		Time.timeScale = 0.2f;
		StartCoroutine ( DoPickup () );
	}

	IEnumerator DoPickup ()
	{
		grabbingObject = false;
		armActuator.SetTarget ( curObjective.transform.position );
		armActuator.Unfold ( true );
//		armActuator.MoveToTarget ();
//		yield return StartCoroutine ( RotateTo ( curObjective.transform.position ) );
		while ( !grabbingObject )
			yield return null;
		IsNearObjective = false;
		Vector3 pos = GetPositionOnFlatbed ();
//		armActuator.SetTarget ( pos );
//		armActuator.MoveToTarget ();
		armActuator.MoveTarget ( pos );
		while ( grabbingObject )
			yield return null;
//		yield return StartCoroutine ( RotateTo ( pos ) );
		curObjective.transform.rotation = Quaternion.identity;
		curObjective.transform.parent = robotBody;
		Collider c = curObjective.GetComponent<Collider> ();
		float bottomY = c.bounds.min.y;
//		if ( bottomY < curObjective.transform.position.y )
//		{
//			float delta = curObjective.transform.position.y - bottomY;
//			curObjective.transform.position 
//		}
		curObjective.transform.position = pos;
		grabbingObject = false;
		isPickingUp = false;
		rb.isKinematic = false;
		armActuator.Fold ( true );
//		Time.timeScale = 1;
	}

/*	IEnumerator RotateTo (Vector3 targetPosition)
	{
		yield return null;
		Vector3 toTarget = targetPosition - robotArmShoulder.position;
		toTarget.y = 0;
		toTarget = toTarget.normalized;
//		Debug.DrawLine ( robotArmShoulder.position, robotArmShoulder.position + toTarget, Color.blue );
		Quaternion targetRot = Quaternion.LookRotation ( toTarget ) * storedShoulder;
		while ( Quaternion.Angle ( robotArmShoulder.rotation, targetRot ) > 0.1f )
		{
			robotArmShoulder.rotation = Quaternion.RotateTowards ( robotArmShoulder.rotation, targetRot, 50 * Time.deltaTime );
			yield return null;
		}
		robotArmShoulder.rotation = targetRot;
	}*/

	public void OnPickup ()
	{
//		if ( pickupCallback != null )
//			pickupCallback ( curObjective );
//		else
//			Destroy ( curObjective );
//		curObjective = null;
//		IsNearObjective = false;
		curObjective.transform.position = robotGrabPoint.position;
		curObjective.transform.parent = robotGrabPoint;
		grabbingObject = !grabbingObject;
//		grabbingObject = true;
//		isPickingUp = false;
//		if ( curCamera == 0 )
//			actualCamera.SetParent ( fpsPosition, false );
	}

	Vector3 GetPositionOnFlatbed ()
	{
		if ( storedLocations.Count > 0 )
		{
			Vector3 pos = storedLocations [ 0 ].position;
			Destroy ( storedLocations [ 0 ].gameObject );
			storedLocations.RemoveAt ( 0 );
			return pos;
		}

		throw new System.NullReferenceException ( "No more positions left on the rover!" );
//		Collider c = curObjective.GetComponent<Collider> ();
//		c.enabled = false;
//		curObjective.transform.rotation = flatbed.rotation;
//		BoxCollider b = flatbed.GetComponent<BoxCollider> ();
//		Vector3 size = flatbed.rotation * flatbed.localScale;// / 2;
//		Debug.DrawLine ( flatbed.position, flatbed.position - size/2, Color.green, 5 );
//		Debug.DrawLine ( flatbed.position, flatbed.position + size/2, Color.green, 5 );
//		size.z /= 3;
//		size.x /= 2;
//		Vector3 rand = -flatbed.forward * flatbed.localScale.z * 1 / 6 + flatbed.forward * Random.Range ( -size.z, size.z ) + flatbed.right * Random.Range ( -size.x, size.x );
//		Vector3 rand = flatbed.forward * Random.Range ( -size.z, size.z ) + flatbed.right * Random.Range ( -size.x, size.x );

//		return flatbed.position + rand;
	}

	public override void CarryObjective (GameObject objective)
	{
		if ( objective == null )
			return;
		Collider c = objective.GetComponent<Collider> ();
		c.isTrigger = true;
		c.enabled = false;
		Destroy ( c );
		objective.transform.rotation = flatbed.rotation;
		BoxCollider b = flatbed.GetComponent<BoxCollider> ();
		Vector3 size = flatbed.rotation * flatbed.localScale / 2;
//		Debug.DrawLine ( flatbed.position, flatbed.position - size, Color.blue, 2 );
//		Debug.DrawLine ( flatbed.position, flatbed.position + size, Color.red, 2 );
		Vector3 rand = flatbed.forward * Random.Range ( -size.z, size.z ) + flatbed.right * Random.Range ( -size.x, size.x );
//		Vector3 rand = new Vector3 ( Random.Range ( -size.x, size.x ) * flatbed.localScale.x, 0, Random.Range ( -size.z, size.z ) * flatbed.localScale.z );
		objective.transform.position = flatbed.position + rand;
		objective.transform.parent = flatbed;
	}

	public void OnAnimatorIK (int layer)
	{
		Debug.Log ( "animator IK" );
	}
}