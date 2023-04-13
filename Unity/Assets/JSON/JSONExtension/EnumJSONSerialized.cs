using System;
using System.Collections;
using System.Collections.Generic;

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
        object defaultValue = GetDefaultValue(type);
        Dictionary<string, object> nameToValue = new Dictionary<string, object>();
        Dictionary<object, string> valueToName = new Dictionary<object, string>();
        for (int index = 0; index < names.Length; index++)
        {
            // 重复 key 问题，只能尽力解决
            nameToValue[names[index]] = values.GetValue(index);
            valueToName[values.GetValue(index)] = names[index];
        }
        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                JSONObject @object = new JSONObject(JSONObject.Type.STRING);
                @object.str = valueToName[instance];
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return defaultValue;
                return nameToValue[@object.str];
            });
        return serialized;
    }
    public static JSONSerialized? CreateEnumFlagsJSONSerialized(Type type)
    {
        if (!type.IsEnum)
            throw new Exception($"{nameof(type)} : {type} is not Enum type");
        if (type.IsGenericType)
            return null;
        object defaultValue = GetDefaultValue(type);
        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                JSONObject @object = new JSONObject(JSONObject.Type.STRING);
                @object.str = instance.ToString();
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return defaultValue;
                return Enum.Parse(type, @object.str);
            });
        return serialized;
    }
    private static object GetDefaultValue(Type type)
    {
        Type underlyingType = Enum.GetUnderlyingType(type);
        object value = Convert.ChangeType(0, underlyingType);
        return value;
    }
}