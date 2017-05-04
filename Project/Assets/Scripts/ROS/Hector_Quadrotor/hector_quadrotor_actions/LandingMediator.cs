using System;
using actionlib;
using hector_uav_msgs;

namespace hector_quadrotor_actions
{
	public class LandingMediator : AActionMediator
	{
		public static Type Action { get { return LandingAction; } }
		public static Type ActionGoal { get { return LandingActionGoal; } }
		public static Type Goal { get { return LandingGoal; } }
		public static Type ActionResult { get { return LandingActionResult; } }
		public static Type Result { get { return LandingResult; } }
		public static Type ActionFeedback { get { return LandingActionFeedback; } }
		public static Type Feedback { get { return LandingFeedback; } }
	}
}