using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;
using System.Security.AccessControl;

public class CommandServer : MonoBehaviour
{
	public RobotRemoteControl robotRemoteControl;
	public IRobotController robotController;
	public Camera frontFacingCamera;
	private SocketIOComponent _socket;

	void Start()
	{
		_socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
		_socket.On("open", OnOpen);
		_socket.On("steer", OnSteer);
		_socket.On("manual", onManual);
		_socket.On ( "fixed_turn", OnFixedTurn );
		_socket.On ( "pickup", OnPickup );
		robotController = robotRemoteControl.robot;
		frontFacingCamera = robotController.recordingCam;
	}

	void OnOpen(SocketIOEvent obj)
	{
//		Debug.Log("Connection Open");
		EmitTelemetry(obj);
	}

	// 
	void onManual(SocketIOEvent obj)
	{
		EmitTelemetry (obj);
	}

	void OnSteer(SocketIOEvent obj)
	{
//		Debug.Log ( "Steer" );
		JSONObject jsonObject = obj.data;
		robotRemoteControl.SteeringAngle = float.Parse(jsonObject.GetField("steering_angle").str);
		robotRemoteControl.ThrottleInput = float.Parse(jsonObject.GetField("throttle").str);
		if ( jsonObject.HasField ( "brake" ) )
			robotRemoteControl.BrakeInput = float.Parse ( jsonObject.GetField ( "brake" ).str );
		else
			robotRemoteControl.BrakeInput = 0;
//		robotRemoteControl.VerticalAngle = float.Parse ( jsonObject.GetField ( "vert_angle" ).str );
		EmitTelemetry(obj);
	}

	void OnFixedTurn(SocketIOEvent obj)
	{
		JSONObject json = obj.data;
		float angle = float.Parse ( json.GetField ( "angle" ).str );
		float time = 0;
		if ( json.HasField ( "time" ) )
			time = float.Parse ( json.GetField ( "time" ).str );
		robotRemoteControl.FixedTurn ( angle, time );
		EmitTelemetry ( obj );
	}

	void OnPickup (SocketIOEvent obj)
	{
		robotRemoteControl.PickupSample ();
		EmitTelemetry ( obj );
	}

	void EmitTelemetry(SocketIOEvent obj)
	{
//		Debug.Log ( "Emitting" );
		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			print("Attempting to Send...");
			// send only if it's not being manually driven
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
				_socket.Emit("telemetry", new JSONObject());
			}
			else {
				// Collect Data from the Car
				Dictionary<string, string> data = new Dictionary<string, string>();

				data["steering_angle"] = robotController.SteerAngle.ToString("N4");
//				data["vert_angle"] = robotController.VerticalAngle.ToString ("N4");
				data["throttle"] = robotController.ThrottleInput.ToString("N4");
				data["brake"] = robotController.BrakeInput.ToString ("N4");
				data["speed"] = robotController.Speed.ToString("N4");
				Vector3 pos = robotController.Position;
				data["position"] = pos.x.ToString ("N4") + "," + pos.z.ToString ("N4");
				data["orientation"] = robotController.Orientation.ToString ("N4");
				data["fixed_turn"] = robotController.IsTurningInPlace ? "1" : "0";
				data["near_sample"] = robotController.IsNearObjective ? "1" : "0";
				data["picking_up"] = robotController.IsPickingUpSample ? "1" : "0";
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(frontFacingCamera));
//				Debug.Log ("sangle " + data["steering_angle"] + " vert " + data["vert_angle"] + " throt " + data["throttle"] + " speed " + data["speed"] + " image " + data["image"]);
				_socket.Emit("telemetry", new JSONObject(data));
			}
		});

		//    UnityMainThreadDispatcher.Instance().Enqueue(() =>
		//    {
		//      	
		//      
		//
		//		// send only if it's not being manually driven
		//		if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
		//			_socket.Emit("telemetry", new JSONObject());
		//		}
		//		else {
		//			// Collect Data from the Car
		//			Dictionary<string, string> data = new Dictionary<string, string>();
		//			data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
		//			data["throttle"] = _carController.AccelInput.ToString("N4");
		//			data["speed"] = _carController.CurrentSpeed.ToString("N4");
		//			data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
		//			_socket.Emit("telemetry", new JSONObject(data));
		//		}
		//      
		////      
		//    });
	}
}