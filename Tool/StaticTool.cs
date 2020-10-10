using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using CYQ.Data.Table;
using CYQ.Data.SQL;
using System.IO;
using System.Web;
using System.Threading;


namespace CYQ.Data.Tool
{


    /// <summary>
    /// ��̬����������
    /// </summary>
    internal static class StaticTool
    {
        /// <summary>
        /// ��GUIDת��16�ֽ��ַ���
        /// </summary>
        /// <returns></returns>
        internal static string ToGuidByteString(string guid)
        {
            return BitConverter.ToString(new Guid(guid).ToByteArray()).Replace("-", "");
        }

        static MDictionary<string, string> hashKeyCache = new MDictionary<string, string>(32);
        internal static string GetHashKey(string sourceString)
        {
            try
            {
                if (hashKeyCache.ContainsKey(sourceString))
                {
                    return hashKeyCache[sourceString];
                }
                else
                {
                    if (hashKeyCache.Count > 512)
                    {
                        hashKeyCache.Clear();
                        hashKeyCache = new MDictionary<string, string>(64);
                    }
                    string value = "K" + Math.Abs(sourceString.GetHashCode()) + sourceString.Length;
                    hashKeyCache.Add(sourceString, value);
                    return value;
                }
            }
            catch
            {
                return sourceString;
            }
        }
        /// <summary>
        /// ���ڱ�ʶ�����û�Ϊ��λ���� ���� ��Ψһ��ʶ
        /// </summary>
        /// <returns></returns>
        public static string GetMasterSlaveKey()
        {
            string id = string.Empty;
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    id = HttpContext.Current.Session.SessionID;
                }
                else if (HttpContext.Current.Request["Token"] != null)
                {
                    id = HttpContext.Current.Request["Token"];
                }
                else if (HttpContext.Current.Request.Headers["Token"] != null)
                {
                    id = HttpContext.Current.Request.Headers["Token"];
                }
                else if (HttpContext.Current.Request["MasterSlaveid"] != null)
                {
                    id = HttpContext.Current.Request["MasterSlaveid"];
                }
                if (string.IsNullOrEmpty(id))
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies["MasterSlaveid"];
                    if (cookie != null)
                    {
                        id = cookie.Value;
                    }
                    else
                    {
                        id = Guid.NewGuid().ToString().Replace("-", "");
                        cookie = new HttpCookie("MasterSlaveid", id);
                        cookie.Expires = DateTime.Now.AddMonths(1);
                        HttpContext.Current.Response.Cookies.Add(cookie);
                    }
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                id = DateTime.Now.Minute + Thread.CurrentThread.ManagedThreadId.ToString();
            }
            return "MasterSlave_" + id;
        }
        /// <summary>
        /// ���ڱ�ʶ�����û�Ϊ��λ���� ȫ������ ��Ψһ��ʶ
        /// </summary>
        public static string GetTransationKey(string conn)
        {
            string key = Thread.CurrentThread.ManagedThreadId.ToString();
            if (HttpContext.Current != null)
            {
                string id = string.Empty;
                if (HttpContext.Current.Session != null)
                {
                    id = HttpContext.Current.Session.SessionID;
                }
                else if (HttpContext.Current.Request["Token"] != null)
                {
                    id = HttpContext.Current.Request["Token"];
                }
                else if (HttpContext.Current.Request.Headers["Token"] != null)
                {
                    id = HttpContext.Current.Request.Headers["Token"];
                }
                key = id + key;
            }
            int hash = ConnBean.GetHashCode(conn);
            return "Transation_" + key + hash;
        }
    }
}
