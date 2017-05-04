using System;
using actionlib;
using hector_uav_msgs;

namespace hector_quadrotor_actions
{
	public class PoseMediator : AActionMediator
	{
		public static Type Action { get { return PoseAction; } }
		public static Type ActionGoal { get { return PoseActionGoal; } }
		public static Type Goal { get { return PoseGoal; } }
		public static Type ActionResult { get { return PoseActionResult; } }
		public static Type Result { get { return PoseResult; } }
		public static Type ActionFeedback { get { return PoseActionFeedback; } }
		public static Type Feedback { get { return PoseFeedback; } }
	}
}