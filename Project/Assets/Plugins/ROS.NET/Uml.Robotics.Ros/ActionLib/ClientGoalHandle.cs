using System;
using System.Collections.Generic;
using System.Text;

using Messages;
using Messages.actionlib_msgs;

namespace Uml.Robotics.Ros.ActionLib
{
    public class ClientGoalHandle<TGoal, TResult, TFeedback>
        where TGoal : InnerActionMessage, new()
        where TResult : InnerActionMessage, new()
        where TFeedback : InnerActionMessage, new()
    {
        public string Id { get; set; }
        public GoalActionMessage<TGoal> Goal { get; set; }
        public CommunicationState State { get; set; }
        public Action<ClientGoalHandle<TGoal, TResult, TFeedback>> OnTransitionCallback { get; set; }
        public Action<ClientGoalHandle<TGoal, TResult, TFeedback>, FeedbackActionMessage<TFeedback>> OnFeedbackCallback { get; set; }
        public bool Active { get; set; }
        public GoalStatus LatestGoalStatus { get; set; }
        public ResultActionMessage<TResult> LatestResultAction { get; set; }
        public TResult Result
        {
            get
            {
                if (!Active)
                {
                    ROS.Error()("actionlib", "Trying to getResult on an inactive ClientGoalHandle.");
                }
                if (LatestResultAction != null)
                {
                    return LatestResultAction.Result;
                }

                return null;
            }
        }

        private IActionClient<TGoal, TResult, TFeedback> actionClient;


        public ClientGoalHandle(IActionClient<TGoal, TResult, TFeedback> actionClient, GoalActionMessage<TGoal> goalAction,
            Action<ClientGoalHandle<TGoal, TResult, TFeedback>> OnTransitionCallback,
            Action<ClientGoalHandle<TGoal, TResult, TFeedback>, FeedbackActionMessage<TFeedback>> OnFeedbackCallback)
        {
            this.actionClient = actionClient;
            Id = goalAction.GoalId.id;
            Goal = goalAction;
            State = CommunicationState.WAITING_FOR_GOAL_ACK;
            this.OnTransitionCallback = OnTransitionCallback;
            this.OnFeedbackCallback = OnFeedbackCallback;
            Active = true;
        }


        public void Cancel()
        {
            if (!Active)
            {
                ROS.Error()("actionlib", "Trying to cancel() on an inactive goal handle.");
            }

            if ((State == CommunicationState.WAITING_FOR_RESULT ||
                State == CommunicationState.RECALLING ||
                State == CommunicationState.PREEMPTING ||
                State == CommunicationState.DONE))
            {
                ROS.Debug()("actionlib", $"Got a cancel() request while in state {State}, so ignoring it");
                return;
            }
            else if (!(State == CommunicationState.WAITING_FOR_GOAL_ACK ||
              State == CommunicationState.PENDING ||
              State == CommunicationState.ACTIVE ||
              State == CommunicationState.WAITING_FOR_CANCEL_ACK))
            {
                ROS.Debug()("actionlib", $"BUG: Unhandled CommState: {State}");
            }

            var cancelMessage = new GoalID();
            cancelMessage.id = Id;
            actionClient.CancelPublisher.publish(cancelMessage);
            actionClient.TransitionToState(this, State);
        }


        public void Resend()
        {
            if (!Active)
            {
                ROS.Error()("actionlib", "Trying to resend() on an inactive goal handle.");
            }
            actionClient.GoalPublisher.publish(Goal);
        }


        public void Reset()
        {
            OnTransitionCallback = null;
            OnFeedbackCallback = null;
            Active = false;
        }
    }
}
