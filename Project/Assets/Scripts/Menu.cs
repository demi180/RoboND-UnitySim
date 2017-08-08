using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	public Canvas canvas;

	public GameObject rosObject;
	public GameObject quadObject;
	public GameObject quadCamObject;

	public GameObject peopleSpawnerObject;
	public GameObject peopleCamObject;
	public GameObject recordingObject;


	void Awake ()
	{
	}

	void Start ()
	{
		EnableCanvas ();
	}

	void LateUpdate ()
	{
		if ( Input.GetKeyDown ( KeyCode.F1 ) )
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene ( UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name );
		}
	}

	void EnableCanvas ()
	{
		canvas.enabled = true;
	}

	public void OnModeSelect (int mode)
	{
		// controls
		if ( mode == 0 )
		{
			rosObject.SetActive ( false );
		}

		// deep learning
		if ( mode == 1 )
		{
			rosObject.SetActive ( false );
		}

		canvas.enabled = false;
	}

	public void OnExitButton ()
	{
		Application.Quit ();
	}
}