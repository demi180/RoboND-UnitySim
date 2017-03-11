using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectiveSpawner : MonoBehaviour
{
	public GameObject[] objectives;
	public int spawnCount;

	void Start ()
	{
		if ( spawnCount == 0 )
			spawnCount = Random.Range ( 1, objectives.Length );
		
		List<int> indices = new List<int> ();
		for ( int i = 0; i < objectives.Length; i++ )
		{
			indices.Add ( i );
			objectives [ i ].gameObject.SetActive ( false );
		}

		for ( int i = 0; i < spawnCount; i++ )
		{
			int index = Random.Range ( 0, indices.Count );
			GameObject ob = objectives [ indices [ index ] ];
			ob.SetActive ( true );
			ob.GetComponent<Renderer> ().material.color = new Color ( 0.1f, 0.1f, 0.8f );
			indices.RemoveAt ( index );
		}
	}
}