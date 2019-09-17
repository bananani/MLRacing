using UnityEngine;

namespace Common.Attributes
{
    public class EnumFlagAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagAttribute() { }

        public EnumFlagAttribute(string name)
        {
            enumName = name;
        }
    }
}