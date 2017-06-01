using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YAMLParser;

namespace FauxMessages
{
    public class ActionFile
    {
        public string Name { get; private set; }
        public MsgFile GoalMessage { get; private set; }
        public MsgFile ResultMessage { get; private set; }
        public MsgFile FeedbackMessage { get; private set; }
        public MsgFile GoalActionMessage { get; private set; }
        public MsgFile ResultActionMessage { get; private set; }
        public MsgFile FeedbackActionMessage { get; private set; }

        private string fileNamespace = "Messages";
        private List<SingleType> stuff = new List<SingleType>();
        private string className;
        private MsgFileLocation MsgFileLocation;
        private List<string> linesOfActionFile = new List<string>();


        public ActionFile(MsgFileLocation filename)
        {
            // Read in action file
            string[] lines = File.ReadAllLines(filename.Path);
            InitializeActionFile(filename, lines);
        }


        public ActionFile(MsgFileLocation filename, string[] lines)
        {
            InitializeActionFile(filename, lines);
        }


        public void ParseAndResolveTypes()
        {
            GoalMessage.ParseAndResolveTypes();
            GoalActionMessage.ParseAndResolveTypes();
            ResultMessage.ParseAndResolveTypes();
            ResultActionMessage.ParseAndResolveTypes();
            FeedbackMessage.ParseAndResolveTypes();
            FeedbackActionMessage.ParseAndResolveTypes();
        }


        public void Write(string outdir)
        {
            string[] chunks = Name.Split('.');
            for (int i = 0; i < chunks.Length - 1; i++)
                outdir = Path.Combine(outdir, chunks[i]);
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);
            string contents = GenerateActionMessages();
            if (contents != null)
                File.WriteAllText(Path.Combine(outdir, MsgFileLocation.basename + "ActionMessages.cs"),
                    contents.Replace("FauxMessages", "Messages")
                );
        }


        /// <summary>
        /// Loads the template for a single message and replaces all the $placeholders with appropriate content
        /// </summary>
        public string GenerateMessageFromTemplate(string template, MsgFile message, MsgFile childMessage)
        {
            var properties = message.GenerateProperties();
            template = template.Replace("$CLASS_NAME", message.classname);
            template = template.Replace("$$PROPERTIES", properties);
            template = template.Replace("$ISMETA", message.meta.ToString().ToLower());
            template = template.Replace("$MSGTYPE", fileNamespace.Replace("Messages.", "") + "/" + message.classname);
            template = template.Replace("$MESSAGEDEFINITION", "@\"" + message.Definition + "\"");
            template = template.Replace("$HASHEADER", message.HasHeader.ToString().ToLower());
            template = template.Replace("$NULLCONSTBODY", "");
            template = template.Replace("$EXTRACONSTRUCTOR", "");

            template = template.Replace("$MD5SUM", MD5.Sum(message));

            // Set the base class of the message
            var actionClasses = new List<string> { "InnerActionMessage",
                "GoalActionMessage<$ACTION_GENERIC>",
                "ResultActionMessage<$ACTION_GENERIC>",
                "FeedbackActionMessage<$ACTION_GENERIC>"
            };
            var actionClass = actionClasses[(int)message.ActionMessageType];
            actionClass = actionClass.Replace("$ACTION_GENERIC", childMessage != null ? childMessage.Name : "");
            template = template.Replace("$ACTION_CLASS", actionClass);

            string deserializationCode = "";
            string serializationCode = "";
            string randomizationCode = "";
            string equalizationCode = "";
            for (int i = 0; i < message.Stuff.Count; i++)
            {
                deserializationCode += message.GenerateDeserializationCode(message.Stuff[i], 1);
                serializationCode += message.GenerateSerializationCode(message.Stuff[i], 1);
                randomizationCode += message.GenerateRandomizationCode(message.Stuff[i], 1);
                equalizationCode += message.GenerateEqualityCode(message.Stuff[i], 1);
            }

            template = template.Replace("$SERIALIZATIONCODE", serializationCode);
            template = template.Replace("$DESERIALIZATIONCODE", deserializationCode);
            template = template.Replace("$RANDOMIZATIONCODE", randomizationCode);
            template = template.Replace("$EQUALITYCODE", equalizationCode);

            return template;
        }


