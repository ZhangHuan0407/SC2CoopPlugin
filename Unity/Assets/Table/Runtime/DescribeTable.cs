using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Table
{
    /// <summary>
    /// 描述文字
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public class DescribeTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public struct Entry
        {
            /// <summary>
            /// 字符串ID
            /// </summary>
            public string ID;
            /// <summary>
            /// 字符串描述字面量
            /// </summary>
            public string StringValue;

            public override int GetHashCode() => ID.GetHashCode();
            public override bool Equals(object obj)
            {
                if (obj is Entry entry)
                    return ID.Equals(entry.ID);
                else
                    return false;
            }
        }

        /* field */
        [SerializeField]
        private Entry[] m_Entries;
        public Entry[] Entries
        {
            get => m_Entries;
        }

        [NonSerialized]
        public readonly Dictionary<string, Entry> Data;

        /* ctor */
        public DescribeTable()
        {
            Data = new Dictionary<string, Entry>();
        }

        /* inter */
        public string this[string ID]
        {
            get
            {
                if (Data.TryGetValue(ID, out Entry entry))
                    return entry.StringValue;
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Not found DescribeData, ID : {ID}");
#endif
                    return ID;
                }
            }
        }
        public object GetValue(string key) => this[key];

        /* func */
        public string UseFormat(string ID, params string[] values) => string.Format(this[ID], values);

        #region Serialized
        public static JSONObject ToJSON(object instance) => JSONMap.FieldsToJSON(instance, null);
        public static object ParseJSON(JSONObject @object)
        {
            DescribeTable describeTable = new DescribeTable();
            JSONMap.FieldsParseJSON(describeTable, @object);
            for (int index = 0; index < describeTable.Entries.Length; index++)
            {
                describeTable.Entries[index].StringValue = WebUtility.HtmlDecode(describeTable.Entries[index].StringValue);
                Entry entry = describeTable.Entries[index];
                describeTable.Data.Add(entry.ID, entry);
            }
            return describeTable;
        }
        #endregion
    }
}