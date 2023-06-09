using System;
using System.Collections.Generic;

namespace Table
{
    /// <summary>
    /// 本地化字符串数据类型独立化
    /// </summary>
    public struct StringID
    {
        /* field */
        public string Key;
        public string Localization => TableManager.LocalizationTable[Key];

        /* ctor */
        public StringID(string key)
        {
            Key = key;
        }

        /* func */
        public override string ToString() => Localization;

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (instance is StringID stringID)
                return JSONObject.CreateStringObject(stringID.Key);
            else
                return new JSONObject(JSONObject.Type.STRING);
        }
        public static object ParseJSON(JSONObject @object)
        {
            StringID stringID = new StringID();
            if (@object.type == JSONObject.Type.STRING)
                stringID.Key = @object.str;
            return stringID;
        }

        public override bool Equals(object obj)
        {
            return obj is StringID iD &&
                   Key == iD.Key;
        }

        public override int GetHashCode()
        {
            return 990326508 + EqualityComparer<string>.Default.GetHashCode(Key);
        }
        #endregion
    }
}