using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FauxMessages;
using Microsoft.Extensions.Logging;
using Uml.Robotics.Ros;

namespace YAMLParser
{
    public static class MD5
    {
        public static Dictionary<string, string> md5memo = new Dictionary<string, string>();
        public static Dictionary<string, string> srvmd5memo = new Dictionary<string, string>();
        private static ILogger Logger { get; } = ApplicationLogging.CreateLogger("MD5");

        public static string Sum(SrvFile srvFile)
        {
            
            if (!srvmd5memo.ContainsKey(srvFile.Name))
            {
                string hashableReq = PrepareToHash(srvFile.Request);
                string hashableRes = PrepareToHash(srvFile.Response);
                if (hashableReq == null || hashableRes == null)
                    return null;

                byte[] req = Encoding.ASCII.GetBytes(hashableReq);
                byte[] res = Encoding.ASCII.GetBytes(hashableRes);

                var md5 = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.MD5);
                md5.AppendData(req);
                md5.AppendData(res);
                var hash = md5.GetHashAndReset();

                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.AppendFormat("{0:x2}", hash[i]);
                }
                srvmd5memo.Add(srvFile.Name, sb.ToString());
            }
            return srvmd5memo[srvFile.Name];
        }

        public static string Sum(MsgFile m)
        {
            if (!md5memo.ContainsKey(m.Name))
            {
                string hashText = PrepareToHash(m);
                if (hashText == null)
                    return null;
                md5memo[m.Name] = Sum(hashText);
            }
            return md5memo[m.Name];
        }

        public static string Sum(ActionFile actionFile)
        {
            if (!srvmd5memo.ContainsKey(actionFile.Name))
            {
                Sum(actionFile.GoalMessage);
                Sum(actionFile.ResultMessage);
                Sum(actionFile.FeedbackMessage);
                string hashableGoal = PrepareToHash(actionFile.GoalMessage);
                string hashableResult = PrepareToHash(actionFile.ResultMessage);
                string hashableFeedback = PrepareToHash(actionFile.FeedbackMessage);
                if (hashableGoal == null || hashableResult == null || hashableFeedback == null)
                    return null;

                byte[] goal = Encoding.ASCII.GetBytes(hashableGoal);
                byte[] result = Encoding.ASCII.GetBytes(hashableResult);
                byte[] feedback = Encoding.ASCII.GetBytes(hashableFeedback);

                var md5 = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.MD5);
                md5.AppendData(goal);
                md5.AppendData(result);
                md5.AppendData(feedback);
                var hash = md5.GetHashAndReset();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.AppendFormat("{0:x2}", hash[i]);
                }
                srvmd5memo.Add(actionFile.Name, sb.ToString());
            }
            return srvmd5memo[actionFile.Name];
        }

        private static string PrepareToHash(MsgFile msgFile)
        {
            string hashText = msgFile.Definition.Trim('\n', '\t', '\r', ' ');
            while (hashText.Contains("  "))
                hashText = hashText.Replace("  ", " ");
            while (hashText.Contains("\r\n"))
                hashText = hashText.Replace("\r\n", "\n");
            hashText = hashText.Trim();
            string[] lines = hashText.Split('\n');

            var haves = new Queue<string>();
            var havenots = new Queue<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.Contains("="))
                {
                    // condense spaces on either side of =
                    string[] ls = l.Split('=');
                    haves.Enqueue(ls[0].Trim() + "=" + ls[1].Trim());
                }
                else
                {
                    havenots.Enqueue(l.Trim());
                }
            }

            hashText = "";
            while (haves.Count + havenots.Count > 0)
            {
                hashText += (haves.Count > 0 ? haves.Dequeue() : havenots.Dequeue()) + (haves.Count + havenots.Count >= 1 ? "\n" : "");
            }

            Dictionary<string, MsgFieldInfo> mfis = MessageFieldHelper.Instantiate(msgFile.Stuff);
            MsgFieldInfo[] fields = mfis.Values.ToArray();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.IsPrimitive)
                {
                    continue;
                }

                MsgFile ms = msgFile.Stuff[i].Definer;
                if (ms == null)
                {
                    KnownStuff.WhatItIs(msgFile, msgFile.Stuff[i]);
                    if (msgFile.Stuff[i].Type.Contains("/"))
                    {
                        msgFile.resolve(msgFile.Stuff[i]);
                    }
                    ms = msgFile.Stuff[i].Definer;
                }
                string sum = null;
                if (ms == null)
                {
                    RosMessage rosMessage = null;
                    var packages = MessageTypeRegistry.Default.PackageNames;
                    foreach (var package in packages)
                    {
                        try
                        {
                            var name = msgFile.Stuff[i].Type;
                            Console.WriteLine($"generate {package}/{name}");
                            rosMessage = RosMessage.Generate($"{package}/{name}");
                            sum = rosMessage.MD5Sum();
                            break;
                        }
                        catch
                        {
                        }
                    }
                    if (rosMessage == null)
                    {
                        Logger.LogDebug("NEEDS ANOTHER PASS: " + msgFile.Name + " B/C OF " + msgFile.Stuff[i].Type);
                        return null;
                    }
                }
                else
                {
                    sum = MD5.Sum(ms);
                }
                if (sum == null)
                {
                    Logger.LogDebug("STILL NEEDS ANOTHER PASS: " + msgFile.Name + " B/C OF " + msgFile.Stuff[i].Type);
                    return null;
                }

                hashText = Regex.Replace(hashText, @"^(.*/)?\b" + fields[i].Type + @"\b(\s*\[\s*\])?", sum, RegexOptions.Multiline);
            }
            return hashText;
        }

        public static string Sum(params string[] str)
        {
            return Sum(str.Select(s => Encoding.ASCII.GetBytes(s)).ToArray());
        }

        public static string Sum(params byte[][] data)
        {
            var md5 = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.MD5);
            if (data.Length > 0)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    md5.AppendData(data[i]);
                }
            }

            var hash = md5.GetHashAndReset();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.AppendFormat("{0:x2}", hash[i]);
            }
            return sb.ToString();
        }
    }
}