﻿using CYQ.Data.SQL;
using CYQ.Data.Table;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CYQ.Data.Tool
{
    /// <summary>
    /// 类型转换（支持json转实体）
    /// </summary>
    public static class ConvertTool
    {
        /// <summary>
        /// 类型转换(精准强大)
        /// </summary>
        /// <param name="value">值处理</param>
        /// <param name="t">类型</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type t)
        {
            if (t == null)
            {
                return null;
            }
            if (t.FullName == "System.Type")
            {
                return (Type)value;
            }
            string strValue = Convert.ToString(value);
            if (t.IsGenericType && t.Name.StartsWith("Nullable"))
            {
                t = Nullable.GetUnderlyingType(t);
                if (strValue == "")
                {
                    return null;
                }
            }
            if (t.Name == "String")
            {
                if (value is byte[])
                {
                    return Convert.ToBase64String((byte[])value);
                }
                return strValue;
            }
            if (t.FullName == "System.Text.StringBuilder")
            {
                return value as StringBuilder;
            }
            if (t.FullName == "System.Text.Encoding")
            {
                return value as Encoding;
            }
            if (strValue == "")
            {
                return Activator.CreateInstance(t);
            }
            else if (t.IsValueType)
            {
                if (t.Name == "DateTime")
                {
                    switch (strValue.ToLower().TrimEnd(')', '('))
                    {
                        case "now":
                        case "getdate":
                        case "current_timestamp":
                            return DateTime.Now;
                    }
                    if (DateTime.Parse(strValue) == DateTime.MinValue)
                    {
                        return (DateTime)SqlDateTime.MinValue;
                    }
                    return Convert.ChangeType(value, t);//这里用value，避免丢失毫秒
                }
                if (t.Name == "Guid")
                {
                    if (strValue == SqlValue.Guid || strValue.StartsWith("newid"))
                    {
                        return Guid.NewGuid();
                    }
                    return new Guid(strValue);
                }
                else if (t.Name.StartsWith("Int"))
                {
                    switch (strValue.ToLower())
                    {
                        case "true":
                            return 1;
                        case "false":
                            return 0;
                    }
                    if (strValue.IndexOf('.') > -1)
                    {
                        strValue = strValue.Split('.')[0];
                    }
                    else if (value.GetType().IsEnum)
                    {
                        return (int)value;
                    }
                }
                else if (t.Name == "Double" || t.Name == "Single")
                {
                    switch (strValue.ToLower())
                    {
                        case "infinity":
                        case "正无穷大":
                            return double.PositiveInfinity;
                        case "-infinity":
                        case "负无穷大":
                            return double.NegativeInfinity;
                    }
                }
                else if (t.Name == "Boolean")
                {
                    switch (strValue.ToLower())
                    {
                        case "yes":
                        case "true":
                        case "1":
                        case "on":
                        case "是":
                            return true;
                        case "no":
                        case "false":
                        case "0":
                        case "":
                        case "否":
                        default:
                            return false;
                    }
                }
                else if (t.IsEnum)
                {

                    return Enum.Parse(t, strValue, true);
                }
                return Convert.ChangeType(strValue, t);
            }
            else
            {
                Type valueType = value.GetType();
                //if(valueType.IsEnum && t.is)

                if (valueType.FullName != t.FullName)
                {
                    switch (ReflectTool.GetSystemType(ref t))
                    {
                        case SysType.Custom:

                            return MDataRow.CreateFrom(strValue).ToEntity(t);
                        case SysType.Generic:
                            if (t.Name.StartsWith("List"))
                            {
                                return MDataTable.CreateFrom(strValue).ToList(t);
                            }
                            break;
                        case SysType.Array:
                            if (t.Name == "Byte[]")
                            {
                                if (valueType.Name == "String")
                                {
                                    return Convert.FromBase64String(strValue);
                                }
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    new BinaryFormatter().Serialize(ms, value);
                                    return ms.ToArray();
                                }
                            }
                            break;
                    }
                }
                return Convert.ChangeType(value, t);
            }
        }

        /// <summary>
        /// 类型转换(精准强大)
        /// </summary>
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(value, typeof(T));
        }

    }
}
