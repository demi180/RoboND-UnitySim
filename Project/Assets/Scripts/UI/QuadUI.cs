using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuadUI : MonoBehaviour
{
	public Text recordButtonText;

	void Awake ()
	{
	}

	public void OnRecordButton ()
	{
		if (QuadController.ActiveController.isRecordingPath)
		{
			recordButtonText.text = "Record Path";
			QuadController.ActiveController.EndRecordPath ();
		} else
		{
			recordButtonText.text = "Stop Recording";
			QuadController.ActiveController.BeginRecordPath ();
		}
	}
}