using System;
using System.Collections.Generic;
using System.Text;

using Messages;
using Messages.actionlib_msgs;

namespace Uml.Robotics.Ros.ActionLib
{
    public interface IActionClient<TGoal, TResult, TFeedback>
        where TGoal : InnerActionMessage, new()
        where TResult : InnerActionMessage, new()
        where TFeedback : InnerActionMessage, new()
    {
        Publisher<GoalActionMessage<TGoal>> GoalPublisher { get; }
        Publisher<GoalID> CancelPublisher { get; }
        void TransitionToState(ClientGoalHandle<TGoal, TResult, TFeedback> goalHandle, CommunicationState state);
    }
}
