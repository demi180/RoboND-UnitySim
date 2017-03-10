using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerController : IRobotController
{
	public override Vector3 GroundVelocity { get { return new Vector3 ( controller.velocity.x, 0, controller.velocity.z ); } }
	public override Vector3 VerticalVelocity { get { return new Vector3 ( 0, controller.velocity.y, 0 ); } }
	public override float SteerAngle { get { return robotBody.eulerAngles.y; } }
	public override float Zoom { get { return cameraDefaultFOV / camera.fieldOfView; } }

	public Transform robotBody;
	public Transform cameraHAxis;
	public Transform cameraVAxis;
	public Transform fpsPosition;
	public Transform tpsPosition;
	public Transform actualCamera;
	public Camera camera;
	public CharacterController controller;
	public Rigidbody rigidbody;
	public Animator animator;
	public Collider bodyCollider;

	public float cameraMinAngle;
	public float cameraMaxAngle;
	public float cameraMinFOV;
	public float cameraMaxFOV;
	public float cameraDefaultFOV = 60;

	public PhysicMaterial idleMaterial;
	public PhysicMaterial movingMaterial;
	public LayerMask collisionMask;

	[SerializeField]
	float slopeAngle;
	float moveInput;
	int curCamera;
	[SerializeField]
	bool isFacingSlope;

	void Awake ()
	{
		actualCamera.SetParent ( fpsPosition );
		ResetZoom ();
	}

	void Update ()
	{
		// check for slope angle ahead
		Vector3 kneePoint = robotBody.position + Vector3.up * 0.4f;
		float rayDistance = 1;
//		Ray ray = new Ray ( kneePoint, Vector3.down );
		Ray ray = new Ray ( kneePoint, ( robotBody.forward * Mathf.Sign ( moveInput ) - Vector3.up ).normalized );
		RaycastHit hit;
		if ( Physics.Raycast ( ray, out hit, rayDistance, 1 << collisionMask.value ) )
		{
			slopeAngle = Vector3.Angle ( hit.normal, Vector3.up );
			if ( slopeAngle >= maxSlope )
				isFacingSlope = true;
			else
				isFacingSlope = false;

		} else
		{
			isFacingSlope = false;
			slopeAngle = 0;
		}
		Debug.DrawLine ( ray.origin, ray.origin + ray.direction * rayDistance, Color.red );

		if ( isFacingSlope )
			animator.applyRootMotion = false;
		else
			animator.applyRootMotion = true;
//			moveInput = 0;
		animator.SetFloat ( "Vertical", moveInput );
		bodyCollider.material = moveInput == 0 ? idleMaterial : movingMaterial;
	}

	public override void Move (float input)
	{
//		controller.SimpleMove ( robotBody.forward * input * moveSpeed );
		moveInput = input;
	}

	public override void Move (Vector3 direction)
	{
		direction *= moveSpeed;
		direction.y = Physics.gravity.y * Time.deltaTime;
		controller.Move ( direction );
	}

	public override void Rotate (float angle)
	{
		robotBody.Rotate ( Vector3.up * angle * hRotateSpeed );
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
		camera.fieldOfView = 60;
		//		camera.ResetFieldOfView ();
	}

	public override void SwitchCamera ()
	{
		if ( curCamera == 0 )
		{
			curCamera = 1;
			actualCamera.SetParent ( tpsPosition, false );
		} else
		{
			curCamera = 0;
			actualCamera.SetParent ( fpsPosition, false );
		}
	}

	public override Vector3 TransformDirection (Vector3 localDirection)
	{
		return robotBody.TransformDirection ( localDirection );
	}
}