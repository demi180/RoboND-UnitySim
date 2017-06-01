using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Uml.Robotics.XmlRpc;

namespace Uml.Robotics.Ros
{
    public delegate void ParamDelegate(string key, XmlRpcValue value);
    public delegate void ParamStringDelegate(string key, string value);
    public delegate void ParamDoubleDelegate(string key, double value);
    public delegate void ParamIntDelegate(string key, int value);
    public delegate void ParamBoolDelegate(string key, bool value);

    public static class Param
    {
        private static ILogger Logger { get; } = ApplicationLogging.CreateLogger(nameof(Param));
        public static Dictionary<string, XmlRpcValue> parms = new Dictionary<string, XmlRpcValue>();
        public static object parms_mutex = new object();
        public static List<string> subscribed_params = new List<string>();
        private static Dictionary<string, List<ParamStringDelegate>> StringCallbacks = new Dictionary<string, List<ParamStringDelegate>>();
        private static Dictionary<string, List<ParamIntDelegate>> IntCallbacks = new Dictionary<string, List<ParamIntDelegate>>();
        private static Dictionary<string, List<ParamDoubleDelegate>> DoubleCallbacks = new Dictionary<string, List<ParamDoubleDelegate>>();
        private static Dictionary<string, List<ParamBoolDelegate>> BoolCallbacks = new Dictionary<string, List<ParamBoolDelegate>>();
        private static Dictionary<string, List<ParamDelegate>> Callbacks = new Dictionary<string, List<ParamDelegate>>();

        public static void Subscribe(string key, ParamBoolDelegate del)
        {
            if (!BoolCallbacks.ContainsKey(key))
                BoolCallbacks.Add(key, new List<ParamBoolDelegate>());
            BoolCallbacks[key].Add(del);
            Update(key, GetParam(key, true));
        }

        public static void Subscribe(string key, ParamIntDelegate del)
        {
            if (!IntCallbacks.ContainsKey(key))
                IntCallbacks.Add(key, new List<ParamIntDelegate>());
            IntCallbacks[key].Add(del);
            Update(key, GetParam(key, true));
        }

        public static void Subscribe(string key, ParamDoubleDelegate del)
        {
            if (!DoubleCallbacks.ContainsKey(key))
                DoubleCallbacks.Add(key, new List<ParamDoubleDelegate>());
            DoubleCallbacks[key].Add(del);
            Update(key, GetParam(key, true));
        }

        public static void Subscribe(string key, ParamStringDelegate del)
        {
            if (!StringCallbacks.ContainsKey(key))
                StringCallbacks.Add(key, new List<ParamStringDelegate>());
            StringCallbacks[key].Add(del);
            Update(key, GetParam(key, true));
        }

