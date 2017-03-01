﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.VisualBasic;
using Table = System.Web.UI.WebControls.Table;
using static Extensions.DateTimeRoutines;
using System.Linq.Expressions;
using System.Dynamic;

namespace Extensions
{
    public static class Misc
    {
        public static bool IsNull(this object item)
        {
            if (item == null || System.Convert.IsDBNull(item)) return true;
            return false;
        }

        public static bool IsDefault<T>(this T value) where T : struct
        {
            return value.Equals(default(T));
        }

        public static TConvert ConvertTo<TConvert>(this object entity) where TConvert : new()
        {
            var convertProperties = TypeDescriptor.GetProperties(typeof(TConvert)).Cast<PropertyDescriptor>();
            var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();

            var convert = new TConvert();

            foreach (var entityProperty in entityProperties)
            {
                var property = entityProperty;
                var convertProperty = convertProperties.FirstOrDefault(prop => prop.Name == property.Name);
                if (convertProperty != null)
                {
                    var value = convertToPropType(convertProperty, entityProperty.GetValue(entity));
                    convertProperty.SetValue(convert, value);
                }
            }

            return convert;
        }

        public static object convertToPropType(PropertyDescriptor property, object value)
        {
            object cstVal = null;
            if (property != null)
            {
                Type propType = Nullable.GetUnderlyingType(property.PropertyType);
                bool isNullable = (propType != null);
                if (!isNullable) { propType = property.PropertyType; }
                //bool canAttrib = (value != null || isNullable);
                //if (!canAttrib) { throw new Exception("Cant attrib null on non nullable. "); }
                cstVal = (value == null || System.Convert.IsDBNull(value)) ? null : System.Convert.ChangeType(value, propType);
            }
            return cstVal;
        }

        /// <summary>
        /// Converts specified data to HEX string.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>Returns hex string.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
        public static string ToHex(this byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return BitConverter.ToString(data).ToLower().Replace("-", "");
        }

        public static bool IsWrapped<T>(this T[] data, T[] prefix, T[] suffix)
        {
            if (data.Length < prefix.Length + suffix.Length) return false;

            var end = data.SubArray(0, prefix.Length);

            if (!end.SequenceEqual(prefix)) return false;

            end = data.SubArray(data.Length - suffix.Length, suffix.Length);

            return end.SequenceEqual(suffix);
        }

        public static T[] Wrap<T>(this T[] data, T[] prefix, T[] suffix)
        {
            T[] result = new T[data.Length+prefix.Length+suffix.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
            Buffer.BlockCopy(suffix, 0, result, prefix.Length+data.Length, suffix.Length);
            return result;
        }

        public static T[] Strip<T>(this T[] data, int left, int right)
        {
            T[] result = new T[data.Length - (left + right)];
            Buffer.BlockCopy(data, left, result, 0, result.Length);
            return result;
        }

        public static T[] Copy<T>(this T[] data)
        {
            T[] result = new T[data.Length];
            Buffer.BlockCopy(data, 0, result, 0, result.Length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            if (length > data.Length) length = data.Length;

            T[] result = new T[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }

        public static bool ByteEquals(this byte[] source, byte[] bytes)
        {
            if (source==null || bytes==null)return bytes==source;
            if (source.Length != bytes.Length) return false;
            return source.SequenceEqual<byte>(bytes);
        }

        public static byte[] ToBytes(this Stream stream)
        {
            using(var memoryStream = new MemoryStream())
            {
              stream.CopyTo(memoryStream);
              return memoryStream.ToArray();
            }
        }

        public static bool EqualsI(this object item, params object[] values)
        {
            if (item == null && values.Contains(null)) return true;
            foreach (var value in values)
                if (item.Equals(value)) return true;
            return false;
        }

        public static bool IsAny(this object item, params object[] values)
        {
            if (item == null && values.Contains(null)) return true;
            foreach (var value in values)
                if (item.Equals(value)) return true; 
            return false;
        }

        public static bool IsAny(this char text, params char[] chars)
        {
            foreach (var ch in chars)
            {
                if (text.Equals(ch))
                    return true;
            }
            return false;
        }

        public static bool IsAny(this string text, bool ignoreCase, params string[] values)
        {
            foreach (var value in values)
            {
                if (text.Equals(value) || (ignoreCase && text.EqualsI(value)))
                return true;
            }
            return false;
        }

        public static Guid ToGuid(this object text)
        {
            if (text.IsNull()) return Guid.Empty;
            if (text is Guid) return (Guid) text;
            var str = System.Convert.ToString(text);
            var parsedValue = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(str) && Guid.TryParse(str, out parsedValue)) return parsedValue;
            return Guid.Empty;
        }

        public static byte[] ToBytes(this object text)
        {
            if (!text.IsNull() && text is byte[]) return (byte[])text;
            return null;
        }

        public static bool ToBoolean(this object text, bool defaultValue = false)
        {
            if (text.IsNull()) return defaultValue;
            if (text is bool) return (bool)text;
            var str = System.Convert.ToString(text);
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (str.EqualsI("1","yes")) return true;
                if (str.EqualsI("0","no")) return false;
                bool parsedValue;
                if (bool.TryParse(str, out parsedValue)) return parsedValue;
            }
            return defaultValue;
        }

        public static byte ToByte(this object text, byte defaultValue = 0)
        {
            if (text.IsNull()) return defaultValue;
            if (text is byte || text is decimal || text is double || text is int || text is long || text.GetType().IsEnum) return System.Convert.ToByte(text);
            var str = System.Convert.ToString(text);
            byte parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && byte.TryParse(str, out parsedValue)) return parsedValue;
            return defaultValue;
        }

