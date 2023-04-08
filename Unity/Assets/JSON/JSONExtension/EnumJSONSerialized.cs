using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumJSONSerialized
{
    public static JSONSerialized? CreateEnumJSONSerialized(Type type)
    {
        if (!type.IsEnum)
            throw new Exception($"{nameof(type)} : {type} is not Enum type");
        // 这是个什么玩意?
        // UnityEditor.UIElements.PropertyControl<UnityEditor.UIElements.DataType[TType]>
        if (type.IsGenericType)
            return null;
        string[] names = Enum.GetNames(type);
        Array values = type.GetEnumValues();
        object defaultValue = values.Length > 0 ? values.GetValue(0) : null;
        Dictionary<string, object> nameToVallue = new Dictionary<string, object>();
        Dictionary<object, string> vallueToName = new Dictionary<object, string>();
        for (int index = 0; index < names.Length; index++)
        {
            // 重复 key 问题，只能尽力解决
            nameToVallue[names[index]] = values.GetValue(index);
            vallueToName[values.GetValue(index)] = names[index];
        }
        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                JSONObject @object = new JSONObject(JSONObject.Type.STRING);
                @object.str = vallueToName[instance];
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return defaultValue;
                return nameToVallue[@object.str];
            });
        return serialized;
    }
}