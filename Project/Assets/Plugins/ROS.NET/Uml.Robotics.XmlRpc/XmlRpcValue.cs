using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Collections;

namespace Uml.Robotics.XmlRpc
{
    public enum XmlRpcType
    {
        Empty,
        Boolean,
        Int,
        Double,
        String,
        DateTime,
        Base64,
        Array,
        Struct
    }

    public class XmlRpcValue
        : IEnumerable<XmlRpcValue>
    {
        private static readonly XName VALUE_TAG = "value";
        private static readonly XName BOOLEAN_TAG = "boolean";
        private static readonly XName DOUBLE_TAG = "double";
        private static readonly XName INT_TAG = "int";
        private static readonly XName I4_TAG = "i4";
        private static readonly XName STRING_TAG = "string";
        private static readonly XName DATETIME_TAG = "dateTime.iso8601";
        private static readonly XName BASE64_TAG = "base64";
        private static readonly XName ARRAY_TAG = "array";
        private static readonly XName DATA_TAG = "data";
        private static readonly XName STRUCT_TAG = "struct";
        private static readonly XName MEMBER_TAG = "member";
        private static readonly XName NAME_TAG = "name";

        private XmlRpcType type = XmlRpcType.Empty;
        object value;

        public XmlRpcValue()
        {
        }

        public XmlRpcValue(params object[] initialvalues)
        {
            SetArray(initialvalues.Length);
            for (int i = 0; i < initialvalues.Length; i++)
            {
                SetFromObject(i, initialvalues[i]);
            }
        }

        public XmlRpcValue(IEnumerable<object> arrayValues)
            : this(arrayValues.ToArray())
        {
        }

        public XmlRpcValue(bool value)
        {
            Set(value);
        }

        public XmlRpcValue(int value)
        {
            Set(value);
        }

        public XmlRpcValue(double value)
        {
            Set(value);
        }

        public XmlRpcValue(string value)
        {
            Set(value);
        }

        public int Count
        {
            get
            {
                switch (type)
                {
                    case XmlRpcType.String:
                        return GetString().Length;
                    case XmlRpcType.Base64:
                        return GetBinary().Length;
                    case XmlRpcType.Array:
                        return GetArray().Length;
                    case XmlRpcType.Struct:
                        return GetStruct().Count;
                    default:
                        return 0;
                }
            }
        }

        public bool IsEmpty
        {
            get { return type != XmlRpcType.Empty; }
        }

        public XmlRpcType Type
        {
            get { return type; }
        }

        public bool IsArray
        {
            get { return type == XmlRpcType.Array; }
        }

        public bool IsStruct
        {
            get { return type == XmlRpcType.Struct; }
        }

        public XmlRpcValue this[int index]
        {
            get
            {
                EnsureArraySize(index + 1);
                return Get(index);
            }
            set
            {
                var array = EnsureArraySize(index + 1);
                if (array[index] == null)
                {
                    array[index] = new XmlRpcValue();
                }
                array[index].Set(value);
            }
        }

        public XmlRpcValue this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        private void SetFromObject(int index, object value)
        {
            if (value == null)
                Set(index, string.Empty);
            else if (value is string)
                Set(index, (string)value);
            else if (value is int)
                Set(index, (int)value);
            else if (value is double)
                Set(index, (double)value);
            else if (value is bool)
                Set(index, (bool)value);
            else if (value is DateTime)
                Set(index, (DateTime)value);
            else if (value is byte[])
                Set(index, (byte[])value);
            else
                throw new XmlRpcException($"Cannot set from object {value}.");
        }

        public override bool Equals(object obj)
        {
            var other = obj as XmlRpcValue;

            if (other == null || type != other.type)
                return false;

            switch (type)
            {
                case XmlRpcType.Boolean:
                case XmlRpcType.Int:
                case XmlRpcType.Double:
                case XmlRpcType.String:
                case XmlRpcType.DateTime:
                    return object.Equals(value, other.value);
                case XmlRpcType.Base64:
                    return this.GetBinary().SequenceEqual(other.GetBinary());
                case XmlRpcType.Array:
                    return this.GetArray().SequenceEqual(other.GetArray());
                case XmlRpcType.Struct:
                    return this.GetStruct().SequenceEqual(other.GetStruct());
                case XmlRpcType.Empty:
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return value != null ? value.GetHashCode() : base.GetHashCode();
        }

        public void Copy(XmlRpcValue other)
        {
            switch (other.type)
            {
                case XmlRpcType.Base64:
                    value = other.GetBinary().Clone();
                    break;
                case XmlRpcType.Array:
                    value = other.GetArray().Clone();
                    break;
                case XmlRpcType.Struct:
                    value = new Dictionary<string, XmlRpcValue>(other.GetStruct());
                    break;
                default:
                    value = other.value;
                    break;
            }

            type = other.type;
        }

        public bool HasMember(string name)
        {
            return type == XmlRpcType.Struct && GetStruct().ContainsKey(name);
        }

        public void FromXml(string xml)
        {
            FromXElement(XElement.Parse(xml));
        }

        public void FromXElement(XElement valueElement)
        {
            if (valueElement == null)
                throw new ArgumentNullException(nameof(valueElement), "Value element must not be null.");

            var content = valueElement.Elements().FirstOrDefault();
            if (content == null)
            {
                Set(valueElement.Value);
            }
            else if (content.Name == BOOLEAN_TAG)
            {
                int x = (int)content;
                if (x == 0)
                    Set(false);
                else if (x == 1)
                    Set(true);
                else
                    throw new XmlRpcException("XML-RPC boolean value must be '0' or '1'.");
            }
            else if (content.Name == I4_TAG || content.Name == INT_TAG)
            {
                Set((int)content);
            }
            else if (content.Name == DOUBLE_TAG)
            {
                Set((double)content);
            }
            else if (content.Name == DATETIME_TAG)
            {
                Set(XmlConvert.ToDateTime(content.Value, XmlDateTimeSerializationMode.RoundtripKind));
            }
            else if (content.Name == BASE64_TAG)
            {
                Set(Convert.FromBase64String(content.Value));
            }
            else if (content.Name == STRING_TAG)
            {
                Set(valueElement.Value);
            }
            else if (content.Name == ARRAY_TAG)
            {
                var dataElement = content.Element(DATA_TAG);
                if (dataElement == null)
                    throw new XmlRpcException("Expected <data> element is missing.");
                var valueElements = dataElement.Elements(VALUE_TAG).ToList();
                SetArray(valueElements.Count);
                for (int i = 0; i < valueElements.Count; i++)
                {
                    var v = new XmlRpcValue();
                    v.FromXElement(valueElements[i]);
                    Set(i, v);
                }
            }
            else if (content.Name == STRUCT_TAG)
            {
                foreach (var memberElement in content.Elements(MEMBER_TAG))
                {
                    var nameElement = memberElement.Element(NAME_TAG);
                    if (nameElement == null)
                        throw new XmlRpcException("Expected <name> element is missing.");
                    var name = nameElement.Value;
                    var v = new XmlRpcValue();
                    v.FromXElement(memberElement.Element(VALUE_TAG));
                    Set(name, v);
                }
            }
            else
            {
                Set(valueElement.Value);
            }
        }

        public string ToXml()
        {
            var settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                CloseOutput = false
            };

            var sw = new StringWriter();
            using (var writer = XmlWriter.Create(sw, settings))
            {
                var valueElement = this.ToXElement();
                valueElement.WriteTo(writer);
            }
            return sw.ToString();
        }

        public XElement ToXElement()
        {
            var valueElement = new XElement(VALUE_TAG);
            switch (type)
            {
                case XmlRpcType.Boolean:
                    valueElement.Add(new XElement(BOOLEAN_TAG, GetBool() ? 1 : 0));
                    break;
                case XmlRpcType.Int:
                    valueElement.Add(new XElement(INT_TAG, GetInt()));
                    break;
                case XmlRpcType.Double:
                    valueElement.Add(new XElement(DOUBLE_TAG, GetDouble()));
                    break;
                case XmlRpcType.DateTime:
                    valueElement.Add(new XElement(DATETIME_TAG, XmlConvert.ToString(GetDateTime(), XmlDateTimeSerializationMode.RoundtripKind)));
                    break;
                case XmlRpcType.String:
                    valueElement.Add(new XElement(STRING_TAG, GetString()));
                    break;
                case XmlRpcType.Base64:
                    valueElement.Add(new XElement(BASE64_TAG, Convert.ToBase64String(GetBinary())));
                    break;
                case XmlRpcType.Array:
                    valueElement.Add(new XElement(ARRAY_TAG, new XElement(DATA_TAG, GetArray().Select(x => x.ToXElement()))));
                    break;
                case XmlRpcType.Struct:
                    valueElement.Add(
                        new XElement(STRUCT_TAG,
                            GetStruct()
                            .Select(x => new XElement(MEMBER_TAG,
                                new XElement(NAME_TAG, x.Key), x.Value.ToXElement())
                            )
                        )
                    );
                    break;
                default:
                    throw new XmlRpcException($"Cannot serialize XmlRpcValue type '${type}'.");
            }

            return valueElement;
        }

        public void Set(string value)
        {
            type = XmlRpcType.String;
            this.value = value;
        }

        public void Set(int value)
        {
            type = XmlRpcType.Int;
            this.value = value;
        }

        public void Set(bool value)
        {
            type = XmlRpcType.Boolean;
            this.value = value;
        }

        public void Set(double value)
        {
            type = XmlRpcType.Double;
            this.value = value;
        }

        public void Set(DateTime value)
        {
            type = XmlRpcType.DateTime;
            this.value = value;
        }

        public void Set(byte[] value)
        {
            type = XmlRpcType.Base64;
            this.value = value;
        }

        public void Set(XmlRpcValue value)
        {
            Copy(value);
        }

        public void SetArray(int elementCount)
        {
            type = XmlRpcType.Array;
            EnsureArraySize(elementCount);
        }

		public void Set ( string name, string value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, int value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, bool value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, double value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, byte[] value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, DateTime value )
		{
			Get ( name, true ).Set ( value );
		}
		public void Set ( string name, XmlRpcValue value )
		{
			Get ( name, true ).Set ( value );
		}

		public void Set ( int index, string value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, int value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, bool value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, double value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, byte[] value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, DateTime value )
		{
			this [ index ].Set ( value );
		}
		public void Set ( int index, XmlRpcValue value )
		{
			this [ index ].Set ( value );
		}

		public static explicit operator bool ( XmlRpcValue value )
		{
			return value.GetBool ();
		}
		public static explicit operator int ( XmlRpcValue value )
		{
			return value.GetInt ();
		}
		public static explicit operator double ( XmlRpcValue value )
		{
			return value.GetDouble ();
		}
		public static explicit operator byte[] ( XmlRpcValue value )
		{
			return value.GetBinary ();
		}
		public static explicit operator DateTime ( XmlRpcValue value )
		{
			return value.GetDateTime ();
		}
		public static explicit operator string ( XmlRpcValue value )
		{
			return value.GetString ();
		}

		public IDictionary<string, XmlRpcValue> GetStruct ()
		{
			return (IDictionary<string, XmlRpcValue>) value;
		}
		public XmlRpcValue[] GetArray ()
		{
			return (XmlRpcValue[]) value;
		}
		public int GetInt ()
		{
			return (int) value;
		}
		public string GetString ()
		{
			return (string) value;
		}
		public bool GetBool ()
		{
			return (bool) value;
		}
		public double GetDouble ()
		{
			return (double) value;
		}
		public DateTime GetDateTime ()
		{
			return (DateTime) value;
		}
		public byte[] GetBinary ()
		{
			return (byte[]) value;
		}

        public override string ToString()
        {
            if (!this.IsEmpty)
                return "EMPTY";
            return ToXml();
        }

        private XmlRpcValue[] EnsureArraySize(int size)
        {
            if (type != XmlRpcType.Empty && type != XmlRpcType.Array)
                throw new XmlRpcException($"Cannot convert {type} to array");

            int before = 0;
            var array = value as XmlRpcValue[];
            if (array == null)
            {
                array = new XmlRpcValue[size];
            }
            else
            {
                before = array.Length;
                if (array.Length < size)
                {
                    Array.Resize(ref array, size);
                }
            }

            for (int i = before; i < array.Length; i++)
                array[i] = new XmlRpcValue();

            value = array;
            type = XmlRpcType.Array;
            return array;
        }

		private XmlRpcValue Get ( int index )
		{
			return this.GetArray () [ index ];
		}

        private XmlRpcValue Get(string key, bool createMissing = false)
        {
            if (value == null)
            {
                value = new Dictionary<string, XmlRpcValue>();
                type = XmlRpcType.Struct;
            }
            var s = this.GetStruct();
            if (!s.ContainsKey(key))
            {
                if (createMissing)
                    s[key] = new XmlRpcValue();
                else
                    return null;
            }
            return s[key];
        }

        public IEnumerator<XmlRpcValue> GetEnumerator()
        {
            if (type == XmlRpcType.Array)
            {
                foreach (var x in this.GetArray())
                    yield return x;
            }
            else if (type == XmlRpcType.Struct)
            {
                foreach (var x in this.GetStruct().Values)
                    yield return x;
            }

            // for all other types we do not produce any values (enumerable is empty)
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}