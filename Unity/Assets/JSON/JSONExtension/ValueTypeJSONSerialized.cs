using System;
using System.Collections.Generic;
using System.Reflection;

public static class ValueTypeJSONSerialized
{
    public static JSONSerialized CreateValueTypeJSONSerialized(Type type, List<FieldInfo> serializedFieldInfos)
    {
        if (!type.IsValueType)
            throw new Exception($"{nameof(type)} : {type} is not Value Type");
        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                JSONObject @object = new JSONObject(JSONObject.Type.OBJECT);
                foreach (FieldInfo fieldInfo in serializedFieldInfos)
                {
                    JSONObject jsField = JSONMap.ToJSON(fieldInfo.GetValue(instance));
                    @object.SetField(fieldInfo.Name, jsField);
                }
                return @object;
            },
            (JSONObject @object) =>
            {
                object instance = Activator.CreateInstance(type);
                foreach (FieldInfo fieldInfo in serializedFieldInfos)
                {
                    object field = JSONMap.ParseJSON(fieldInfo.FieldType.FullName, @object.GetField(fieldInfo.Name));
                    fieldInfo.SetValue(instance, field);
                }
                return instance;
            });
        return serialized;
    }
}