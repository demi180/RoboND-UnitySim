using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YAMLParser;

public class PoseServiceSetter : Editor
{
	[MenuItem ("MD5/MD5")]
	static void GetPoseMD5s ()
	{
		MD5.md5memo.Clear ();
		MD5.srvmd5memo.Clear ();
//		Debug.Log ( Application.dataPath );
		string fileName = Application.dataPath + "/Scripts/ROS/YAMLParser/SetPose.srv";
		while (true)
		{
			string fn = fileName.Replace ( "/", "\\" );
			if ( fn == fileName )
			{
				fileName = fn;
				break;
			}
			fileName = fn;
		}
		Debug.Log ( fileName );
		string md5 = MD5.Sum ( new FauxMessages.SrvsFile ( new MsgFileLocation ( fileName, "" ) ) );
		Debug.Log ( "md5 is " + md5 );
		Debug.Log ( "srv md5s:" );
		foreach ( KeyValuePair<string, string> pair in MD5.srvmd5memo )
			Debug.Log ( pair.Key + " : " + pair.Value );
		Debug.Log ( "msg md5s:" );
		foreach ( KeyValuePair<string, string> pair in MD5.md5memo )
			Debug.Log ( pair.Key + " : " + pair.Value );
	}
}