using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FauxMessages
{
    public class MsgFieldInfo
    {
        public string ConstVal;
        public bool IsArray;
        public bool IsConst;
        public bool IsPrimitive;
        public bool IsComplexType;
        public int Length = -1;
        public string Name;
        public string Type;

        public MsgFieldInfo(string name, bool isPrimitive, string type, bool isConst, string constVal, bool isArray,
            string lengths, bool complexType)
        {
            Name = name;
            IsArray = isArray;
            Type = type;
            IsPrimitive = isPrimitive;
            IsComplexType = complexType;
            IsConst = isConst;
            ConstVal = constVal;
            if (!string.IsNullOrWhiteSpace(lengths))
            {
                Length = int.Parse(lengths);
            }
        }
    }
}
