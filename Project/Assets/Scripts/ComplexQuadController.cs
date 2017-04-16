using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexQuadController : MonoBehaviour
{
	public Transform frontLeftRotor;
	public Transform frontRightRotor;
	public Transform rearLeftRotor;
	public Transform rearRightRotor;

	public float moveSpeed = 10;
	public float thrustForce = 25;

	Rigidbody rb;

	void Awake ()
	{
		rb = GetComponent<Rigidbody> ();
	}

	void LateUpdate ()
	{
		Vector3 input = new Vector3 ( Input.GetAxis ( "Horizontal" ), Input.GetAxis ( "Thrust" ), Input.GetAxis ( "Vertical" ) );

		float thrust = thrustForce * input.y / 4 * Time.deltaTime;
		Vector3 upThrust = transform.up * thrust;
		float forwardTiltMultiplier = input.z > 0 ? Mathf.Lerp ( 1f, 0.8f, input.z ) : 1;
		float backwardTiltMultiplier = input.z < 0 ? Mathf.Lerp ( 1f, 0.8f, -input.z ) : 1;
		float rightTiltMultiplier = input.x > 0 ? Mathf.Lerp ( 1f, 0.8f, input.x ) : 1;
		float leftTiltMultiplier = input.x < 0 ? Mathf.Lerp ( 1f, 0.8f, -input.x ) : 1;

		rb.AddForceAtPosition ( upThrust * forwardTiltMultiplier * leftTiltMultiplier, frontLeftRotor.position, ForceMode.Acceleration );
		rb.AddForceAtPosition ( upThrust * forwardTiltMultiplier * rightTiltMultiplier, frontRightRotor.position, ForceMode.Acceleration );
		rb.AddForceAtPosition ( upThrust * backwardTiltMultiplier * leftTiltMultiplier, rearLeftRotor.position, ForceMode.Acceleration );
		rb.AddForceAtPosition ( upThrust * backwardTiltMultiplier * rightTiltMultiplier, rearRightRotor.position, ForceMode.Acceleration );

		float zAngle = transform.localEulerAngles.z;
		while ( zAngle > 180 )
			zAngle -= 360;
		while ( zAngle < -360 )
			zAngle += 360;
		transform.Rotate ( Vector3.up * -zAngle * Time.deltaTime, Space.World );
//		transform.Rotate ( Vector3.up * input.x * thrustForce * Time.deltaTime, Space.World );

		Vector3 velo = Vector3.ClampMagnitude ( rb.velocity, moveSpeed );
		rb.velocity = velo;
	}
}