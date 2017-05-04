using Messages;

namespace Messages
{

	public enum MsgTypes
	{
		Unknown,
		wpf_msgs__Point2,
		wpf_msgs__Waypoints,
		wpf_msgs__WindowState,
		whosonfirst__Dibs,
		uvc_camera__camera_sliders,
		trust_msgs__InterfaceCommand,
		trust_msgs__RunDescription,
		trajectory_msgs__JointTrajectory,
		trajectory_msgs__JointTrajectoryPoint,
		trajectory_msgs__MultiDOFJointTrajectory,
		trajectory_msgs__MultiDOFJointTrajectoryPoint,
		theora_image_transport__Packet,
		tf2_msgs__LookupTransformAction,
		tf2_msgs__LookupTransformActionFeedback,
		tf2_msgs__LookupTransformActionGoal,
		tf2_msgs__LookupTransformActionResult,
		tf2_msgs__LookupTransformFeedback,
		tf2_msgs__LookupTransformGoal,
		tf2_msgs__LookupTransformResult,
		tf2_msgs__TF2Error,
		tf2_msgs__TFMessage,
		tf__tfMessage,
		std_msgs__Bool,
		std_msgs__Byte,
		std_msgs__ByteMultiArray,
		std_msgs__Char,
		std_msgs__ColorRGBA,
		std_msgs__Duration,
		std_msgs__Empty,
		std_msgs__Float32,
		std_msgs__Float32MultiArray,
		std_msgs__Float64,
		std_msgs__Float64MultiArray,
		std_msgs__Header,
		std_msgs__Int16,
		std_msgs__Int16MultiArray,
		std_msgs__Int32,
		std_msgs__Int32MultiArray,
		std_msgs__Int64,
		std_msgs__Int64MultiArray,
		std_msgs__Int8,
		std_msgs__Int8MultiArray,
		std_msgs__MultiArrayDimension,
		std_msgs__MultiArrayLayout,
		std_msgs__String,
		std_msgs__Time,
		std_msgs__UInt16,
		std_msgs__UInt16MultiArray,
		std_msgs__UInt32,
		std_msgs__UInt32MultiArray,
		std_msgs__UInt64,
		std_msgs__UInt64MultiArray,
		std_msgs__UInt8,
		std_msgs__UInt8MultiArray,
		shape_msgs__Mesh,
		shape_msgs__MeshTriangle,
		shape_msgs__Plane,
		shape_msgs__SolidPrimitive,
		sensor_msgs__BatteryState,
		sensor_msgs__CameraInfo,
		sensor_msgs__ChannelFloat32,
		sensor_msgs__CompressedImage,
		sensor_msgs__FluidPressure,
		sensor_msgs__Illuminance,
		sensor_msgs__Image,
		sensor_msgs__Imu,
		sensor_msgs__JointState,
		sensor_msgs__Joy,
		sensor_msgs__JoyFeedback,
		sensor_msgs__JoyFeedbackArray,
		sensor_msgs__LaserEcho,
		sensor_msgs__LaserScan,
		sensor_msgs__MagneticField,
		sensor_msgs__MultiDOFJointState,
		sensor_msgs__MultiEchoLaserScan,
		sensor_msgs__NavSatFix,
		sensor_msgs__NavSatStatus,
		sensor_msgs__PointCloud,
		sensor_msgs__PointCloud2,
		sensor_msgs__PointField,
		sensor_msgs__Range,
		sensor_msgs__RegionOfInterest,
		sensor_msgs__RelativeHumidity,
		sensor_msgs__Temperature,
		sensor_msgs__TimeReference,
		sample_acquisition__ArmMovement,
		sample_acquisition__ArmStatus,
		rosgraph_msgs__Clock,
		rosgraph_msgs__Log,
		roscsharp__Logger,
		rock_publisher__imgData,
		rock_publisher__imgDataArray,
		rock_publisher__recalibrateMsg,
		pcl_msgs__ModelCoefficients,
		pcl_msgs__PointIndices,
		pcl_msgs__PolygonMesh,
		pcl_msgs__Vertices,
		octomap_msgs__Octomap,
		octomap_msgs__OctomapWithPose,
		object_recognition_msgs__ObjectInformation,
		object_recognition_msgs__ObjectRecognitionAction,
		object_recognition_msgs__ObjectRecognitionActionFeedback,
		object_recognition_msgs__ObjectRecognitionActionGoal,
		object_recognition_msgs__ObjectRecognitionActionResult,
		object_recognition_msgs__ObjectRecognitionFeedback,
		object_recognition_msgs__ObjectRecognitionGoal,
		object_recognition_msgs__ObjectRecognitionResult,
		object_recognition_msgs__ObjectType,
		object_recognition_msgs__RecognizedObject,
		object_recognition_msgs__RecognizedObjectArray,
		object_recognition_msgs__Table,
		object_recognition_msgs__TableArray,
		nav_msgs__GetMapAction,
		nav_msgs__GetMapActionFeedback,
		nav_msgs__GetMapActionGoal,
		nav_msgs__GetMapActionResult,
		nav_msgs__GetMapFeedback,
		nav_msgs__GetMapGoal,
		nav_msgs__GetMapResult,
		nav_msgs__GridCells,
		nav_msgs__MapMetaData,
		nav_msgs__OccupancyGrid,
		nav_msgs__Odometry,
		nav_msgs__Path,
		move_base_msgs__MoveBaseAction,
		move_base_msgs__MoveBaseActionFeedback,
		move_base_msgs__MoveBaseActionGoal,
		move_base_msgs__MoveBaseActionResult,
		move_base_msgs__MoveBaseFeedback,
		move_base_msgs__MoveBaseGoal,
		move_base_msgs__MoveBaseResult,
		map_msgs__OccupancyGridUpdate,
		map_msgs__PointCloud2Update,
		map_msgs__ProjectedMap,
		map_msgs__ProjectedMapInfo,
		humanoid_nav_msgs__ExecFootstepsAction,
		humanoid_nav_msgs__ExecFootstepsActionFeedback,
		humanoid_nav_msgs__ExecFootstepsActionGoal,
		humanoid_nav_msgs__ExecFootstepsActionResult,
		humanoid_nav_msgs__ExecFootstepsFeedback,
		humanoid_nav_msgs__ExecFootstepsGoal,
		humanoid_nav_msgs__ExecFootstepsResult,
		humanoid_nav_msgs__StepTarget,
		histogram_msgs__histogramsnapshot,
		histogram_msgs__MCLSnapshot,
		geometry_msgs__Point,
		geometry_msgs__Point32,
		geometry_msgs__PointStamped,
		geometry_msgs__Polygon,
		geometry_msgs__PolygonStamped,
		geometry_msgs__Pose,
		geometry_msgs__Pose2D,
		geometry_msgs__PoseArray,
		geometry_msgs__PoseStamped,
		geometry_msgs__PoseWithCovariance,
		geometry_msgs__PoseWithCovarianceStamped,
		geometry_msgs__Quaternion,
		geometry_msgs__QuaternionStamped,
		geometry_msgs__Transform,
		geometry_msgs__TransformStamped,
		geometry_msgs__Twist,
		geometry_msgs__TwistStamped,
		geometry_msgs__TwistWithCovariance,
		geometry_msgs__TwistWithCovarianceStamped,
		geometry_msgs__Vector3,
		geometry_msgs__Vector3Stamped,
		geometry_msgs__Wrench,
		geometry_msgs__WrenchStamped,
		gazebo_msgs__ContactsState,
		gazebo_msgs__ContactState,
		gazebo_msgs__LinkState,
		gazebo_msgs__LinkStates,
		gazebo_msgs__ModelState,
		gazebo_msgs__ModelStates,
		gazebo_msgs__ODEJointProperties,
		gazebo_msgs__ODEPhysics,
		gazebo_msgs__WorldState,
		experiment__SimonSays,
		dynamixel_msgs__JointState,
		dynamic_reconfigure__BoolParameter,
		dynamic_reconfigure__Config,
		dynamic_reconfigure__ConfigDescription,
		dynamic_reconfigure__DoubleParameter,
		dynamic_reconfigure__Group,
		dynamic_reconfigure__GroupState,
		dynamic_reconfigure__IntParameter,
		dynamic_reconfigure__ParamDescription,
		dynamic_reconfigure__SensorLevels,
		dynamic_reconfigure__StrParameter,
		custom_msgs__arrayofdeez,
		custom_msgs__arraytest,
		custom_msgs__cgeo,
		custom_msgs__ptz,
		custom_msgs__robotMortality,
		custom_msgs__servosPos,
		custom_msgs__simpleintarray,
		control_msgs__FollowJointTrajectoryAction,
		control_msgs__FollowJointTrajectoryActionFeedback,
		control_msgs__FollowJointTrajectoryActionGoal,
		control_msgs__FollowJointTrajectoryActionResult,
		control_msgs__FollowJointTrajectoryFeedback,
		control_msgs__FollowJointTrajectoryGoal,
		control_msgs__FollowJointTrajectoryResult,
		control_msgs__GripperCommand,
		control_msgs__GripperCommandAction,
		control_msgs__GripperCommandActionFeedback,
		control_msgs__GripperCommandActionGoal,
		control_msgs__GripperCommandActionResult,
		control_msgs__GripperCommandFeedback,
		control_msgs__GripperCommandGoal,
		control_msgs__GripperCommandResult,
		control_msgs__JointControllerState,
		control_msgs__JointTolerance,
		control_msgs__JointTrajectoryAction,
		control_msgs__JointTrajectoryActionFeedback,
		control_msgs__JointTrajectoryActionGoal,
		control_msgs__JointTrajectoryActionResult,
		control_msgs__JointTrajectoryControllerState,
		control_msgs__JointTrajectoryFeedback,
		control_msgs__JointTrajectoryGoal,
		control_msgs__JointTrajectoryResult,
		control_msgs__PidState,
		control_msgs__PointHeadAction,
		control_msgs__PointHeadActionFeedback,
		control_msgs__PointHeadActionGoal,
		control_msgs__PointHeadActionResult,
		control_msgs__PointHeadFeedback,
		control_msgs__PointHeadGoal,
		control_msgs__PointHeadResult,
		control_msgs__SingleJointPositionAction,
		control_msgs__SingleJointPositionActionFeedback,
		control_msgs__SingleJointPositionActionGoal,
		control_msgs__SingleJointPositionActionResult,
		control_msgs__SingleJointPositionFeedback,
		control_msgs__SingleJointPositionGoal,
		control_msgs__SingleJointPositionResult,
		baxter_core_msgs__AnalogIOState,
		baxter_core_msgs__AnalogIOStates,
		baxter_core_msgs__AnalogOutputCommand,
		baxter_core_msgs__AssemblyState,
		baxter_core_msgs__AssemblyStates,
		baxter_core_msgs__CameraControl,
		baxter_core_msgs__CameraSettings,
		baxter_core_msgs__CollisionAvoidanceState,
		baxter_core_msgs__CollisionDetectionState,
		baxter_core_msgs__DigitalIOState,
		baxter_core_msgs__DigitalIOStates,
		baxter_core_msgs__DigitalOutputCommand,
		baxter_core_msgs__EndEffectorCommand,
		baxter_core_msgs__EndEffectorProperties,
		baxter_core_msgs__EndEffectorState,
		baxter_core_msgs__EndpointState,
		baxter_core_msgs__EndpointStates,
		baxter_core_msgs__HeadPanCommand,
		baxter_core_msgs__HeadState,
		baxter_core_msgs__ITBState,
		baxter_core_msgs__ITBStates,
		baxter_core_msgs__JointCommand,
		baxter_core_msgs__NavigatorState,
		baxter_core_msgs__NavigatorStates,
		baxter_core_msgs__RobustControllerStatus,
		baxter_core_msgs__SEAJointState,
		actionlib_msgs__GoalID,
		actionlib_msgs__GoalStatus,
		actionlib_msgs__GoalStatusArray,
		topic_tools__DemuxAdd__Request,
		topic_tools__DemuxAdd__Response,
		topic_tools__DemuxDelete__Request,
		topic_tools__DemuxDelete__Response,
		topic_tools__DemuxList__Request,
		topic_tools__DemuxList__Response,
		topic_tools__DemuxSelect__Request,
		topic_tools__DemuxSelect__Response,
		topic_tools__MuxAdd__Request,
		topic_tools__MuxAdd__Response,
		topic_tools__MuxDelete__Request,
		topic_tools__MuxDelete__Response,
		topic_tools__MuxList__Request,
		topic_tools__MuxList__Response,
		topic_tools__MuxSelect__Request,
		topic_tools__MuxSelect__Response,
		tf2_msgs__FrameGraph__Request,
		tf2_msgs__FrameGraph__Response,
		tf__FrameGraph__Request,
		tf__FrameGraph__Response,
		std_srvs__Empty__Request,
		std_srvs__Empty__Response,
		std_srvs__SetBool__Request,
		std_srvs__SetBool__Response,
		std_srvs__Trigger__Request,
		std_srvs__Trigger__Response,
		ServiceTest__AddTwoInts__Request,
		ServiceTest__AddTwoInts__Response,
		sensor_msgs__SetCameraInfo__Request,
		sensor_msgs__SetCameraInfo__Response,
		roscsharp__GetLoggers__Request,
		roscsharp__GetLoggers__Response,
		roscsharp__SetLoggerLevel__Request,
		roscsharp__SetLoggerLevel__Response,
		roscpp_tutorials__TwoInts__Request,
		roscpp_tutorials__TwoInts__Response,
		octomap_msgs__BoundingBoxQuery__Request,
		octomap_msgs__BoundingBoxQuery__Response,
		octomap_msgs__GetOctomap__Request,
		octomap_msgs__GetOctomap__Response,
		object_recognition_msgs__GetObjectInformation__Request,
		object_recognition_msgs__GetObjectInformation__Response,
		nav_msgs__GetMap__Request,
		nav_msgs__GetMap__Response,
		nav_msgs__GetPlan__Request,
		nav_msgs__GetPlan__Response,
		nav_msgs__SetMap__Request,
		nav_msgs__SetMap__Response,
		map_msgs__GetMapROI__Request,
		map_msgs__GetMapROI__Response,
		map_msgs__GetPointMap__Request,
		map_msgs__GetPointMap__Response,
		map_msgs__GetPointMapROI__Request,
		map_msgs__GetPointMapROI__Response,
		map_msgs__ProjectedMapsInfo__Request,
		map_msgs__ProjectedMapsInfo__Response,
		map_msgs__SaveMap__Request,
		map_msgs__SaveMap__Response,
		map_msgs__SetMapProjections__Request,
		map_msgs__SetMapProjections__Response,
		humanoid_nav_msgs__ClipFootstep__Request,
		humanoid_nav_msgs__ClipFootstep__Response,
		humanoid_nav_msgs__PlanFootsteps__Request,
		humanoid_nav_msgs__PlanFootsteps__Response,
		humanoid_nav_msgs__PlanFootstepsBetweenFeet__Request,
		humanoid_nav_msgs__PlanFootstepsBetweenFeet__Response,
		humanoid_nav_msgs__StepTargetService__Request,
		humanoid_nav_msgs__StepTargetService__Response,
		gazebo_msgs__ApplyBodyWrench__Request,
		gazebo_msgs__ApplyBodyWrench__Response,
		gazebo_msgs__ApplyJointEffort__Request,
		gazebo_msgs__ApplyJointEffort__Response,
		gazebo_msgs__BodyRequest__Request,
		gazebo_msgs__BodyRequest__Response,
		gazebo_msgs__DeleteModel__Request,
		gazebo_msgs__DeleteModel__Response,
		gazebo_msgs__GetJointProperties__Request,
		gazebo_msgs__GetJointProperties__Response,
		gazebo_msgs__GetLinkProperties__Request,
		gazebo_msgs__GetLinkProperties__Response,
		gazebo_msgs__GetLinkState__Request,
		gazebo_msgs__GetLinkState__Response,
		gazebo_msgs__GetModelProperties__Request,
		gazebo_msgs__GetModelProperties__Response,
		gazebo_msgs__GetModelState__Request,
		gazebo_msgs__GetModelState__Response,
		gazebo_msgs__GetPhysicsProperties__Request,
		gazebo_msgs__GetPhysicsProperties__Response,
		gazebo_msgs__GetWorldProperties__Request,
		gazebo_msgs__GetWorldProperties__Response,
		gazebo_msgs__JointRequest__Request,
		gazebo_msgs__JointRequest__Response,
		gazebo_msgs__SetJointProperties__Request,
		gazebo_msgs__SetJointProperties__Response,
		gazebo_msgs__SetJointTrajectory__Request,
		gazebo_msgs__SetJointTrajectory__Response,
		gazebo_msgs__SetLinkProperties__Request,
		gazebo_msgs__SetLinkProperties__Response,
		gazebo_msgs__SetLinkState__Request,
		gazebo_msgs__SetLinkState__Response,
		gazebo_msgs__SetModelConfiguration__Request,
		gazebo_msgs__SetModelConfiguration__Response,
		gazebo_msgs__SetModelState__Request,
		gazebo_msgs__SetModelState__Response,
		gazebo_msgs__SetPhysicsProperties__Request,
		gazebo_msgs__SetPhysicsProperties__Response,
		gazebo_msgs__SpawnModel__Request,
		gazebo_msgs__SpawnModel__Response,
		dynamic_reconfigure__Reconfigure__Request,
		dynamic_reconfigure__Reconfigure__Response,
		control_msgs__QueryCalibrationState__Request,
		control_msgs__QueryCalibrationState__Response,
		control_msgs__QueryTrajectoryState__Request,
		control_msgs__QueryTrajectoryState__Response,
		baxter_core_msgs__CloseCamera__Request,
		baxter_core_msgs__CloseCamera__Response,
		baxter_core_msgs__ListCameras__Request,
		baxter_core_msgs__ListCameras__Response,
		baxter_core_msgs__OpenCamera__Request,
		baxter_core_msgs__OpenCamera__Response,
		baxter_core_msgs__SolvePositionIK__Request,
		baxter_core_msgs__SolvePositionIK__Response,

