using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCamera : MonoBehaviour
{
	public Camera cam;
	public Shader replacementShader;
	Texture2D tex;

	float texTime;

	void OnEnable ()
	{
		if ( cam != null && replacementShader != null )
			cam.SetReplacementShader ( replacementShader, "" );
	}

	void OnDisable ()
	{
		cam.ResetReplacementShader ();
	}

	void Awake ()
	{
		tex = new Texture2D ( cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false );
		tex.Apply ();
	}

	void Update ()
	{
		if ( Time.time > texTime )
		{
			RenderTexture rt = cam.targetTexture;
			int w = Random.Range ( 0, rt.width );
			int h = Random.Range ( 0, rt.height );
			RenderTexture.active = rt;
			tex.ReadPixels ( new Rect ( 0, 0, rt.width, rt.height ), 0, 0 );
			tex.Apply ();
			RenderTexture.active = null;
			Color c = tex.GetPixel ( w, h );
			Debug.Log ( "Color is: " + c );
			texTime = Time.time + 2;
		}
	}
}
