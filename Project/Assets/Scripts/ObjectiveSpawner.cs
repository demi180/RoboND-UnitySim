﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectiveSpawner : MonoBehaviour
{
	public GameObject[] prefabs;
	GameObject[] objectives;
	public int spawnCount;
	public Color[] colors;

	string colorProp = "_CristalColor";

	void Start ()
	{
		int count = transform.childCount;
		if ( spawnCount == 0 )
			spawnCount = Random.Range ( 1, count );


		objectives = new GameObject [ count ];
		List<int> indices = new List<int> ();
		for ( int i = 0; i < count; i++ )
		{
			objectives [ i ] = transform.GetChild ( i ).gameObject;
			indices.Add ( i );
			GameObject go = Instantiate ( prefabs [ Random.Range ( 0, prefabs.Length ) ] );
			go.transform.position = objectives [ i ].transform.position; // - Vector3.up * go.GetComponent<Collider> ().bounds.extents.y;
			go.transform.rotation = Quaternion.Euler ( new Vector3 ( 0, Random.Range ( 0f, 360f ), 0 ) );
			go.transform.SetParent ( transform );
			Destroy ( objectives [ i ] );
			objectives [ i ] = go;
			go.SetActive ( false );
//			objectives [ i ].SetActive ( false );
		}

		for ( int i = 0; i < spawnCount; i++ )
		{
			int index = Random.Range ( 0, indices.Count );
			GameObject ob = objectives [ indices [ index ] ];
			ob.SetActive ( true );
//			ob.GetComponent<Renderer> ().material.SetColor ( colorProp, colors [ Random.Range ( 0, colors.Length ) ] );
			indices.RemoveAt ( index );
		}
	}
}