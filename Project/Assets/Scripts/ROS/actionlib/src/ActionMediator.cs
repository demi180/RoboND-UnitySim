using System;
namespace actionlib
{
	public abstract class AActionMediator
	{
		public static Type Action { get; }
		public static Type ActionGoal { get; }
		public static Type Goal { get; }
		public static Type ActionResult { get; }
		public static Type Result { get; }
		public static Type ActionFeedback { get; }
		public static Type Feedback { get; }
	}
}