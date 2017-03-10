using UnityEngine;

public abstract class IRobotController : MonoBehaviour
{
	public abstract Vector3 GroundVelocity { get; }
	public abstract Vector3 VerticalVelocity { get; }
	public abstract float SteerAngle { get; }
	public abstract float Zoom { get; }
	public abstract void Move (float input);
	public abstract void Move (Vector3 direction);
	public abstract void Rotate (float angle);
	public abstract void RotateCamera (float horizontal, float vertical);
	public abstract void ZoomCamera (float amount);
	public abstract void ResetZoom ();
	public abstract void SwitchCamera ();
	public abstract Vector3 TransformDirection (Vector3 localDirection);

	public bool allowStrafe;
	public bool allowSprint;
	public bool allowJump;
	public float hRotateSpeed = 90;
	public float vRotateSpeed = 90;
	public float moveSpeed = 5;
	public float sprintMultiplier = 2;
	public float cameraZoomSpeed = 20;
	public float maxSlope = 50;
}