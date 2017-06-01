using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using YAMLParser;

namespace FauxMessages
{
    public static class KnownStuff
    {
        private static char[] spliter = { ' ' };

        /// <summary>
        /// Message namespaces known to ALL messages (in their header's using lines)
        /// </summary>
        private const string STATIC_NAMESPACE_STRING = "using Messages.std_msgs;\nusing String=System.String;\nusing Messages.geometry_msgs;";

        /// <summary>
        /// Returns the namespaced type name (if neccessary)
        /// </summary>
        /// <param name="st">This thing's SingleType</param>
        /// <param name="type">(optional) the type string</param>
        /// <returns>duh</returns>
        public static string GetNamespacedType(SingleType st, string type = null)
        {
            if (type == null)
                type = st.Type;
            if (st.Package != null && !KnownTypes.ContainsKey(st.rostype) && !type.Contains(st.Package))
                return string.Format("Messages.{0}.{1}", st.Package, type);
            return type;
        }

        public static Dictionary<string, string> KnownTypes = new Dictionary<string, string>
        {
            { "float64", "double" },
            { "float32", "Single" },
            { "uint64", "ulong" },
            { "uint32", "uint" },
            { "uint16", "ushort" },
            { "uint8", "byte" },
            { "int64", "long" },
            { "int32", "int" },
            { "int16", "short" },
            { "int8", "sbyte" },
            { "byte", "byte" },
            { "bool", "bool" },
            { "char", "char" },
            { "time", "Time" },
            { "string", "string" },
            { "duration", "Duration"}
        };

        public static string GetConstTypesAffix(string type)
        {
            switch (type.ToLower())
            {
                case "decimal":
                    return "m";
                case "single":
                case "float":
                    return "f";
                case "long":
                    return "l";
                case "ulong":
                    return "ul";
                case "uint":
                    return "u";
                default:
                    return "";
            }
        }

        public static SingleType WhatItIs(MsgFile parent, string s, string extraindent)
        {
            string[] pieces = s.Split('/');
            string package = null;
            if (pieces.Length == 2)
            {
                package = pieces[0];
                s = pieces[1];
            }
            SingleType st = new SingleType(package, s, extraindent);
            parent.resolve(st);
            WhatItIs(parent, st);
            return st;
        }

        public static void WhatItIs(MsgFile parent, SingleType t)
        {
            if (t.IsPrimitve)
            {
                t.rostype = t.Type;
                SingleType.Finalize(parent, t);
                return;
            }

            t.Finalize(parent, t.input.Split(spliter, StringSplitOptions.RemoveEmptyEntries), false);
        }
    }
}