        /// <summary>
        /// Loads the template for the action message class, which holds all six action messages and inserts the code for each
        /// message class.
        /// </summary>
        /// <returns></returns>
        public string GenerateActionMessages()
        {
            var template = Templates.ActionMessagesPlaceHolder;
            template = template.Replace("$NAMESPACE", GoalMessage.Package);

            var messages = new List<MessageTemplateInfo>
            {
                new MessageTemplateInfo(GoalActionMessage, GoalMessage, "$ACTION_GOAL_MESSAGE", "$GOAL_MESSAGE"),
                new MessageTemplateInfo(ResultActionMessage, ResultMessage, "$ACTION_RESULT_MESSAGE", "$RESULT_MESSAGE"),
                new MessageTemplateInfo(FeedbackActionMessage, FeedbackMessage, "$ACTION_FEEDBACK_MESSAGE", "$FEEDBACK_MESSAGE")
            };

            foreach (var messagePair in messages)
            {
                var generatedCode = GenerateMessageFromTemplate(Templates.InnerMessageTemplate, messagePair.InnerMessage, null);
                template = template.Replace(messagePair.InnerMessagePlaceHolder, generatedCode);
                /*generatedCode = GenerateMessageFromTemplate(Templates.ActionMessageTemplate, messagePair.OuterMessage,
                    messagePair.InnerMessage
                );
                template = template.Replace(messagePair.OuterMessagePlaceHoder, generatedCode);*/
            }

            return template;
        }


        /// <summary>
        /// Wrapper to create a MsgsFile
        /// </summary>
        private MsgFile CreateMessageFile(MsgFileLocation messageLocation, List<string> parameters, string suffix)
        {
            var result = new MsgFile(new MsgFileLocation(
                messageLocation.Path, messageLocation.searchroot),
                parameters,
                suffix
            );

            return result;
        }


        private void InitializeActionFile(MsgFileLocation filename, string[] lines)
        {
            MsgFileLocation = filename;
            Name = filename.package + "." + filename.basename;
            className = filename.basename;
            fileNamespace += "." + filename.package;

            var parsedAction = ParseActionFile(lines);

            // Goal Messages
            GoalMessage = CreateMessageFile(filename, parsedAction.GoalParameters, "Goal");
            GoalActionMessage = CreateMessageFile(filename, parsedAction.GoalActionParameters, "ActionGoal");
            GoalActionMessage.ActionMessageType = ActionMessageType.Goal;


            // Result Messages
            ResultMessage = CreateMessageFile(filename, parsedAction.ResultParameters, "Result");
            ResultActionMessage = CreateMessageFile(filename, parsedAction.ResultActionParameters, "ActionResult");
            ResultActionMessage.ActionMessageType = ActionMessageType.Result;

            // Feedback Messages
            FeedbackMessage = CreateMessageFile(filename, parsedAction.FeedbackParameters, "Feedback");
            FeedbackActionMessage = CreateMessageFile(filename, parsedAction.FeedbackActionParameters, "ActionFeedback");
            FeedbackActionMessage.ActionMessageType = ActionMessageType.Feedback;
        }


