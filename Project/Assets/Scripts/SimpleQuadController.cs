using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuadController : MonoBehaviour
{
	public float moveSpeed = 10;
	public float thrustForce = 25;
	public float maxTilt = 22.5f;
	public float tiltSpeed = 22.5f;

	Rigidbody rb;
	float tiltX;
	float tiltZ;
	
	void Awake ()
	{
		rb = GetComponent<Rigidbody> ();
	}

	void LateUpdate ()
	{
		Vector3 input = new Vector3 ( Input.GetAxis ( "Horizontal" ), Input.GetAxis ( "Thrust" ), Input.GetAxis ( "Vertical" ) );

		Vector3 forwardVelocity = Vector3.fwd * input.z * moveSpeed;
		Vector3 sidewaysVelocity = Vector3.right * input.x * moveSpeed;
		Vector3 upVelocity = Vector3.up * input.y * thrustForce;
		Vector3 inputVelo = forwardVelocity + sidewaysVelocity + upVelocity;

		rb.AddRelativeForce ( inputVelo * Time.deltaTime, ForceMode.VelocityChange );
		transform.Rotate ( Vector3.up * input.x * thrustForce * Time.deltaTime, Space.World );
//		rb.AddTorque ( Vector3.up * input.x * moveSpeed * Time.deltaTime, ForceMode.VelocityChange );
		Vector3 velo = Vector3.ClampMagnitude ( rb.velocity, moveSpeed );
		rb.velocity = velo;

		tiltX = Mathf.MoveTowards ( tiltX, maxTilt * input.z, tiltSpeed * Time.deltaTime );
		tiltZ = Mathf.MoveTowards ( tiltZ, maxTilt * -input.x, tiltSpeed * Time.deltaTime );
		Vector3 euler = transform.localEulerAngles;
		euler.x = tiltX;
		euler.z = tiltZ;
		transform.localEulerAngles = euler;


	}
}