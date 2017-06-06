using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointSpawner : MonoBehaviour
{
	public Transform spawnTarget;
	public Transform[] spawnPoints;
	public Transform targetInstance;
	public OrbitCamera followCam;

	void Awake ()
	{
		Transform spawn = GetRandomPoint ();
		targetInstance = Instantiate ( spawnTarget );
		targetInstance.position = spawn.position;
		targetInstance.gameObject.SetActive ( true );
		followCam.target = targetInstance;
	}

	Transform GetRandomPoint ()
	{
		return spawnPoints [ Random.Range ( 0, spawnPoints.Length ) ];
	}
}