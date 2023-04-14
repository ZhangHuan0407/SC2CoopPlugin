using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;

namespace Table
{
    /// <summary>
    /// 描述文字
    /// </summary>
    public class LocalizationTable
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
        [NonSerialized]
        public readonly Dictionary<string, Entry> Data;

        /* ctor */
        public LocalizationTable()
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
            set
            {
                Entry entry = new Entry()
                {
                    ID = ID,
                    StringValue = WebUtility.HtmlDecode(value),
                };
            }
        }

        /* func */
        public string Format(string ID, params string[] values) => string.Format(this[ID], values);

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (!(instance is LocalizationTable table))
                return new JSONObject(JSONObject.Type.NULL);
            JSONObject @object = new JSONObject(JSONObject.Type.OBJECT);
            foreach (Entry entry in table.Data.Values)
                @object.AddField(entry.ID, JSONObject.CreateStringObject(entry.StringValue));
            return @object;
        }
        public static object ParseJSON(JSONObject @object)
        {
            if (@object is null || @object.IsNull)
                return null;
            LocalizationTable table = new LocalizationTable();
            foreach (var pair in @object.dictionary)
                table[pair.Key] = pair.Value.str;
            return table;
        }
        #endregion
    }
}