using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public static class TypeExtensions
    {
        public static string FriendlyName(this Type type, bool fullName = false, bool omitGenericArguments = false)
        {
            string text = fullName ? type.FullName : type.Name;
            if (!type.IsGenericType)
            {
                return text;
            }
            int num = text.IndexOf('`');
            if (num > 0)
            {
                text = text.Substring(0, num);
            }
            if (omitGenericArguments)
            {
                return text;
            }
            Type[] genericArguments = type.GetGenericArguments();
            StringBuilder stringBuilder = new StringBuilder(text);
            stringBuilder.Append("<");
            for (int i = 0; i < genericArguments.Length - 1; i++)
            {
                stringBuilder.AppendFormat("{0},", genericArguments[i].FriendlyName(false, false), fullName);
            }
            stringBuilder.AppendFormat("{0}>", genericArguments[genericArguments.Length - 1].FriendlyName(false, false), fullName);
            return stringBuilder.ToString();
        }

        public static bool IsNumeric(this Type type)
        {
            if (type.IsSubclassOfRawGeneric(typeof(Nullable<>)))
            {
                type = type.GenericTypeArguments.First<Type>();
            }
            if (type.IsEnum)
            {
                return false;
            }
            TypeCode typeCode = Type.GetTypeCode(type);
            return typeCode - TypeCode.SByte <= 10;
        }

        public static bool IsNullableNumeric(this Type type)
        {
            return type.IsSubclassOfRawGeneric(typeof(Nullable<>)) && type.GenericTypeArguments.First<Type>().IsNumeric();
        }

        public static bool Browsable(this Type activity)
        {
            bool result;
            try
            {
                BrowsableAttribute browsableAttribute = TypeDescriptor.GetAttributes(activity).OfType<BrowsableAttribute>().FirstOrDefault<BrowsableAttribute>();
                result = (browsableAttribute == null || browsableAttribute.Browsable);
            }
            catch (Exception exception)
            {
                result = false;
            }
            return result;
        }

        public static string CleanInvalidXmlChars(this string text, string replaceWith = "")
        {
            string pattern = "[^\\x09\\x0A\\x0D\\x20-\\uD7FF\\uE000-\\uFFFD\\u10000-\\u10FFFF]";
            return Regex.Replace(text, pattern, replaceWith);
        }

        public static string Description(this Type activity)
        {
            IEnumerable<DescriptionAttribute> source = TypeDescriptor.GetAttributes(activity).OfType<DescriptionAttribute>();
            if (source.Count<DescriptionAttribute>() > 0)
            {
                return source.ElementAt(0).Description;
            }
            return null;
        }

        public static string DisplayName(this Type activity)
        {
            DisplayNameAttribute displayNameAttribute = TypeDescriptor.GetAttributes(activity).OfType<DisplayNameAttribute>().FirstOrDefault<DisplayNameAttribute>();
            if (displayNameAttribute != null)
            {
                return displayNameAttribute.DisplayName;
            }
            if (activity.IsGenericType && !activity.IsGenericTypeDefinition)
            {
                return activity.GetGenericTypeDefinition().DisplayName();
            }
            return null;
        }

        public static string HelpKeyword(this Type activity)
        {
            IEnumerable<HelpKeywordAttribute> source = TypeDescriptor.GetAttributes(activity).OfType<HelpKeywordAttribute>();
            if (source.Count<HelpKeywordAttribute>() > 0)
            {
                return source.ElementAt(0).HelpKeyword;
            }
            return null;
        }

        public static bool IsExpandableProperty(this Type propertyType)
        {
            try
            {
                TypeConverterAttribute typeConverterAttribute = TypeDescriptor.GetAttributes(propertyType).OfType<TypeConverterAttribute>().FirstOrDefault<TypeConverterAttribute>();
                if (typeConverterAttribute == null)
                {
                    return false;
                }
                Type type = Type.GetType(typeConverterAttribute.ConverterTypeName);
                if (type != null)
                {
                    return type.IsAssignableFrom(typeof(ExpandableObjectConverter)) || type.IsSubclassOf(typeof(ExpandableObjectConverter));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            return false;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type right = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == right)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static bool IsSameOrSubclass(this Type toCheck, Type potentialBase)
        {
            return !(toCheck == null) && !(potentialBase == null) && (toCheck.IsSubclassOf(potentialBase) || toCheck == potentialBase);
        }

        public static Type GetAsGenericTypeOrDefault(this Type type)
        {
            if (!type.IsGenericType || type.IsGenericTypeDefinition)
            {
                return type;
            }
            return type.GetGenericTypeDefinition();
        }

        public static Type GetUnderlyingTypeFromGenericTypeOrDefault(this Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (!type.IsGenericType || type.IsGenericTypeDefinition)
            {
                return type;
            }
            return type.GenericTypeArguments.First<Type>();
        }


        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            IEnumerable<Type> result;
            try
            {
                result = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                result = from t in ex.Types
                         where t != null
                         select t;
            }
            return result;
        }

        public static Attribute TryGetAttributeByFullName(this Type type, string attributeFullName)
        {
            Attribute attribute = TypeDescriptor.GetAttributes(type).Cast<Attribute>().FirstOrDefault((Attribute a) => a.GetType().FullName == attributeFullName);
            if (attribute != null)
            {
                return attribute;
            }
            return null;
        }
    }
}
