using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Ros_CSharp;
using XmlRpc_Wrapper;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ROSStatus
{
	Disconnected,
	Connecting,
	Connected
}

public class ROSController : MonoBehaviour
{
	static object instanceLock = new object ();
	public static ROSController instance;
	public static ROSStatus Status
	{
		get {
			if ( instance == null )
				return ROSStatus.Disconnected;
			return instance.status;
		}
	}
	static object callbackLock = new object ();
	static Queue<Action> callbacks = new Queue<Action> ();
	static Queue<NodeHandle> nodes = new Queue<NodeHandle> ();
	public static bool delayedStart;

	public string rosMasterURI = "http://localhost:11311";
	public string nodePrefix = "";
	public bool overrideURI;

	ROSStatus status;
	bool starting;
	bool stopping;
	bool delete;
	bool connectedToMaster;

	void Awake ()
	{
		if ( instance != null && instance != this )
		{
			Debug.LogError ( "Too many ROSControllers! Only one must exist." );
			Destroy ( gameObject );
			return;
		}

		GetConfigFile ();

		Debug.LogWarning ( "Main thread ID " + System.Threading.Thread.CurrentThread.ManagedThreadId );

		status = ROSStatus.Disconnected;

//		Application.targetFrameRate = 0;
		if ( QualitySettings.vSyncCount == 2 )
			Application.targetFrameRate = 30;
		else
			Application.targetFrameRate = 60;

//		Debug.Log ( "ros master is " + ROS.ROS_MASTER_URI );
		if ( ( string.IsNullOrEmpty ( Environment.GetEnvironmentVariable ( "ROS_MASTER_URI", EnvironmentVariableTarget.User ) ) &&
		     string.IsNullOrEmpty ( Environment.GetEnvironmentVariable ( "ROS_MASTER_URI", EnvironmentVariableTarget.Machine ) ) ) || overrideURI )
			ROS.ROS_MASTER_URI = rosMasterURI;
//			delayedStart = true;
		instance = this;
		StartROS ();
		new Thread ( new ThreadStart ( UpdateMasterConnection ) ).Start ();
	}

	void Update ()
	{
		if ( ROS.isStarted () && ROS.ok && connectedToMaster )
			status = ROSStatus.Connected;
		else
			if ( ROS.shutting_down || !ROS.isStarted () || !ROS.ok )
			status = ROSStatus.Disconnected;
		else
			status = ROSStatus.Connecting;
	}

	void OnDestroy ()
	{
		StopROS ();
	}

	void OnApplicationQuit ()
	{
		StopROS ();
	}

	void GetConfigFile ()
	{
		string filename = Application.dataPath + "/ros_settings.txt";

		if ( File.Exists ( filename ) )
		{
//			Debug.Log ( "exists" );
			using ( var fs = new FileStream ( filename, FileMode.Open, FileAccess.Read ) )
			{
				byte[] bytes = new byte[fs.Length]; 
				fs.Read ( bytes, 0, bytes.Length );
				string json = System.Text.Encoding.UTF8.GetString ( bytes );
//				Debug.Log ( "json: " + json );
				JSONObject jo = new JSONObject ( json );
				if ( jo.HasField ( "override" ) && jo.GetField ( "override" ).b )
				{
					if ( jo.HasField ( "ip" ) && jo.GetField ( "ip" ).IsString )
						rosMasterURI = "http://" + jo.GetField ( "ip" ).str;
					if ( jo.HasField ( "port" ) && jo.GetField ( "port" ).IsNumber )
						rosMasterURI += ":" + ( (int) ( jo.GetField ( "port" ).n ) ).ToString ();
					else
						rosMasterURI += ":11311";
					Debug.Log ( "setting ip to " + rosMasterURI );
				}
			}
		} else
		{
//			Debug.Log ( "not exists" );
		}
	}

