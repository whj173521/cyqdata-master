using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using CYQ.Data.Tool;

namespace CYQ.Data
{
    /// <summary>
    /// ���ݿ�������ʵ��
    /// </summary>
    internal partial class ConnBean
    {
        private ConnBean()
        {

        }
        /// <summary>
        /// ��Ӧ��ConnectionString��Name
        /// ������ԭʼ������������
        /// </summary>
        public string ConnName = string.Empty;
        /// <summary>
        /// ���ӵ�״̬�Ƿ�������
        /// </summary>
        public bool IsOK = true;
        /// <summary>
        /// �Ƿ�ӿ�
        /// </summary>
        public bool IsSlave = false;
        /// <summary>
        /// �Ƿ��ÿ�
        /// </summary>
        public bool IsBackup = false;
        /// <summary>
        /// ���Ӵ���ʱ���쳣��Ϣ��
        /// </summary>
        internal string ErrorMsg = string.Empty;
        /// <summary>
        /// ������ʽ��������ݿ������ַ���
        /// </summary>
        public string ConnString = string.Empty;
        /// <summary>
        /// ���ݿ�����
        /// </summary>
        public DataBaseType ConnDataBaseType;
        /// <summary>
        /// ���ݿ�汾��Ϣ
        /// </summary>
        public string Version;
        public ConnBean Clone()
        {
            ConnBean cb = new ConnBean();
            cb.ConnName = this.ConnName;
            cb.ConnString = this.ConnString;
            cb.ConnDataBaseType = this.ConnDataBaseType;
            cb.IsOK = this.IsOK;
            return cb;
        }
        public bool TryTestConn()
        {
            //err = string.Empty;
            if (!string.IsNullOrEmpty(ConnName) && AppConfig.GetAppBool("Conn.Result", true))
            {
                DalBase helper = DalCreate.CreateDal(ConnName);
                try
                {

                    helper.Con.Open();
                    Version = helper.Con.ServerVersion;
                    if (string.IsNullOrEmpty(Version))
                    {
                        Version = helper.DataBaseType.ToString();
                    }
                    helper.Con.Close();
                    IsOK = true;
                    ErrorMsg = string.Empty;
                }
                catch (Exception er)
                {
                    ErrorMsg = er.Message;
                    IsOK = false;
                }
                finally
                {
                    helper.Dispose();
                }
            }
            else
            {
                IsOK = false;
            }
            return IsOK;
        }
        public override int GetHashCode()
        {
            return Math.Abs(ConnString.Replace(" ", "").ToLower().GetHashCode());
        }
    }
    internal partial class ConnBean
    {
        /// <summary>
        /// �������ӵĶ��󼯺�
        /// </summary>
        private static MDictionary<int, ConnBean> connBeanDicCache = new MDictionary<int, ConnBean>();
        public static void ClearCache(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                int hash = key.GetHashCode();
                ConnBean cb = connBeanDicCache[hash];
                if (cb != null)
                {
                    connBeanDicCache.Remove(hash);
                    connBeanDicCache.Remove(cb.ConnName.GetHashCode());
                    connBeanDicCache.Remove(cb.ConnString.GetHashCode());
                }
            }
        }
        public static int GetHashCode(string conn)
        {
            ConnBean co = Create(conn);
            if (co == null)
            {
                if (conn == null) { return "".GetHashCode(); }
                return conn.GetHashCode();
            }
            return co.GetHashCode();
        }
        static readonly object o = new object();
        /// <summary>
        /// ����һ��ʵ����
        /// </summary>
        /// <returns></returns>
        public static ConnBean Create(string connNameOrString)
        {
            string connString = string.Format(AppConfig.GetConn(connNameOrString), AppConfig.WebRootPath);
            if (string.IsNullOrEmpty(connString))
            {
                return null;
            }
            //��⻺������ľ��
            int hash = connNameOrString.GetHashCode();
            lock (o)
            {
                if (connBeanDicCache.ContainsKey(hash))
                {
                    return connBeanDicCache[hash];
                }
                ConnBean cb = new ConnBean();
                cb.ConnName = connNameOrString;
                cb.ConnDataBaseType = GetDataBaseType(connString);
                cb.ConnString = RemoveConnProvider(cb.ConnDataBaseType, connString);
                connBeanDicCache.Add(hash, cb);
                hash = cb.ConnString.GetHashCode();
                if (!connBeanDicCache.ContainsKey(hash))
                {
                    connBeanDicCache.Add(hash, cb);//�����ݣ���connName,connStringΪkey
                }
                return cb;
            }
        }
        /// <summary>
        /// ȥ�� �����е� provider=xxxx;
        /// </summary>
        public static string RemoveConnProvider(DataBaseType dal, string connString)
        {
            if (dal != DataBaseType.Access)
            {
                string conn = connString.ToLower();
                int index = conn.IndexOf("provider");
                if (index > -1 && index < connString.Length - 5 && (connString[index + 8] == '=' || connString[index + 9] == '='))
                {
                    int end = conn.IndexOf(';', index);
                    if (end > index)
                    {
                        connString = connString.Remove(index, end - index + 1);
                    }
                }
            }
            return connString;
        }
        public static DataBaseType GetDataBaseType(string connString)
        {
            connString = connString.ToLower().Replace(" ", "");//ȥ���ո�

            #region �ȴ��������жϹ����
            if (connString.Contains("server=") && !connString.Contains("port="))
            {
                //server=.;database=xx;uid=xx;pwd=xx;
                return DataBaseType.MsSql;
            }
            if (connString.Contains("txtpath="))
            {
                // txt path={0}
                return DataBaseType.Txt;
            }
            if (connString.Contains("xmlpath="))
            {
                // xml path={0}
                return DataBaseType.Xml;
            }
            if (connString.Contains(".mdb") || connString.Contains(".accdb"))
            {
                //Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}App_Data/demo.mdb
                //Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}App_Data/demo.accdb
                return DataBaseType.Access;
            }
            if (connString.Contains(".db;") || connString.Contains(".db3;"))
            {
                //Data Source={0}App_Data/demo.db;failifmissing=false
                return DataBaseType.SQLite;
            }
            if (connString.Contains("provider=msdaora") || connString.Contains("provider=oraoledb.oracle")
               || connString.Contains("description=") || connString.Contains("fororacle"))
            {
                //Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT = 1521)))(CONNECT_DATA =(Sid = orcl)));User id=sa;password=123456
                return DataBaseType.Oracle;
            }

            #endregion

            #region �����򵥻�˿�ʶ��
            if (connString.Contains("provider=ase") || connString.Contains("port=5000"))
            {
                //data source=127.0.0.1;port=5000;database=cyqdata;uid=sa;pwd=123456
                return DataBaseType.Sybase;
            }
            if (connString.Contains("port=5432"))
            {
                ////server=.;port=5432;database=xx;uid=xx;pwd=xx;
                return DataBaseType.PostgreSQL;
            }
            if (connString.Contains("port=3306"))
            {
                //host=localhost;port=3306;database=mysql;uid=root;pwd=123456;Convert Zero Datetime=True;
                return DataBaseType.MySql;
            }
            #endregion

            if (connString.Contains("host=") && File.Exists(AppConfig.AssemblyPath + "MySql.Data.dll"))
            {
                return DataBaseType.MySql;
            }

            if (connString.Contains("datasource") &&
                (File.Exists(AppConfig.AssemblyPath + "Sybase.AdoNet2.AseClient.dll") || File.Exists(AppConfig.AssemblyPath + "Sybase.AdoNet4.AseClient.dll")))
            {
                return DataBaseType.Sybase;
            }
            //postgre��mssql���������һ����Ϊpostgre
            if (File.Exists(AppConfig.AssemblyPath + "Npgsql.dll"))
            {
                return DataBaseType.PostgreSQL;
            }

            return DataBaseType.MsSql;

        }

    }
}