        /// <summary>
        /// Extracts and generates the parameters for the six messages that are needed to use the actionlib, i.e. Goal,
        /// ActionGoal, Result, ActionResult, Feedback, ActionFeedback. The Action parameters are the ones that are generated.
        /// </summary>
        /// <param name="lines">The content of the .action file</param>
        /// <returns>A ValueTuple with the parameters in a different field</returns>
        private (List<string> GoalParameters, List<string> ResultParameters, List<string> FeedbackParameters,
            List<string> GoalActionParameters, List<string> ResultActionParameters, List<string> FeedbackActionParameters)
            ParseActionFile (string[] lines)
        {
            var goalParameters = new List<string>();
            var resultParameters = new List<string>();
            var feedbackParameters = new List<string>();
            var goalActionParameters = new List<string>();
            var resultActionParameters = new List<string>();
            var feedbackActionParameters = new List<string>();

            linesOfActionFile = new List<string>();
            int foundDelimeters = 0;

            // Search through for the "---" separator between request and response
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                lines[lineNumber] = lines[lineNumber].Replace("\"", "\\\"");
                if (lines[lineNumber].Contains('#'))
                {
                    lines[lineNumber] = lines[lineNumber].Substring(0, lines[lineNumber].IndexOf('#'));
                }
                lines[lineNumber] = lines[lineNumber].Trim();

                if (lines[lineNumber].Length == 0)
                {
                    continue;
                }
                linesOfActionFile.Add(lines[lineNumber]);

                if (lines[lineNumber].Contains("---"))
                {
                    foundDelimeters += 1;
                }

                if (goalActionParameters.Count == 0)
                {
                    // Check always if the goal action header has been set, because in the case of an empty goal, the delimeter
                    // would jump over the this header
                    goalActionParameters.Add("Header header");
                    goalActionParameters.Add("actionlib_msgs/GoalID goal_id");
                    goalActionParameters.Add($"{Name.Replace(".", "/")}Goal goal");
                }

                if (foundDelimeters == 0)
                {
                    goalParameters.Add(lines[lineNumber]);
                }
                else if (foundDelimeters == 1)
                {
                    if (resultActionParameters.Count == 0)
                    {
                        resultActionParameters.Add("Header header");
                        resultActionParameters.Add("actionlib_msgs/GoalStatus status");
                        resultActionParameters.Add($"{Name.Replace(".", "/")}Result result");
                    } else
                    {
                        resultParameters.Add(lines[lineNumber]);
                    }
                }
                else if (foundDelimeters == 2)
                {
                    if (feedbackActionParameters.Count == 0)
                    {
                        feedbackActionParameters.Add("Header header");
                        feedbackActionParameters.Add("actionlib_msgs/GoalStatus status");
                        feedbackActionParameters.Add($"{Name.Replace(".", "/")}Feedback feedback");
                    } else
                    {
                        feedbackParameters.Add(lines[lineNumber]);
                    }
                } else
                {
                    throw new InvalidOperationException($"Action file has an unexpected amount of --- delimeters.");
                }
            }

            return (goalParameters, resultParameters, feedbackParameters, goalActionParameters, resultActionParameters,
                feedbackActionParameters);
        }


        private readonly List<string> actionInterfaceImplementations = new List<string>
        {
            // No Action Message
            "",
            // Goal Action Message
            "public Messages.std_msgs.Header Header { get { return header; } set { header = value; } }" + "\n" +
            "public Messages.actionlib_msgs.GoalID GoalId { get { return goal_id; } set { goal_id = value; } }",
            // Result Action Message
            "public Messages.std_msgs.Header Header { get { return header; } set { header = value; } }" + "\n" +
            "public Messages.actionlib_msgs.GoalStatus GoalStatus { get { return status; } set { status = value; } }",
            // Feedback Action Message
            "public Messages.std_msgs.Header Header { get { return header; } set { header = value; } }" + "\n" +
            "public Messages.actionlib_msgs.GoalStatus GoalStatus { get { return status; } set { status = value; } }",
        };
    }


    class MessageTemplateInfo
    {
        public MsgFile OuterMessage { get; }
        public MsgFile InnerMessage { get; }
        public string OuterMessagePlaceHoder { get; }
        public string InnerMessagePlaceHolder { get; }

        public MessageTemplateInfo(MsgFile OuterMessage, MsgFile InnerMessage, string OuterMessagePlaceHoder,
            string InnerMessagePlaceHolder)
        {
            this.OuterMessage = OuterMessage;
            this.InnerMessage = InnerMessage;
            this.OuterMessagePlaceHoder = OuterMessagePlaceHoder;
            this.InnerMessagePlaceHolder = InnerMessagePlaceHolder;
        }
    }
}
