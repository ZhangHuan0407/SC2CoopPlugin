using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace Table
{
    /// <summary>
    /// 所有的数据表
    /// </summary>
    public static class TableManager
    {
        /* field */
        private static DescribeTable m_DescribeTable;
        public static DescribeTable DescribeTable
        {
            get => m_DescribeTable;
        }


        /* func */
        public static void LoadInnerTables()
        {

        }

        public static void LoadDefaultDescribeTable() => LoadDescribeTable(Application.systemLanguage);
        public static void LoadDescribeTable(SystemLanguage systemLanguage)
        {


        }
    }
}