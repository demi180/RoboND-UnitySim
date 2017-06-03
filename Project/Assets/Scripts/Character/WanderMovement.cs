using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderMovement : MonoBehaviour
{
	public NavMeshAgent agent;

	Transform myTransform;
	float start;

	void Awake ()
	{
		myTransform = GetComponent<Transform> ();
		agent.autoBraking = false;

		start = Random.value * 1000f;
		float y = Random.value * 360f;
		Vector3 euler = transform.eulerAngles;
		euler.y = y;
		transform.eulerAngles = euler;
	}

	void LateUpdate ()
	{
		Vector3 euler = transform.eulerAngles;
		euler.y += 0.5f - Mathf.PerlinNoise ( start + Time.time, start + Time.time );
		transform.eulerAngles = euler;
		agent.Move ( transform.forward * agent.speed * Time.deltaTime );

		return;
//		Vector3 forward = Vector3.forward;
		Vector3 forward = transform.forward;
		forward.x = 0.5f - Mathf.PerlinNoise ( start + Time.time, 0 )/3;
		forward.z = 0.5f - Mathf.PerlinNoise ( 0, start + Time.time )/3;
//		Debug.Log ( "1\t\t" + Mathf.PerlinNoise ( start + Time.time, 0 ) );
//		Debug.Log ( "2\t\t" + Mathf.PerlinNoise ( 0, start + Time.time ) );
//		Debug.Log ( forward.normalized );
		transform.rotation = Quaternion.LookRotation ( forward.normalized, Vector3.up );
		agent.Move ( forward.normalized * agent.speed * Time.deltaTime );
	}
}