using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathing;

public enum DecelerationFactor
{
	Slow,
	Normal,
	Fast
}

public class PathFollower : MonoBehaviour
{
	public Path Path { get { return path; } }
	public bool HasPath { get { return path != null; } }
	public bool active;

	public QuadController quad;

	public float maxSpeed = 5;
	public float maxTorque = 17;
	public float minDist = 1;
	public LayerMask groundMask;

	Transform tr;
	Path path;
	PathSample destination;
	int curNode;
	Vector3 force;
	Vector3 torque;
	[SerializeField]
	bool following;


	void Awake ()
	{
		tr = transform;
		groundMask = LayerMask.GetMask ( "Ground" );
	}

	void FixedUpdate ()
	{
		if ( active && destination != null )
		{
			float testDistance = minDist * minDist;
//			if ( curNode == path.Nodes.Length - 1 )
//				testDistance = arriveDist * arriveDist;
			if ( ( destination.position - tr.position ).sqrMagnitude < testDistance )
			{
				if ( curNode == path.Nodes.Length - 1 )
				{
					path = null;
					destination = null;
					following = false;
					// clear the visualization of the path
					PathPlanner.ClearViz ();
					return;
				}

				curNode++;
				destination = path.Nodes [ curNode ];
			}
			following = true;
			UpdateSteering ();
		}
	}

	void LateUpdate ()
	{
	}

	void UpdateSteering ()
	{
		force = Vector3.zero;
		torque = Vector3.zero;

		UpdatePRY ();
		UpdateThrust ();

		quad.ApplyMotorForce ( force );
		quad.ApplyMotorTorque ( torque );
	}

	void UpdatePRY ()
	{
		Vector3 toTarget = destination.position - quad.Position;
		Vector3 targetPRY = Quaternion.LookRotation ( toTarget ).eulerAngles;
		Vector3 curPRY = quad.Rotation.eulerAngles;
//		torque.x =  
	}

	void UpdateThrust ()
	{
		Vector3 toTarget = destination.position = quad.Position;

	}

	public void SetPath (Path p)
	{
		if ( p != null && p.Nodes != null && p.Nodes.Length > 0 )
		{
			path = p;
			tr.position = p.Nodes [ 0 ].position;
			tr.rotation = p.Nodes [ 0 ].orientation;
			curNode = 1;
			destination = p.Nodes [ 1 ];
			Debug.Log ( "path set" );
			
		} else
		{
			path = null;
			destination = null;
			following = false;
		}
	}
}