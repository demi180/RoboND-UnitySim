using System;
using System.Collections.Generic;
using System.Text;

using Messages;
using Messages.actionlib_msgs;

namespace Uml.Robotics.Ros.ActionLib.Interfaces
{
    public interface IActionServer<TGoal, TResult, TFeedback>
        where TGoal : InnerActionMessage, new()
        where TResult : InnerActionMessage, new()
        where TFeedback : InnerActionMessage, new()
    {
        void PublishResult(GoalStatus goalStatus, TResult result);
        void PublishFeedback(GoalStatus goalStatus, TFeedback feedback);
        void PublishStatus();
    }
}
