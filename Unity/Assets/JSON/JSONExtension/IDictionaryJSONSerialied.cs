using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class IDictionaryJSONSerialized
{
    public static JSONSerialized CreateIDictionaryJSONSerialized(Type type, out Type keyType, out Type valueType)
    {
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>)))
            throw new Exception($"{nameof(type)} : {type} is not Dictionary<TKey, TValue> type");

        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length != 2)
            throw new Exception($"{nameof(type)} : {type} can not get generic arguments");

        Type keyType1 = keyType = genericArguments[0];
        Type valueType1 = valueType = genericArguments[1];
        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                IDictionary dictionary = instance as IDictionary;
                if (dictionary == null)
                    return new JSONObject(JSONObject.Type.NULL);

                JSONObject jsKeySet = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject jsValueSet = new JSONObject(JSONObject.Type.ARRAY);
                foreach (object key in dictionary.Keys)
                {
                    jsKeySet.Add(JSONMap.ToJSON(keyType1, key));
                    jsValueSet.Add(JSONMap.ToJSON(valueType1, dictionary[key]));
                }
                JSONObject @object = new JSONObject(JSONObject.Type.OBJECT);
                @object.SetField("Keys", jsKeySet);
                @object.SetField("Values", jsValueSet);
                @object.SetField("Count", dictionary.Count);
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return null;

                IDictionary dictionary = constructor.Invoke(new object[0]) as IDictionary;
                JSONObject jsKeySet = @object.GetField("Keys");
                JSONObject jsValueSet = @object.GetField("Values");
                JSONObject count = @object.GetField("Count");
                for (int index = 0; index < count.i; index++)
                {
                    object key = JSONMap.ParseJSON(keyType1.FullName, jsKeySet[index]);
                    object value = JSONMap.ParseJSON(valueType1.FullName, jsValueSet[index]);
                    dictionary[key] = value;
                }
                return dictionary;
            });
        return serialized;
    }
}