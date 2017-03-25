using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmActuator : MonoBehaviour
{
	public Transform baseTransform;
	public Transform shoulder;
	public Transform upperArm;
	public Transform elbow;
	public Transform forearm;
	public Transform wrist;
	public Transform target;
	public PointConstraint handConstraint;
	public Transform foldedPosition;
	public Transform targetPosition;

	public float rotationSpeed = 360;
	public float upperArmMinAngle = 10;
	public float upperArmMaxAngle = 60;
	public float elbowMinAngle = 25;
	public float elbowMaxAngle = 150;

	float upperArmLength;
	float forearmLength;
	float elbowEuler;

	public System.Action onArrive;
	bool announceArrive;
	float timeNotMoved;
	Vector3 lastWristPos;

	void Awake ()
	{
		upperArmLength = ( elbow.position - upperArm.position ).magnitude;
		forearmLength = ( wrist.position - elbow.position ).magnitude;
		elbowEuler = 150;
		handConstraint.weight = 0;
	}

	void LateUpdate ()
	{
		if ( target == null )
			return;

		bool didMove = false;

		// shoulder
		Vector3 toTarget = ( target.position - shoulder.position ).normalized;
		toTarget = Vector3.ProjectOnPlane ( toTarget, Vector3.up ).normalized;
		Vector3 up = Vector3.up;
		Quaternion targetRot = Quaternion.LookRotation ( toTarget, up );
		shoulder.rotation = Quaternion.RotateTowards ( shoulder.rotation, targetRot, rotationSpeed * Time.deltaTime );
		Debug.DrawLine ( shoulder.position, shoulder.position + toTarget, Color.blue );

		// turn to target first
		if ( Quaternion.Angle ( shoulder.rotation, targetRot ) > 0.1f )
			return;

		// use the law of cosines to find the angles we need
		// law says c² = a² + b² - 2ab*cos(C) where C opposite c
		// so C would be....
		// 2ab*cos(C) = a² + b² - c²
		// cos(C) = (a² + b² - c²) / 2ab
		// and C = Acos ( (a² + b² - c²) / 2ab )


		toTarget = target.position - upperArm.position;
		toTarget = Vector3.ProjectOnPlane ( toTarget, upperArm.right );
//		toTarget = Vector3.ClampMagnitude ( toTarget, upperArmLength + forearmLength );
		float a = upperArmLength;
		float b = forearmLength; // let alpha opposite b = upper arm angle
		float c = toTarget.magnitude; // let gamma opposite c = forearm angle
		float alpha = Mathf.Acos ( ( c * c + a * a - b * b ) / ( 2 * a * c ) ) * Mathf.Rad2Deg;
//		float gamma = Mathf.Acos ( ( a * a + b * b - c * c ) / ( 2 * a * b ) ) * Mathf.Rad2Deg;
//		if ( alpha == 0 || float.IsNaN ( alpha ) )
//			alpha = 0.1f;
//		if ( gamma > 180 || float.IsNaN ( gamma ) )
//			gamma = 179.9f;
//		if ( gamma <= 0 )
//			gamma = 0.1f;
//		Debug.Log ( "UAL: " + a + " FAL: " + b + " tot: " + c + " Alpha: " + alpha + " Gamma: " + gamma );
//		Debug.Log ( "cosAlpha = " + ( c * c + b * b - a * a ) / ( 2 * c * b ) );

//		toTarget = Vector3.ProjectOnPlane ( toTarget, upperArm.right );
//		Vector3 project = Vector3.Project ( upperArm.up * upperArmLength, toTarget );
//		Vector3 pos = upperArm.position + upperArm.up * upperArmLength;
//		Debug.DrawLine ( pos, upperArm.position + project, Color.white );
//		Debug.DrawLine ( pos, pos + project, Color.black );

		// upper arm
		alpha = Mathf.Clamp ( 90 - alpha, upperArmMinAngle, upperArmMaxAngle );
		Quaternion q = Quaternion.AngleAxis ( alpha, upperArm.right );
		targetRot = Quaternion.LookRotation ( toTarget, upperArm.up );
		targetRot = q * targetRot;
		upperArm.rotation = Quaternion.RotateTowards ( upperArm.rotation, targetRot, rotationSpeed * Time.deltaTime );
		Vector3 euler = upperArm.localEulerAngles;
		euler.x = Mathf.Clamp ( euler.x, upperArmMinAngle, upperArmMaxAngle );
		euler.y = euler.z = 0;
		upperArm.localEulerAngles = euler;
		Debug.DrawRay ( upperArm.position, upperArm.forward, Color.blue );
		Debug.DrawRay ( upperArm.position, toTarget, Color.cyan );

		// elbow
		toTarget = target.position - elbow.position;
		toTarget = Vector3.ProjectOnPlane ( toTarget, elbow.right );
		float gamma = Vector3.Angle ( -upperArm.up, toTarget );
		gamma = Mathf.Clamp ( gamma, elbowMinAngle, elbowMaxAngle );
		elbowEuler = Mathf.MoveTowards ( elbowEuler, 180 - gamma, rotationSpeed * Time.deltaTime );
		euler = new Vector3 ( elbowEuler, 0, 0 );
		elbow.localEulerAngles = euler;
//		toTarget = target.position - elbow.position;
//		toTarget = Vector3.ProjectOnPlane ( toTarget, elbow.right );
//		toTarget = Vector3.ClampMagnitude ( toTarget, forearmLength );
//		Debug.Log ( "Gamma: " + gamma + " New gamma: " + ( 180 - gamma ) );//+ " Eulerx: " + euler.x );
//		gamma = Mathf.Clamp ( 180 - gamma, elbowMinAngle, elbowMaxAngle );
//		Vector3 look = Quaternion.AngleAxis ( gamma, elbow.right ) * upperArm.up;
//		targetRot = Quaternion.LookRotation ( look, -elbow.forward );
//		q = Quaternion.AngleAxis ( 90, elbow.right );
//		targetRot = Quaternion.LookRotation ( toTarget, elbow.up );
//		targetRot = q * targetRot;
//		toTarget = target.position - elbow.position;
//		toTarget = Vector3.ProjectOnPlane ( toTarget, elbow.right );
//		toTarget = Vector3.ClampMagnitude ( toTarget, forearmLength );
//		q = Quaternion.AngleAxis ( 90, elbow.right );
//		targetRot = Quaternion.LookRotation ( toTarget, elbow.up );
//		targetRot = q * targetRot;
		Debug.DrawRay ( elbow.position, toTarget, Color.magenta );
		Debug.DrawRay ( elbow.position, elbow.up, Color.green );
		Debug.DrawRay ( elbow.position, elbow.right, Color.red );
//		elbow.localRotation = Quaternion.RotateTowards ( elbow.localRotation, targetRot, rotationSpeed * Time.deltaTime );
//		elbow.rotation = Quaternion.RotateTowards ( elbow.rotation, targetRot, rotationSpeed * Time.deltaTime );
//		euler = elbow.localEulerAngles;
//		euler.x += 90;
//		gamma = 180 - gamma;
//		euler.x = Mathf.MoveTowards ( euler.x, gamma, rotationSpeed * Time.deltaTime );
//		euler.x = 180 - euler.x;
//		euler.x = Mathf.Clamp ( euler.x, elbowMinAngle, elbowMaxAngle );
//		if ( euler.y < 90 )
//		if ( Mathf.Approximately ( euler.y, 0 ) )
//			euler.y = 0;
//		else
//			euler.y = 180;
//		if ( euler.z < 90 )
//		if ( Mathf.Approximately ( euler.z, 0 ) )
//			euler.z = 0;
//		else
//			euler.z = 180;
//		euler.y = euler.z = 0;
//		Debug.Log ( "y: " + euler.y + " z: " + euler.z );
//		elbow.localEulerAngles = euler;
//		elbow.localEulerAngles = new Vector3 ( 150, 0, 0 );
		Debug.DrawRay ( elbow.position, elbow.forward, Color.blue );

		// piston?

		// wrist
		toTarget = shoulder.forward;
//		toTarget = ( target.position - wrist.position ).normalized;
//		if ( toTarget == Vector3.zero )
//			toTarget = wrist.forward;
//		toTarget = Vector3.ProjectOnPlane ( toTarget, Vector3.up ).normalized;
		targetRot = Quaternion.LookRotation ( toTarget );
		wrist.rotation = Quaternion.RotateTowards ( wrist.rotation, targetRot, rotationSpeed * Time.deltaTime );

		if ( wrist.position == lastWristPos )
			timeNotMoved += Time.deltaTime;
		else
			timeNotMoved = 0;
		lastWristPos = wrist.position;

		if ( announceArrive )
		{
			if ( timeNotMoved >= 0.5f )
//			if ( didMove && ( target.position - wrist.position ).sqrMagnitude < 0.01f )
//				didMove = false;
//			if ( !didMove )
			{
				announceArrive = false;
				if ( onArrive != null )
					onArrive ();
			}
		}
	}

	public void Fold (bool announce = false)
	{
		handConstraint.weight = 0;
		announceArrive = announce;
		timeNotMoved = 0;
	}

	public void Unfold (bool announce = false)
	{
		handConstraint.weight = 1;
		announceArrive = announce;
		timeNotMoved = 0;
	}

	public void MoveToTarget (float time = 2)
	{
		StartCoroutine ( MoveTo ( time ) );
	}

	IEnumerator MoveTo (float time)
	{
		yield return null;
		float t = 0;
		float spd = 1f / time;
		while ( t < time )
		{
			handConstraint.weight += Time.deltaTime * spd;
			if ( handConstraint.weight >= 1 )
				break;
			t += Time.deltaTime;
			yield return null;
		}
		handConstraint.weight = 1;
		if ( onArrive != null )
			onArrive ();
	}

	public void SetTarget (Vector3 position)
	{
		targetPosition.position = position;
	}

	public void MoveTarget (Vector3 position, float time = 3)
	{
		StartCoroutine ( MoveTo ( position, time ) );
	}

	IEnumerator MoveTo (Vector3 position, float time)
	{
		yield return null;
		float t = 0;
		float spd = 1f / time;
		float dist = ( position - targetPosition.position ).magnitude;
		Vector3 start = targetPosition.position;
		while ( t < 1f )
		{
			targetPosition.position = Vector3.Slerp ( start, position, t );
//			targetPosition.position = Vector3.MoveTowards ( targetPosition.position, position, dist * spd * Time.deltaTime );
			if ( targetPosition.position == position )
				break;
			t += Time.deltaTime * spd;
			yield return null;
		}

		targetPosition.position = position;
		if ( onArrive != null )
			onArrive ();
	}
}