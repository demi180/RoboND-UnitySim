using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Hjg.Pngcs;

public class OrbitCamera : MonoBehaviour
{
	public Transform viewCam;
	public Transform colorCam;
	public Transform bwCam;
	public Transform target;

	public float vRotSpeed = 90;
	public float hRotSpeed = 90;
	public float minvAngle = -70;
	public float maxVAngle = 20;
	public float minDist = 1;
	public float maxDist = 20;
	public float distSpeed = 1;
	public float timeScale = 1;
	public float recordFrequency = 10;

	public Shader whiteShader;


	Transform tr;
	Camera cam1;
	Camera cam2;
	float distanceDelta;
	float vAngleDelta;
	float desiredZ;
	float eulerX;

	float lastTimeChange;
	string timeLabel;
	bool recording;
	float nextRecordTime;
	int imageCount;
	string path1;
	string path2;
	Texture2D tex1;
	Texture2D tex2;
	object lock1 = new object ();
	object lock2 = new object ();

	void Awake ()
	{
		tr = transform;
		distanceDelta = distSpeed;
		vAngleDelta = vRotSpeed;
		colorCam.localPosition = bwCam.localPosition = -Vector3.forward * minDist;
		cam1 = colorCam.GetComponent<Camera> ();
		cam2 = bwCam.GetComponent<Camera> ();
		cam2.SetReplacementShader ( whiteShader, "" );
		Time.timeScale = timeScale;
		recordFrequency = 1f / recordFrequency;
		RecordingController.BeginRecordCallback = OnBeginRecording;
		RecordingController.EndRecordCallback = OnEndRecording;
	}

	void LateUpdate ()
	{
		if ( target == null )
			return;
		// position our camera base with the character's head, roughly
		tr.position = target.position + Vector3.up * 2;
		// rotate around the target horizontally
		tr.Rotate ( Vector3.up * hRotSpeed * Time.deltaTime );
		eulerX += vAngleDelta * Time.deltaTime;
		if ( eulerX <= minvAngle || eulerX >= maxVAngle )
		{
			eulerX = Mathf.Clamp ( eulerX, minvAngle, maxVAngle );
			vAngleDelta *= -1;
		}
		Vector3 euler = tr.localEulerAngles;
		euler.x = eulerX;
		euler.z = 0;
		tr.localEulerAngles = euler;

		// adjust the distance from target
		Vector3 lp = viewCam.localPosition;
		desiredZ += distanceDelta * Time.deltaTime;
//		lp.z += distanceDelta * Time.deltaTime;
		if ( desiredZ <= minDist || desiredZ >= maxDist )
		{
			desiredZ = Mathf.Clamp ( desiredZ, minDist, maxDist );
			distanceDelta *= -1;
		}

		// check if character is obscured from camera
		// use forward instead of -forward because cameras are rotated 180
		Ray ray = new Ray ( tr.position, tr.forward );
		RaycastHit hit;
		if ( Physics.SphereCast ( ray, 0.05f, out hit, desiredZ ) )
		{
//			Debug.Log ( "hitting " + hit.collider.name );
			lp.z = hit.distance - 0.1f;
			Debug.DrawRay ( ray.origin, ray.direction * hit.distance );

		} else
			lp.z = desiredZ;
//			lp.z = Mathf.MoveTowards ( lp.z, desiredZ, 10 * Time.deltaTime );
		// and assign the new distance
		viewCam.localPosition = colorCam.localPosition = bwCam.localPosition = lp;


		// adjust time scale
		int key0 = (int) KeyCode.Alpha0;
		for ( int i = 0; i < 10; i++ )
		{
			if ( Input.GetKeyDown ( (KeyCode) key0 + i ) )
			{
				timeScale = i;
				lastTimeChange = Time.unscaledTime;
				timeLabel = "Speed : ^0x".Replace ( "^0", i.ToString () );
				Time.timeScale = timeScale;
			}
		}

		if ( recording )
		{
			if ( Time.time >= nextRecordTime )
			{
				WriteImage ();
				nextRecordTime = Time.time + recordFrequency;
			}
		}
	}

	void OnGUI ()
	{
		float delta = Time.unscaledTime - lastTimeChange;
		if ( delta < 2 )
		{
			float a = 0;
			if ( delta < 0.2f )
				a = delta * 5;
			else
			if ( delta > 1.6f )
				a = ( 2f - delta ) * 2.5f;
			else
				a = 1;

			int fontSize = GUI.skin.label.fontSize;
			GUI.skin.label.fontSize = 40 * Screen.height / 1080;
			GUI.color = new Color ( 0, 0, 0, a / 2 );
			Rect r = new Rect ( 5, 5, 500, 100 );
			GUI.Label ( new Rect ( r.x + 1, r.y + 1, r.width, r.height ), timeLabel );
			GUI.color = new Color ( 1, 1, 1, a );
			GUI.Label ( r, timeLabel );
			GUI.skin.label.fontSize = fontSize;
		}
	}

	void OnBeginRecording ()
	{
		recording = true;
	}

	void OnEndRecording ()
	{
		recording = false;
	}

	void WriteImage ()
	{
		string prefix = imageCount.ToString ( "D5" );
//		Debug.Log ( "writing " + prefix );
		imageCount++;
		// needed to force camera update 
		RenderTexture targetTexture = cam1.targetTexture;
		RenderTexture.active = targetTexture;
		byte[] bytes;
		lock ( lock1 )
		{
			tex1 = new Texture2D ( targetTexture.width, targetTexture.height, TextureFormat.RGB24, false );
			tex1.ReadPixels ( new Rect ( 0, 0, targetTexture.width, targetTexture.height ), 0, 0 );
			tex1.Apply ();
			bytes = tex1.EncodeToPNG ();
		}
		string directory = RecordingController.SaveLocation;
		path1 = Path.Combine ( directory, "cam1_" + prefix + ".png" );
		Thread t1 = new Thread ( SaveImage1 );
		t1.Start ();

		targetTexture = cam2.targetTexture;
		lock ( lock2 )
		{
			tex2 = new Texture2D ( targetTexture.width, targetTexture.height, TextureFormat.RGB24, false );
			tex2.ReadPixels ( new Rect ( 0, 0, targetTexture.width, targetTexture.height ), 0, 0 );
			tex2.Apply ();
			bytes = tex2.EncodeToPNG ();
		}
		path2 = Path.Combine ( directory, "cam2_" + prefix + ".png" );
		Thread t2 = new Thread ( SaveImage2 );
		t2.Start ();

//		image = null;
		RenderTexture.active = null;
//		Destroy ( tex1 );
//		Destroy ( tex2 );
//		Destroy ( texture2D );
	}

	void SaveImage1 ()
	{
		lock ( lock1 )
		{
			byte[] bytes = tex1.EncodeToPNG ();
			File.WriteAllBytes ( path1, bytes );
			Destroy ( tex1 );
		}
		tex1.GetRawTextureData ();
	}

	void SaveImage2 ()
	{
		lock ( lock2 )
		{
			byte[] bytes = tex2.EncodeToPNG ();
			File.WriteAllBytes ( path2, bytes );
			Destroy ( tex2 );
		}

	}

	void Test ()
	{
		Texture2D tex = new Texture2D ( 1280, 720, TextureFormat.RGB24, false );
		tex.Apply ();
		ImageInfo info = new ImageInfo ( 1280, 720, 24, false );

		MemoryStream stream = new MemoryStream ( tex.GetRawTextureData () );
		Hjg.Pngcs.PngWriter writer = new PngWriter ( stream, info, Application.dataPath + "test123.png" );
//		writer.
		Destroy ( tex );
	}
}