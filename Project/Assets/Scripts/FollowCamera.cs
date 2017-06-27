using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ros_CSharp;
using Messages;
using Empty = Messages.std_srvs.Empty;
using SetInt = Messages.quad_controller.SetInt;
using SetFloat = Messages.quad_controller.SetFloat;

public enum CameraPoseType
{
	XNorm,
	YNorm,
	ZNorm,
	Iso,
	Free
}

public class FollowCamera : MonoBehaviour
{
	public static FollowCamera ActiveCamera;

	public QuadController target;
//	public Transform target;
	public float followDistance = 5;
	public float height = 4;

	public bool autoAlign = false;
	public Vector3 forward;
	public CameraPoseType poseType;

	NodeHandle nh;
	ServiceServer distanceSrv;
	ServiceServer poseTypeSrv;

	bool setRotationFlag;
	Quaternion targetRotation;
	float initialFollowDistance;

	void Awake ()
	{
		if ( ActiveCamera == null )
			ActiveCamera = this;
		initialFollowDistance = followDistance;
	}

	void Start ()
	{
		forward = target.Forward;
		if ( ROSController.instance != null )
			ROSController.StartROS ( OnRosInit );
	}

	void LateUpdate ()
	{
		if ( setRotationFlag )
		{
			setRotationFlag = false;
			transform.rotation = targetRotation;
		}

		transform.position = target.Position - transform.forward * followDistance;
/*		if ( autoAlign )
		{
			Vector3 forward = Vector3.ProjectOnPlane ( target.Force, Vector3.up ).normalized;
			Vector3 localForward = Vector3.ProjectOnPlane ( transform.forward, Vector3.up ).normalized;
			
			transform.position = target.Position - forward * followDistance + Vector3.up * height;
			Quaternion q = Quaternion.FromToRotation ( localForward, forward );
			transform.rotation =  Quaternion.RotateTowards ( transform.rotation, q * transform.rotation, 360 * Time.deltaTime );
		} else
		{
			transform.position = target.Position - forward * followDistance + Vector3.up * height;
		}*/

		if ( Input.GetKeyDown ( KeyCode.F8 ) )
		{
			float dist = Random.Range ( 2f, 20f );
			new System.Threading.Thread ( () =>
			{
				SetFloat.Request req = new SetFloat.Request ();
				SetFloat.Response resp = new SetFloat.Response ();
				req.data = dist;

				if ( nh.serviceClient<SetFloat.Request, SetFloat.Response> ( "/quad_rotor/camera_distance" ).call ( req, ref resp ) )
					Debug.Log ( resp.success + " " + resp.newData );
				else
					Debug.Log ( "Failed" );
			} ).Start ();
		}

		if ( Input.GetKeyDown ( KeyCode.Alpha1 ) || Input.GetKeyDown ( KeyCode.Alpha2 ) || Input.GetKeyDown ( KeyCode.Alpha3 ) || Input.GetKeyDown ( KeyCode.Alpha4 ) || Input.GetKeyDown ( KeyCode.Alpha5 ) )
		{
			int pose = 0;
			if ( Input.GetKeyDown ( KeyCode.Alpha2 ) )
				pose = 1;
			if ( Input.GetKeyDown ( KeyCode.Alpha3 ) )
				pose = 2;
			if ( Input.GetKeyDown ( KeyCode.Alpha4 ) )
				pose = 3;
			if ( Input.GetKeyDown ( KeyCode.Alpha5 ) )
				pose = 4;
			new System.Threading.Thread ( () =>
			{
				SetInt.Request req = new SetInt.Request ();
				SetInt.Response resp = new SetInt.Response ();
				req.data = pose;

				if ( nh.serviceClient<SetInt.Request, SetInt.Response> ( "/quad_rotor/camera_pose_type" ).call ( req, ref resp ) )
					Debug.Log ( resp.success + " " + resp.newData );
				else
					Debug.Log ( "Failed" );
			} ).Start ();
		}
	}

	void ChangePoseType (CameraPoseType newType)
	{
		poseType = newType;

		switch ( poseType )
		{
		case CameraPoseType.XNorm:
			targetRotation = Quaternion.LookRotation ( Vector3.forward, Vector3.up );
			break;

		case CameraPoseType.YNorm:
			targetRotation = Quaternion.LookRotation ( -Vector3.right, Vector3.up );
			break;

		case CameraPoseType.ZNorm:
			targetRotation = Quaternion.LookRotation ( -Vector3.up, ( Vector3.forward - Vector3.right ).normalized );
			break;

		case CameraPoseType.Iso:
		case CameraPoseType.Free:
			targetRotation = Quaternion.LookRotation ( ( Vector3.forward - Vector3.right - Vector3.up ).normalized, ( Vector3.forward - Vector3.right + Vector3.up ).normalized );
			break;
		}

		setRotationFlag = true;
	}

	void OnRosInit ()
	{
		nh = ROS.GlobalNodeHandle;
		poseTypeSrv = nh.advertiseService<SetInt.Request, SetInt.Response> ( "/quad_rotor/camera_pose_type", SetCameraPoseType );
		distanceSrv = nh.advertiseService<SetFloat.Request, SetFloat.Response> ( "/quad_rotor/camera_distance", SetFollowDistance );
	}

	bool SetFollowDistance (SetFloat.Request req, ref SetFloat.Response resp)
	{
		followDistance = req.data;

		resp.newData = followDistance;
		resp.success = true;

		return true;
	}

	bool SetCameraPoseType (SetInt.Request req, ref SetInt.Response resp)
	{
		ChangePoseType ( (CameraPoseType) req.data );

		resp.newData = (int) poseType;
		resp.success = true;

		return true;
	}
}