using System;
using actionlib;
using hector_uav_msgs;

namespace hector_quadrotor_actions
{
	public class TakeoffMediator : AActionMediator
	{
		public static Type Action { get { return TakeoffAction; } }
		public static Type ActionGoal { get { return TakeoffActionGoal; } }
		public static Type Goal { get { return TakeoffGoal; } }
		public static Type ActionResult { get { return TakeoffActionResult; } }
		public static Type Result { get { return TakeoffResult; } }
		public static Type ActionFeedback { get { return TakeoffActionFeedback; } }
		public static Type Feedback { get { return TakeoffFeedback; } }
	}
}