using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class IListJSONSerialized
{
    public static JSONSerialized CreateArrayJSONSerialized(Type type)
    {
        if (!type.IsArray)
            throw new Exception($"{nameof(type)} : {type} is not Array type");
        JSONSerialized serialized;
        int arrayRank = type.GetArrayRank();
        Type elementType = type.GetElementType();
        if (arrayRank == 1)
        {
            serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                Array array = instance as Array;
                if (array == null)
                    return new JSONObject(JSONObject.Type.NULL);
                JSONObject jsArray = new JSONObject(JSONObject.Type.ARRAY);
                for (int index = 0; index < array.Length; index++)
                {
                    object item = array.GetValue(index);
                    jsArray.Add(JSONMap.ToJSON(elementType, item));
                }
                return jsArray;
            },
            (JSONObject @object) => 
            {
                if (@object is null || @object.IsNull)
                    return null;
                Array array = Array.CreateInstance(elementType, @object.Count);
                for (int index = 0; index < @object.Count; index++)
                {
                    object item = JSONMap.ParseJSON(elementType.FullName, @object[index]);
                    array.SetValue(item, index);
                }
                return array;
            });
        }
        else
        {
            serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                if (!(instance is Array array))
                    return new JSONObject(JSONObject.Type.NULL);
                JSONObject jsObject = new JSONObject(JSONObject.Type.OBJECT);
                int[] dimension = new int[arrayRank];
                JSONObject jsDimension = new JSONObject(JSONObject.Type.ARRAY);
                for (int index = 0; index < arrayRank; index++)
                {
                    int length = array.GetLength(index);
                    dimension[index] = length;
                    jsDimension.Add(length);
                }
                jsObject.SetField("dimension", jsDimension);

                JSONObject items = new JSONObject(JSONObject.Type.ARRAY);
                int[] lengthValue = new int[arrayRank];
                for (int index = 0; index < array.Length; index++)
                {
                    int indexValue = index;
                    for (int dimensionIndex = arrayRank - 1; dimensionIndex >= 0; dimensionIndex--)
                    {
                        int length = dimension[dimensionIndex];
                        lengthValue[dimensionIndex] = indexValue % length;
                        indexValue /= length;
                    }
                    object item = array.GetValue(lengthValue);
                    items.Add(JSONMap.ToJSON(elementType, item));
                }
                jsObject.SetField("items", items);
                return jsObject;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return null;
                JSONObject jsDimension = @object.GetField("dimension");
                JSONObject items = @object.GetField("items");
                int[] dimension = new int[jsDimension.Count];
                for (int index = 0; index < jsDimension.Count; index++)
                    dimension[index] = (int)jsDimension[index].i;
                Array array = Array.CreateInstance(elementType, dimension);
                int[] lengthValue = new int[arrayRank];
                for (int index = 0; index < items.Count; index++)
                {
                    int indexValue = index;
                    for (int dimensionIndex = arrayRank - 1; dimensionIndex >= 0; dimensionIndex--)
                    {
                        int length = dimension[dimensionIndex];
                        lengthValue[dimensionIndex] = indexValue % length;
                        indexValue /= length;
                    }
                    object item = JSONMap.ParseJSON(elementType.FullName, items[index]);
                    array.SetValue(item, lengthValue);
                }
                return array;
            });
        }
        return serialized;
    }

    public static JSONSerialized CreateListJSONSerialized(Type type, out Type genericArgument)
    {
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(List<>)))
            throw new Exception($"{nameof(type)} : {type} is not List<T> type");

        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length != 1)
            throw new Exception($"{nameof(type)} : {type} can not get generic arguments");
        Type genericArgument1 = genericArgument = genericArguments[0];
        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[1] {typeof(int) }, null);

        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                IList list = instance as IList;
                if (list == null)
                    return new JSONObject(JSONObject.Type.NULL);

                JSONObject @object = new JSONObject(JSONObject.Type.ARRAY);
                for (int index = 0; index < list.Count; index++)
                    @object.Add(JSONMap.ToJSON(genericArgument1, list[index]));
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return null;

                IList list = constructor.Invoke(new object[1] { @object.Count }) as IList;
                for (int index = 0; index < @object.Count; index++)
                {
                    object child = JSONMap.ParseJSON(genericArgument1.FullName, @object[index]);
                    list.Add(child);
                }
                return list;
            });
        return serialized;
    }

    public static JSONSerialized CreateHashSetJSONSerialized(Type type, out Type genericArgument)
    {
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(HashSet<>)))
            throw new Exception($"{nameof(type)} : {type} is not HashSet<T> type");

        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length != 1)
            throw new Exception($"{nameof(type)} : {type} can not get generic arguments");
        Type genericArgument1 = genericArgument = genericArguments[0];
        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[1] { typeof(int) }, null);
        MethodInfo addMethodInfo = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                IEnumerable enumerable = instance as IEnumerable;
                if (enumerable == null)
                    return new JSONObject(JSONObject.Type.NULL);

                JSONObject @object = new JSONObject(JSONObject.Type.ARRAY);
                foreach (object item in enumerable)
                {
                    @object.Add(JSONMap.ToJSON(genericArgument1, item));
                }
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return null;

                object hashSet = constructor.Invoke(new object[1] { @object.Count });
                object[] paramseters = new object[1];
                for (int index = 0; index < @object.Count; index++)
                {
                    paramseters[0] = JSONMap.ParseJSON(genericArgument1.FullName, @object[index]);
                    addMethodInfo.Invoke(hashSet, paramseters);
                }
                return hashSet;
            });
        return serialized;
    }
}