        public static void Subscribe(string key, ParamDelegate del)
        {
            if (!Callbacks.ContainsKey(key))
                Callbacks.Add(key, new List<ParamDelegate>());
            Callbacks[key].Add(del);
            Update(key, GetParam(key, true));
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="mapped_key">Fully mapped name of the parameter to be set</param>
        /// <param name="val">Value of the paramter</param>
        private static void SetOnServer(string mapped_key, XmlRpcValue parm)
        {
            parm.Set(0, ThisNode.Name);
            parm.Set(1, mapped_key);
            // parm.Set(2, ...), the value to be set on the parameter server was stored in parm by the calling function already )

            var response = new XmlRpcValue();
            var payload = new XmlRpcValue();
            lock (parms_mutex)
            {
                if (Master.execute("setParam", parm, response, payload, true))
                {
                    if (subscribed_params.Contains(mapped_key))
                        parms.Add(mapped_key, parm);
                }
                else
                {
                    throw new RosException("RPC call setParam for key " + mapped_key + " failed. ");
                }
            }
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <param name="val">Value of the paramter</param>
        public static void Set(string key, XmlRpcValue val)
        {
            XmlRpcValue parm = new XmlRpcValue();
            parm.Set(2, val);
            SetOnServer(Names.Resolve(key), parm);
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <param name="val">Value of the paramter</param>
        public static void Set(string key, string val)
        {
            XmlRpcValue parm = new XmlRpcValue();
            parm.Set(2, val);
            SetOnServer(Names.Resolve(key), parm);
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <param name="val">Value of the paramter</param>
        public static void Set(string key, double val)
        {
            XmlRpcValue parm = new XmlRpcValue();
            parm.Set(2, val);
            SetOnServer(Names.Resolve(key), parm);
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <param name="val">Value of the paramter</param>
        public static void Set(string key, int val)
        {
            XmlRpcValue parm = new XmlRpcValue();
            parm.Set(2, val);
            SetOnServer(Names.Resolve(key), parm);
        }

        /// <summary>
        ///     Sets the paramater on the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <param name="val">Value of the paramter</param>
        public static void Set(string key, bool val)
        {
            XmlRpcValue parm = new XmlRpcValue();
            parm.Set(2, val);
            SetOnServer(Names.Resolve(key), parm);
        }

        /// <summary>
        ///     Gets the parameter from the parameter server
        /// </summary>
        /// <param name="key">Name of the parameter</param>
        /// <returns></returns>
        internal static XmlRpcValue GetParam(String key, bool use_cache = false)
        {
            string mapped_key = Names.Resolve(key);
            XmlRpcValue payload;
            if (!GetImpl(mapped_key, out payload, use_cache))
                payload = null;
            return payload;
        }

        private static bool SafeGet<T>(string key, out T dest, T def = default(T))
        {
            try
            {
                XmlRpcValue v = GetParam(key);
                if (v == null || !v.IsEmpty)
                {
                    if (def == null)
                    {
                        dest=default(T);
                        return false;
                    }
                    dest = def;
                    return true;
                }

                // TODO: Change this....
                if (typeof(T) == typeof(int))
                {
                    dest = (T)(object)v.GetInt();
                }
                else if (typeof(T) == typeof(string))
                {
                    dest = (T)(object)v.GetString();
                }
                else if (typeof(T) == typeof(bool))
                {
                    dest = (T)(object)v.GetBool();
                }
                else if (typeof(T) == typeof(int))
                {
                    dest = (T)(object)v.GetInt();
                }
                else if (typeof(T) == typeof(XmlRpcValue))
                {
                    dest = (T)(object)v;
                }
                else
                {
                    dest=default(T);
                }

                return true;
            }
            catch
            {
                dest=default(T);
                return false;
            }
        }

        public static bool Get(string key, out XmlRpcValue dest)
        {
            return SafeGet(key, out dest);
        }

        public static bool Get(string key, out bool dest)
        {
            return SafeGet(key, out dest);
        }

        public static bool Get(string key, out bool dest, bool def)
        {
            return SafeGet(key, out dest, def);
        }

        public static bool Get(string key, out int dest)
        {
            return SafeGet(key, out dest);
        }

        public static bool Get(string key, out int dest, int def)
        {
            return SafeGet(key, out dest, def);
        }

        public static bool Get(string key, out double dest)
        {
            return SafeGet(key, out dest);
        }

        public static bool Get(string key, out double dest, double def)
        {
            return SafeGet(key, out dest, def);
        }

        public static bool Get(string key, out string dest, string def = null)
        {
            return SafeGet(key, out dest, def);
        }

        public static List<string> List()
        {
            List<string> ret = new List<string>();
            XmlRpcValue parm = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            parm.Set(0, ThisNode.Name);
            if (!Master.execute("getParamNames", parm, result, payload, false))
                return ret;
            if (result.Count != 3 || result[0].GetInt() != 1 || result[2].Type != XmlRpcType.Array)
            {
                Logger.LogWarning("Expected a return code, a description, and a list!");
                return ret;
            }
            for (int i = 0; i < payload.Count; i++)
            {
                ret.Add(payload[i].GetString());
            }
            return ret;
        }

        /// <summary>
        ///     Checks if the paramter exists.
        /// </summary>
        /// <param name="key">Name of the paramerer</param>
        /// <returns></returns>
        public static bool Has(string key)
        {
            XmlRpcValue parm = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            parm.Set(0, ThisNode.Name);
            parm.Set(1, Names.Resolve(key));
            if (!Master.execute("hasParam", parm, result, payload, false))
                return false;
            if (result.Count != 3 || result[0].GetInt() != 1 || result[2].Type != XmlRpcType.Boolean)
                return false;
            return result[2].GetBool();
        }

        /// <summary>
        ///     Deletes a parameter from the parameter server.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Del(string key)
        {
            string mapped_key = Names.Resolve(key);
            lock (parms_mutex)
            {
                if (subscribed_params.Contains(key))
                {
                    subscribed_params.Remove(key);
                    if (parms.ContainsKey(key))
                        parms.Remove(key);
                }
            }

            XmlRpcValue parm = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            parm.Set(0, ThisNode.Name);
            parm.Set(1, mapped_key);
            if (!Master.execute("deleteParam", parm, result, payload, false))
                return false;
            return true;
        }

        public static void Init(IDictionary<string, string> remappingArgs)
        {
            foreach (string name in remappingArgs.Keys)
            {
                string param = remappingArgs[name];
                if (name.Length < 2)
                    continue;
                if (name[0] == '_' && name[1] != '_')
                {
                    string localName = "~" + name.Substring(1);
                    bool success = int.TryParse(param, out int i);
                    if (success)
                    {
                        Set(Names.Resolve(localName), i);
                        continue;
                    }
                    success = double.TryParse(param, out double d);
                    if (success)
                    {
                        Set(Names.Resolve(localName), d);
                        continue;
                    }
                    success = bool.TryParse(param.ToLower(), out bool b);
                    if (success)
                    {
                        Set(Names.Resolve(localName), b);
                        continue;
                    }
                    Set(Names.Resolve(localName), param);
                }
            }
            XmlRpcManager.Instance.Bind("paramUpdate", ParamUpdateCallback);
        }

        /// <summary>
        ///     Manually update the value of a parameter
        /// </summary>
        /// <param name="key">Name of parameter</param>
        /// <param name="v">Value to update param to</param>
        public static void Update(string key, XmlRpcValue v)
        {
            if (v == null)
                return;
            string clean_key = Names.Clean(key);
            lock (parms_mutex)
            {
                if (!parms.ContainsKey(clean_key))
                    parms.Add(clean_key, v);
                else
                    parms[clean_key] = v;
                if (BoolCallbacks.ContainsKey(clean_key))
                {
                    foreach (ParamBoolDelegate b in BoolCallbacks[clean_key])
                        b.Invoke(clean_key, new XmlRpcValue(v).GetBool());
                }
                if (IntCallbacks.ContainsKey(clean_key))
                {
                    foreach (ParamIntDelegate b in IntCallbacks[clean_key])
                        b.Invoke(clean_key, new XmlRpcValue(v).GetInt());
                }
                if (DoubleCallbacks.ContainsKey(clean_key))
                {
                    foreach (ParamDoubleDelegate b in DoubleCallbacks[clean_key])
                        b.Invoke(clean_key, new XmlRpcValue(v).GetDouble());
                }
                if (StringCallbacks.ContainsKey(clean_key))
                {
                    foreach (ParamStringDelegate b in StringCallbacks[clean_key])
                        b.Invoke(clean_key, new XmlRpcValue(v).GetString());
                }
                if (Callbacks.ContainsKey(clean_key))
                {
                    foreach (ParamDelegate b in Callbacks[clean_key])
                        b.Invoke(clean_key, new XmlRpcValue(v));
                }
            }
        }

        /// <summary>
        ///     Fired when a parameter gets updated
        /// </summary>
        /// <param name="parm">Name of parameter</param>
        /// <param name="result">New value of parameter</param>
        public static void ParamUpdateCallback(XmlRpcValue val, XmlRpcValue result)
        {
            val.Set(0, 1);
            val.Set(1, "");
            val.Set(2, 0);
            //update(XmlRpcValue.LookUp(parm)[1].Get<string>(), XmlRpcValue.LookUp(parm)[2]);
            /// TODO: check carefully this stuff. It looks strange
            Update(val[1].GetString(), val[2]);
        }

        public static bool GetImpl(string key, out XmlRpcValue v, bool use_cache)
        {
            string mapped_key = Names.Resolve(key);
            v=new XmlRpcValue();

            if (use_cache)
            {
                lock (parms_mutex)
                {
                    if (subscribed_params.Contains(mapped_key))
                    {
                        if (parms.ContainsKey(mapped_key))
                        {
                            if (parms[mapped_key].IsEmpty)
                            {
                                v = parms[mapped_key];
                                return true;
                            }
                            return false;
                        }
                    }
                    else
                    {
                        subscribed_params.Add(mapped_key);
                        XmlRpcValue parm = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
                        parm.Set(0, ThisNode.Name);
                        parm.Set(1, XmlRpcManager.Instance.Uri);
                        parm.Set(2, mapped_key);
                        if (!Master.execute("subscribeParam", parm, result, payload, false))
                        {
                            subscribed_params.Remove(mapped_key);
                            use_cache = false;
                        }
                    }
                }
            }

            XmlRpcValue parm2 = new XmlRpcValue(), result2 = new XmlRpcValue();
            parm2.Set(0, ThisNode.Name);
            parm2.Set(1, mapped_key);
            v.SetArray(0);

            bool ret = Master.execute("getParam", parm2, result2, v, false);

            if (use_cache)
            {
                lock (parms_mutex)
                {
                    parms.Add(mapped_key, v);
                }
            }

            return ret;
        }

        internal static void Reset()
        {
            parms.Clear();
            subscribed_params.Clear();
            StringCallbacks.Clear();
            IntCallbacks.Clear();
            DoubleCallbacks.Clear();
            BoolCallbacks.Clear();
            Callbacks.Clear();
        }

        internal static void Terminate()
        {
            XmlRpcManager.Instance.Unbind("paramUpdate");
        }
    }
}
