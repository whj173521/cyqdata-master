using System;
using System.Collections.Generic;
using System.Text;

namespace CYQ.Data
{ /// <summary>
    /// ���������ݿ�����
    /// </summary>
    public enum DataBaseType
    {
        None,
        /// <summary>
        /// MSSQL[2000/2005/2008/2012/...]
        /// </summary>
        MsSql,
        // Excel,
        Access,
        Oracle,
        MySql,
        SQLite,
        /// <summary>
        /// No Support Now
        /// </summary>
        // FireBird,
        /// <summary>
        /// PostgreSQL 
        /// </summary>
        PostgreSQL,
        /// <summary>
        /// Txt DataBase
        /// </summary>
        Txt,
        /// <summary>
        /// Xml DataBase
        /// </summary>
        Xml,
        Sybase
    }
    /// <summary>
    /// �����������[MProc SetCustom������ʹ�õĲ���]
    /// </summary>
    public enum ParaType
    {
        /// <summary>
        /// Oracle �α�����
        /// </summary>
        Cursor,
        /// <summary>
        /// �������
        /// </summary>
        OutPut,
        /// <summary>
        /// �����������
        /// </summary>
        InputOutput,
        /// <summary>
        /// ����ֵ����
        /// </summary>
        ReturnValue,
        /// <summary>
        /// Oracle CLOB����
        /// </summary>
        CLOB,
        /// <summary>
        ///  Oracle NCLOB����
        /// </summary>
        NCLOB,
        /// <summary>
        ///  MSSQL �û����������
        /// </summary>
        Structured
    }

    /// <summary>
    /// �������ݿ�Ľ��
    /// </summary>
    internal enum DBResetResult
    {
        /// <summary>
        ///  �ɹ��л� ���ݿ�����
        /// </summary>
        Yes,
        /// <summary>
        /// δ�л� - ��ͬ���ݿ�����
        /// </summary>
        No_SaveDbName,
        /// <summary>
        /// δ�л� - �����С�
        /// </summary>
        No_Transationing,
        /// <summary>
        /// δ�л� - �����ݿ��������ڡ�
        /// </summary>
        No_DBNoExists,
    }
    /// <summary>
    /// �������ӵļ���
    /// </summary>
    internal enum AllowConnLevel
    {
        Master = 1,
        MasterBackup = 2,
        MaterBackupSlave = 3
    }
}
