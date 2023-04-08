using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

public static class ObjectSerialized
{
    public static JSONSerialized CreateObjectDefaultSerialized(Type type, List<FieldInfo> serializedFieldInfos)
    {
        ConstructorInfo selectConstructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
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
                object instance;
                if (selectConstructorInfo != null)
                    instance = selectConstructorInfo.Invoke(new object[0]);
                else
                    instance = FormatterServices.GetUninitializedObject(type);
                foreach (FieldInfo fieldInfo in serializedFieldInfos)
                {
                    object field = JSONMap.ParseJSON(fieldInfo.FieldType.FullName, @object.GetField(fieldInfo.Name));
                    fieldInfo.SetValue(instance, field);
                }
                return instance;
            });
        serialized.Fields.AddRange(serializedFieldInfos);
        return serialized;
    }
}