        public static int ToInt32(this object text, int defaultValue=0)
        {
            if (text.IsNull()) return defaultValue;
            if (text is decimal || text is double || text is int || text is long || text is byte || text.GetType().IsEnum) return System.Convert.ToInt32(text);
            var str = System.Convert.ToString(text);
            int parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && int.TryParse(str, out parsedValue)) return parsedValue;
            return defaultValue;
        }

        public static long ToInt64(this object text, long defaultValue = 0)
        {
            if (text.IsNull()) return defaultValue;
            if (text is decimal || text is double || text is int || text is long || text is byte|| text.GetType().IsEnum) return System.Convert.ToInt64(text);
            var str = System.Convert.ToString(text);
            long parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && long.TryParse(str, out parsedValue)) return parsedValue;
            return defaultValue;
        }

        public static double ToDouble(this object text, double defaultValue = 0)
        {
            if (text.IsNull()) return defaultValue;
            if (text is decimal || text is double || text is int || text is long || text is byte || text.GetType().IsEnum) return System.Convert.ToDouble(text);
            var str = System.Convert.ToString(text);
            double parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && double.TryParse(str, out parsedValue)) return parsedValue;
            return defaultValue;
        }

        public static decimal ToDecimal(this object text, decimal defaultValue = 0)
        {
            if (text.IsNull()) return defaultValue;
            if (text is decimal || text is double || text is int || text is long || text is byte || text.GetType().IsEnum) return System.Convert.ToDecimal(text);
            var str = System.Convert.ToString(text);
            decimal parsedValue;
            if (!string.IsNullOrWhiteSpace(str) && decimal.TryParse(str, out parsedValue)) return parsedValue;
            return defaultValue;
        }

        public static TimeSpan ToTimeSpan(this object text,DateInterval defaultUnit=DateInterval.Second)
        {
            if (text.IsNull()) return TimeSpan.Zero;
            if (text is TimeSpan) return (TimeSpan)text;
            var str = System.Convert.ToString(text);
            if (string.IsNullOrWhiteSpace(str)) return TimeSpan.Zero;

            double units;
            if (str.IsRealNumber() && double.TryParse(str, out units))
            {
                switch (defaultUnit)
                {
                    case DateInterval.Day:
                        return TimeSpan.FromDays(units);
                    case DateInterval.Hour:
                        return TimeSpan.FromHours(units);
                    case DateInterval.Minute:
                        return TimeSpan.FromMinutes(units);
                    case DateInterval.Second:
                        return TimeSpan.FromSeconds(units);
                    default:
                        throw new FormatException("Invalid default '" + defaultUnit + "' - units must be days, hours, minutes, or seconds");
                }
            }
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(str, out timeSpan)) return timeSpan;
            throw new FormatException("Interval must be in format [d.]hh:mm[:ss] or an integer of total " + defaultUnit);
        }

        public static string ToStringOrNull(this object text)
        {
            string result = null;
            if (text is string)
                result = (string)text;
            else if (!text.IsNull())
                result = System.Convert.ToString(text);

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        public static string ToStringOrEmpty(this object text)
        {
            string result = null;
            if (text is string)
                result = (string)text;
            else if (!text.IsNull())
                result = System.Convert.ToString(text);

            return string.IsNullOrWhiteSpace(result) ? string.Empty : result;
        }

        public static string ToStringOr(this object text, string replacement)
        {
            string result = null;
            if (text is string)
                result = (string)text;
            else if (!text.IsNull())
                result = System.Convert.ToString(text);

            return string.IsNullOrWhiteSpace(result) ? replacement : result;
        }

        public static DateTime ToDateTime(this object text)
        {
            if (text.IsNull())return DateTime.MinValue;
            if (text is DateTime) return (DateTime)text;
            var str = System.Convert.ToString(text);
            if (!string.IsNullOrWhiteSpace(str))
            {
                DateTime parsedValue;
                if (DateTime.TryParseExact(str, Time.ShortDateFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out parsedValue))
                    return parsedValue;

                if (DateTime.TryParseExact(str, Time.ShortDateFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out parsedValue))
                    return parsedValue;

                if (DateTime.TryParse(str, out parsedValue)) return parsedValue;
            }
            return DateTime.MinValue;
        }

        public static T ToEnum<T>(this object input, T defaultValue = default(T)) where T : struct,IComparable
        {
            if (!typeof(T).IsEnum)throw new ArgumentException("Type T must be an Enum");

            if (input.IsNull()) return defaultValue;

            if (input is T) return (T)input;

            var str = System.Convert.ToString(input);

            if (string.IsNullOrWhiteSpace(str)) return defaultValue;

            T parsedValue = defaultValue;
            if (Enum.TryParse(str, out parsedValue)) return parsedValue;

            return defaultValue;
        }


        public static T Parse<T>(this object input, T defaultValue = default(T)) where T : struct,IComparable
        {
            if (input is T) return (T)input;

            var result = defaultValue;

            var str = System.Convert.ToString(input);

            if (string.IsNullOrWhiteSpace(str)) return result;

            if (typeof(T).IsEnum && Enum.TryParse<T>(str, out result)) return result;

            var converter = TypeDescriptor.GetConverter(typeof(T));

            return (T)converter.ConvertFromString(str);
        }

        public static T Convert<T>(this object input, T defaultValue = default(T))
        {
            if (input is T || input != null) return (T)input;
            return defaultValue;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this TSource source,Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
        public static object GetPropertyValue(object Object, string PropertyName)
        {
            try
            {
                var myInfo = Object.GetType().GetPropertyInfo(PropertyName);
                return myInfo.GetValue(Object, null);
            }
            catch { }
            return null;
        }

        public static PropertyInfo GetPropertyInfo(this Type type,string propertyName)
        {
            try
            {
                var i = propertyName.IndexOf(".");
                if (i > -1)return type.GetType().GetProperty(propertyName.Substring(0, i));
                return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);


            }
            catch 
            {

            }
            return null;
        }

        public static dynamic Merge(object item1, object item2)
        {
            if (item1 == null || item2 == null)
                return item1 ?? item2 ?? new ExpandoObject();

            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;
            foreach (System.Reflection.PropertyInfo fi in item1.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item1, null);
            }
            foreach (System.Reflection.PropertyInfo fi in item2.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item2, null);
            }
            return result;
        }

        public static Dictionary<string, object> ToPropertyDictionary(this object obj, StringComparer comparer=null, bool newIfEmpty = false)
        {
            if (comparer == null) comparer = StringComparer.CurrentCultureIgnoreCase;
            if (obj == null) return newIfEmpty ? new Dictionary<string, object>(comparer) : null; 
            return obj.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(obj, null),comparer);
        }

        public static bool SetPropertyValue(object obj, string propertyName, object value)
        {
            PropertyInfo myInfo = null;
            var myType = obj.GetType();
            try
            {
                myInfo = GetPropertyInfo(myType, propertyName);
                if (myInfo == null) return false;

                myInfo.SetValue(obj, value, null);
                return true;
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }
            return false;
        }

        public static void CopyProperties(this object source, object target)
        {
            var targetType = target.GetType();
            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                var propGetter = sourceProperty.GetGetMethod();
                var targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty == null) continue;
                var propSetter = targetProperty.GetSetMethod();
                var valueToSet = propGetter.Invoke(source, null);
                propSetter.Invoke(target, new[] { valueToSet });
            }
        }

        public static void HideColumn(this Table table, int index)
        {
            foreach (TableRow row in table.Rows)
            {
                if (row.Cells.Count - 1 >= index && row.Cells[index].ColumnSpan==0)
                {
                    row.Cells[index].Visible = false;
                }
            }
        }

        public static void HideColumn(this Table table, string id)
        {
            foreach (TableRow row in table.Rows)
            {
                var headerRow = row as TableHeaderRow;
                if (headerRow == null) return;
                    
                for (var i=0;i<headerRow.Cells.Count;i++)
                {
                    var cell = headerRow.Cells[i] as TableHeaderCell;
                    if (cell.ID.EqualsI(id))
                    {
                        HideColumn(table, i);;
                        return;
                    }

                }                    
            }
        }
    }
}
