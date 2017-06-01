using System;
using System.Collections.Generic;
using System.Text;

using Messages.actionlib_msgs;
using Messages.std_msgs;
using Messages;
using Uml.Robotics.Ros;
using Uml.Robotics.XmlRpc;
using Uml.Robotics.Ros.ActionLib.Interfaces;

namespace Uml.Robotics.Ros.ActionLib
{
    public class ServerGoalHandle<TGoal, TResult, TFeedback>
        where TGoal : InnerActionMessage, new()
        where TResult : InnerActionMessage, new()
        where TFeedback : InnerActionMessage, new()
    {
        public GoalStatus GoalStatus { get; private set; }
        public GoalID GoalId { get; private set; }
        public TGoal Goal { get; private set; }

        private string text;
        private IActionServer<TGoal, TResult, TFeedback> actionServer;
        public DateTime DestructionTime { get; set; }


        public ServerGoalHandle(IActionServer<TGoal, TResult, TFeedback> actionServer, GoalID goalId, GoalStatus goalStatus,
            TGoal goal)
        {
            this.actionServer = actionServer;
            GoalStatus = goalStatus;
            GoalId = goalId;
            GoalStatus.goal_id = goalId;

            if ((goalId.stamp == null) || (ROS.GetTime(goalId.stamp) == new DateTime(1970, 1, 1, 0, 0, 0)))
            {
                // If stamp is not initialized
                GoalStatus.goal_id.stamp = ROS.GetTime();
            }

            GoalStatus = goalStatus;
            this.Goal = goal;
        }


        public TFeedback CreateFeedback()
        {
            var feedback = new TFeedback();
            return feedback;
        }


        public TResult CreateResult()
        {
            var result = new TResult();
            return result;
        }


        public void SetAborted(TResult result, string text)
        {
            text = text ?? "";
            ROS.Debug()("actionlib", $"Setting status to aborted on goal, id: {GoalId.id}, stamp: {GoalId.stamp}");
            if ((GoalStatus.status == GoalStatus.PREEMPTING) || (GoalStatus.status == GoalStatus.ACTIVE))
            {
                SetGoalResult(GoalStatus.ABORTED, text, result);
            } else
            {
                ROS.Error()("actionlib", "To transition to an aborted state, the goal must be in a preempting or active state, " +
                    $"it is currently in state: {GoalStatus.status}");
            }
        }


        public void SetAccepted(string text)
        {
            text = text ?? "";
            ROS.Debug()("actionlib", $"Accepting goal, id: {GoalId.id}, stamp: {GoalId.stamp}");
            if (GoalStatus.status == GoalStatus.PENDING)
            {
                SetGoalStatus(GoalStatus.ACTIVE, text);
            }
            else if (GoalStatus.status == GoalStatus.RECALLING)
            {
                SetGoalStatus(GoalStatus.PREEMPTING, text);
            }
            else
            {
                ROS.Error()("actionlib", "To transition to an active state, the goal must be in a pending or recalling state, " +
                    $"it is currently in state: {GoalStatus.status}");
            }
        }


        public void SetCanceled(TResult result, string text)
        {
            text = text ?? "";
            ROS.Debug()($"Setting status to canceled on goal, id: {GoalId.id}, stamp: {GoalId.stamp}");
            if ((GoalStatus.status == GoalStatus.PENDING) || (GoalStatus.status == GoalStatus.RECALLING))
            {
                SetGoalResult(GoalStatus.RECALLED, text, result);
            } else if ((GoalStatus.status == GoalStatus.ACTIVE) || (GoalStatus.status == GoalStatus.PREEMPTING))
            {
                SetGoalResult(GoalStatus.PREEMPTED, text, result);
            } else
            {
                ROS.Error()("actionlib", "To transition to a cancelled state, the goal must be in a pending, recalling, active, " +
                    $"or preempting state, it is currently in state: {GoalStatus.status}");
            }
        }


        public bool SetCancelRequested()
        {
            ROS.Debug()("actionlib", $"Transisitoning to a cancel requested state on goal id: {GoalId.id}, stamp: {GoalId.stamp}");
            bool result = false;
            if (GoalStatus.status == GoalStatus.PENDING)
            {
                SetGoalStatus(GoalStatus.RECALLING, "RECALLING");
                result = true;
            }
            if (GoalStatus.status == GoalStatus.ACTIVE)
            {
                SetGoalStatus(GoalStatus.PREEMPTING, "PREEMPTING");
                result = true;
            }

            return result;
        }


        public void SetGoalStatus(byte goalStatus, string text)
        {
            this.GoalStatus.status = goalStatus;
            this.text = text ?? "";
            actionServer.PublishStatus();
        }


        public void SetGoalResult(byte goalStatus, string text, TResult result)
        {
            this.GoalStatus.status = goalStatus;
            this.text = text ?? "";
            actionServer.PublishResult(GoalStatus, result);
            DestructionTime = DateTime.UtcNow;
        }


        public void SetRejected(TResult result, string text)
        {
            text = text ?? "";
            ROS.Debug()("actionlib", $"Setting status to rejected on goal, id: {GoalId.id}, stamp: {GoalId.stamp}");
            if ((GoalStatus.status == GoalStatus.PENDING) || (GoalStatus.status == GoalStatus.RECALLING))
            {
                SetGoalResult(GoalStatus.REJECTED, text, result);
            } else
            {
                ROS.Error()("actionlib", "To transition to a rejected state, the goal must be in a pending or recalling state, " +
                    $"it is currently in state: {GoalStatus.status}");
            }
        }


        public void SetSucceded(TResult result, string text)
        {
            text = text ?? "";
            ROS.Debug()("actionlib", $"Setting status to succeeded on goal, id: {GoalId.id}, stamp: {GoalId.stamp}");
            if ((GoalStatus.status == GoalStatus.PREEMPTING) || (GoalStatus.status == GoalStatus.ACTIVE))
            {
                SetGoalResult(GoalStatus.SUCCEEDED, text, result);
            }
            else
            {
                ROS.Error()("actionlib", "To transition to a succeeded state, the goal must be in a preempting or active state, " +
                    $"it is currently in state: {GoalStatus.status}");
            }
        }


        public void PublishFeedback(TFeedback feedback)
        {
            actionServer.PublishFeedback(GoalStatus, feedback);
        }
    }
}