		// hector stuff
		hector_uav_msgs__Altimeter = 1000,
		hector_uav_msgs__Attitudecommand,
		hector_uav_msgs__Compass,
		hector_uav_msgs__ControllerState,
		hector_uav_msgs__EnableMotors__Request,
		hector_uav_msgs__EnableMotors__Response,
		hector_uav_msgs__RC,
		hector_uav_msgs__HeadingCommand,
		hector_uav_msgs__HeightCommand,
		hector_uav_msgs__LandingAction,
		hector_uav_msgs__LandingActionGoal,
		hector_uav_msgs__LandingActionResult,
		hector_uav_msgs__LandingActionFeedback,
		hector_uav_msgs__LandingGoal,
		hector_uav_msgs__LandingResult,
		hector_uav_msgs__LandingFeedback,
		hector_uav_msgs__MotorCommand,
		hector_uav_msgs__MotorPWM,
		hector_uav_msgs__MotorStatus,
		hector_uav_msgs__PoseGoal,
		hector_uav_msgs__PoseResult,
		hector_uav_msgs__PoseFeedback,
		hector_uav_msgs__PoseAction,
		hector_uav_msgs__PoseActionGoal,
		hector_uav_msgs__PoseActionResult,
		hector_uav_msgs__PoseActionFeedback,
		hector_uav_msgs__PositionXYCommand,
		hector_uav_msgs__RawImu,
		hector_uav_msgs__RawMagnetic,
		hector_uav_msgs__RawRC,
		hector_uav_msgs__RuddersCommand,
		hector_uav_msgs__ServoCommand,
		hector_uav_msgs__Supply,
		hector_uav_msgs__TakeoffGoal,
		hector_uav_msgs__TakeoffResult,
		hector_uav_msgs__TakeoffFeedback,
		hector_uav_msgs__TakeoffAction,
		hector_uav_msgs__TakeoffActionGoal,
		hector_uav_msgs__TakeoffActionResult,
		hector_uav_msgs__TakeoffActionFeedback,
		hector_uav_msgs__ThrustCommand,
		hector_uav_msgs__VelocityXYCommand,
		hector_uav_msgs__VelocityZCommand,
		hector_uav_msgs__YawRateCommand,