	void UpdateMasterConnection ()
	{
		while ( !ROS.shutting_down )
		{
			connectedToMaster = master.check ();
			Thread.Sleep ( 500 );
		}
		connectedToMaster = false;
		Thread.CurrentThread.Join ( 10 );
	}

/*	void OnGUI ()
	{
		float width = 150;
		float y = 5;
		float height = 20;
		GUI.Box ( new Rect ( 5, y, width + 5, 100 ), "" );
		GUI.Label ( new Rect ( 10, y, width, height ), "ROS started: " + ROS.isStarted () );
		y += height;
		GUI.Label ( new Rect ( 10, y, width, height ), "ROS OK: " + ROS.ok );
		y += height;
		GUI.Label ( new Rect ( 10, y, width, height ), "ROS stopping: " + ROS.shutting_down );
		y += height * 2;

		if ( ROS.isStarted () )
		{
			if ( GUI.Button ( new Rect ( 5, y, width + 5, height ), "Stop ROS" ) )
			{
				ROSController.StopROS ();
			}
		} else
		{
			if ( GUI.Button ( new Rect ( 5, y, width + 5, height ), "Start ROS" ) )
			{
				ROSController.StartROS ();
			}
		}
	}*/

	public static void StartROS (Action callback = null)
	{
		#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
			return;
		#endif

		lock ( instanceLock )
		{
			if ( instance == null )
			{
				lock ( callbackLock )
				{
					if ( callback != null )
						callbacks.Enqueue ( callback );
				}
				GameObject go = new GameObject ( "ROSController" );
				go.AddComponent<ROSController> ();
				return;
			}
		}

		if ( ROS.isStarted () && ROS.ok )
		{
			if ( callback != null )
			{
				new Thread ( new ThreadStart ( callback ) ).Start ();
//				callback ();
			}
			return;
		}

		lock ( callbackLock )
		{
			if ( callback != null )
				callbacks.Enqueue ( callback );
		}

		lock ( instanceLock )
		{
			if ( instance.starting )
				return;
		}

		// this gets set when the environment variable ROS_MASTER_URI isn't set
		if ( delayedStart )
			return;

//		string timeString = DateTime.UtcNow.ToString ( "MM_dd_yy_HH_MM_ss" );
//		Debug.Log ( timeString );
		lock ( instanceLock )
		{
			if ( instance.starting )
				return;
			
			instance.starting = true;
			instance.stopping = false;
			Debug.Log ( "ROS is starting" );
			if ( instance.nodePrefix == null )
				instance.nodePrefix = "";
//			instance.status = ROSStatus.Connecting;
			new System.Threading.Thread ( () =>
			{
				ROS.Init ( new string[0], instance.nodePrefix );
			} ).Start ();
			//		ROS.Init ( new string[0], instance.nodePrefix );
			instance.StartCoroutine ( instance.WaitForInit () );
		}
	}

	public static void StopROS ()
	{
		if ( ROS.isStarted () && !ROS.shutting_down && !instance.stopping )
		{
//			instance.status = ROSStatus.Disconnected;
			instance.starting = false;
			instance.stopping = true;
			while ( nodes.Count > 0 )
			{
				NodeHandle node = nodes.Dequeue ();
				node.shutdown ();
				node.Dispose ();
			}
			Debug.Log ( "stopping ROS" );
			ROS.shutdown ();
			ROS.waitForShutdown ();
		}
	}

	public static void AddNode (NodeHandle nh)
	{
		nodes.Enqueue ( nh );
	}

	IEnumerator WaitForInit ()
	{
		while ( !ROS.isStarted () && !ROS.ok && !stopping )
			yield return null;

		XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
		if ( ROS.ok && !stopping )
		{
			lock ( instanceLock )
			{
				starting = false;
			}
//			status = ROSStatus.Connected;
			Debug.Log ( "ROS Init successful" );
			lock ( callbackLock )
			{
				while ( callbacks != null && callbacks.Count > 0 )
				{
					Action action = callbacks.Dequeue ();
					new Thread ( new ThreadStart ( action ) ).Start ();
//					callbacks.Dequeue () ();
				}
			}
		}
	}
}