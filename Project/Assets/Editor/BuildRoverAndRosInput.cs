using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildRoverAndRosInput : MonoBehaviour
{
/*	[MenuItem ("Build/Build Rover and ROS Input", false, 10)]
	static void BuildRoverAndInput ()
	{
		string buildOutput = "Builds/ROS/";
		string fileName = "proto3-ros";
		if ( EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 )
			fileName += ".exe";
		else
			fileName += ".app";
		string[] levels = new string[1] { "Assets/Scenes/proto3.unity" };
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None );

		fileName = "rosRemoteInput";
		if ( EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 )
			fileName += ".exe";
		else
			fileName += ".app";
		levels [ 0 ] = "Assets/Scenes/ros_rover_control.unity";
			BuildPipeline.BuildPlayer ( levels, buildOutput + fileName, EditorUserBuildSettings.activeBuildTarget, BuildOptions.ShowBuiltPlayer );
	}*/

	[MenuItem ("Build/Build Quad_Indoor", false, 10)]
	static void BuildQuadIndoor ()
	{
		string buildOutput = "Builds/ROS/";
		string fileName = "Indoor";
		string[] levels = new string[1] { "Assets/Scenes/quad_indoor.unity" };

		// build windows
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_win.exe", BuildTarget.StandaloneWindows64, BuildOptions.None );

		// build mac
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_osx.app", BuildTarget.StandaloneOSXIntel64, BuildOptions.None );

		// build linux
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_lin.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.ShowBuiltPlayer );
	}

	[MenuItem ("Build/Build Uda-City", false, 10)]
	static void BuildUdaCity ()
	{
		string buildOutput = "Builds/ROS/";
		string fileName = "Outdoor";
		string[] levels = new string[1] { "Assets/Scenes/proto4.unity" };

		// build windows
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_win.exe", BuildTarget.StandaloneWindows64, BuildOptions.None );

		// build mac
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_osx.app", BuildTarget.StandaloneOSXIntel64, BuildOptions.None );

		// build linux
		BuildPipeline.BuildPlayer ( levels, buildOutput + fileName + "_lin.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.ShowBuiltPlayer );
	}
}