using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathing;

namespace Pathing
{
	public class Path
	{
		public PathSample[] Nodes { get { return nodes; } }
		PathSample[] nodes;
		
		public Path (PathSample[] samples)
		{
			nodes = samples;
		}
	}
	
	public class PathSample
	{
		public Vector3 position;
		public Quaternion orientation;
		public float timestamp;
	}
}

public class PathPlanner : MonoBehaviour
{
	static PathPlanner instance;
	public LineRenderer pathPrefab;
	public Transform nodePrefab;


	List<PathSample> path;
	LineRenderer pathRenderer;
	List<Transform> nodeObjects;

	void Awake ()
	{
		instance = this;
		pathRenderer = Instantiate ( pathPrefab, transform );
		pathRenderer.numPositions = 0;
//		pathRenderer.SetPositions ( new Vector3[0] );
		path = new List<PathSample> ();
		nodeObjects = new List<Transform> ();
	}

	public static void AddNode (Vector3 position, Quaternion orientation)
	{
		instance._AddNode ( position, orientation );
	}

	void _AddNode (Vector3 position, Quaternion orientation)
	{
		
		pathRenderer.numPositions = pathRenderer.numPositions + 1;
		pathRenderer.SetPosition ( pathRenderer.numPositions - 1, position );

		PathSample sample = new PathSample ();
		sample.position = position;
		sample.orientation = orientation;
		sample.timestamp = Time.time;
		path.Add ( sample );

		Transform node = Instantiate ( nodePrefab, position, orientation, transform );
		nodeObjects.Add ( node );
	}

	public static PathSample[] GetPath ()
	{
		return instance.path.ToArray ();
	}

	public static void Clear ()
	{
		instance.path.Clear ();
	}

	public static void ClearViz ()
	{
		instance.pathRenderer.numPositions = 0;
		int count = instance.nodeObjects.Count;
		for ( int i = 0; i < count; i++ )
			Destroy ( instance.nodeObjects [ i ].gameObject );
		instance.nodeObjects.Clear ();
	}
}