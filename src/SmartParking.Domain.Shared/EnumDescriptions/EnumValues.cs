using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.EnumDescriptions;

public static class EnumValues
{
    public static string GetEnumDescription(Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        return attribute == null ? value.ToString() : attribute.Description;
    }
}