		// test
		MessageEvent
	}

	public enum SrvTypes
	{
		Unknown,
		topic_tools__DemuxAdd,
		topic_tools__DemuxDelete,
		topic_tools__DemuxList,
		topic_tools__DemuxSelect,
		topic_tools__MuxAdd,
		topic_tools__MuxDelete,
		topic_tools__MuxList,
		topic_tools__MuxSelect,
		tf2_msgs__FrameGraph,
		tf__FrameGraph,
		std_srvs__Empty,
		std_srvs__SetBool,
		std_srvs__Trigger,
		ServiceTest__AddTwoInts,
		sensor_msgs__SetCameraInfo,
		roscsharp__GetLoggers,
		roscsharp__SetLoggerLevel,
		roscpp_tutorials__TwoInts,
		octomap_msgs__BoundingBoxQuery,
		octomap_msgs__GetOctomap,
		object_recognition_msgs__GetObjectInformation,
		nav_msgs__GetMap,
		nav_msgs__GetPlan,
		nav_msgs__SetMap,
		map_msgs__GetMapROI,
		map_msgs__GetPointMap,
		map_msgs__GetPointMapROI,
		map_msgs__ProjectedMapsInfo,
		map_msgs__SaveMap,
		map_msgs__SetMapProjections,
		humanoid_nav_msgs__ClipFootstep,
		humanoid_nav_msgs__PlanFootsteps,
		humanoid_nav_msgs__PlanFootstepsBetweenFeet,
		humanoid_nav_msgs__StepTargetService,
		gazebo_msgs__ApplyBodyWrench,
		gazebo_msgs__ApplyJointEffort,
		gazebo_msgs__BodyRequest,
		gazebo_msgs__DeleteModel,
		gazebo_msgs__GetJointProperties,
		gazebo_msgs__GetLinkProperties,
		gazebo_msgs__GetLinkState,
		gazebo_msgs__GetModelProperties,
		gazebo_msgs__GetModelState,
		gazebo_msgs__GetPhysicsProperties,
		gazebo_msgs__GetWorldProperties,
		gazebo_msgs__JointRequest,
		gazebo_msgs__SetJointProperties,
		gazebo_msgs__SetJointTrajectory,
		gazebo_msgs__SetLinkProperties,
		gazebo_msgs__SetLinkState,
		gazebo_msgs__SetModelConfiguration,
		gazebo_msgs__SetModelState,
		gazebo_msgs__SetPhysicsProperties,
		gazebo_msgs__SpawnModel,
		dynamic_reconfigure__Reconfigure,
		control_msgs__QueryCalibrationState,
		control_msgs__QueryTrajectoryState,
		baxter_core_msgs__CloseCamera,
		baxter_core_msgs__ListCameras,
		baxter_core_msgs__OpenCamera,
		baxter_core_msgs__SolvePositionIK,

		// hector stuff
		hector_uav_msgs__EnableMotors = 1000,
	}

}