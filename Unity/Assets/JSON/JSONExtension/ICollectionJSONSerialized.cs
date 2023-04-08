using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class ICollectionJSONSerialized
{
    public static bool TryCreateCollectionJSONSerialized(Type type, out Type genericArgument, out JSONSerialized serialized)
    {
        Type genericType = type.GetGenericTypeDefinition();
        if (genericType.Equals(typeof(LinkedList<>)))
        {
            serialized = CreateLinkedListJSONSerialized(type, out genericArgument);
            return true;
        }
        serialized = default;
        genericArgument = null;
        return false;
    }
    private static JSONSerialized CreateCollectionJSONSerialized_Internal(Type type, Action<object, object> addItem, Type genericArgument)
    {
        if (addItem is null)
            throw new ArgumentNullException(nameof(addItem));
        if (!type.IsGenericType)
            throw new ArgumentException($"{nameof(type)} : {type.Name} is not Generic type");

        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

        JSONSerialized serialized = new JSONSerialized(
            type,
            (object instance) =>
            {
                ICollection collection = instance as ICollection;
                if (collection == null)
                    return new JSONObject(JSONObject.Type.NULL);

                JSONObject @object = new JSONObject(JSONObject.Type.ARRAY);
                foreach (object item in collection)
                    @object.Add(JSONMap.ToJSON(genericArgument, item));
                return @object;
            },
            (JSONObject @object) =>
            {
                if (@object is null || @object.IsNull)
                    return null;

                object collection = constructor.Invoke(new object[0]);
                for (int index = 0; index < @object.Count; index++)
                {
                    object item = JSONMap.ParseJSON(genericArgument.FullName, @object[index]);
                    addItem(collection, item);
                }
                return collection;
            });
        return serialized;
    }

    public static JSONSerialized CreateLinkedListJSONSerialized(Type type, out Type genericArgument)
    {
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(LinkedList<>)))
            throw new Exception($"{nameof(type)} : {type.Name} is not LinkedList<T> type");

        Type[] genericArguments = type.GetGenericArguments();
        if (genericArguments.Length != 1)
            throw new Exception($"{nameof(type)} : {type.Name} can not get generic arguments");
        Type genericArgument1 = genericArgument = genericArguments[0];

        MethodInfo addMethodInfo = type.GetMethod("AddLast", new Type[1] { genericArgument1 });
        return CreateCollectionJSONSerialized_Internal(
            type,
            (object list, object item) => { addMethodInfo?.Invoke(list, new object[] { item }); },
            genericArgument);
    }
    //public static JSONSerialized CreateHashSetJSONSerialized(Type type, out Type genericArgument)
    //{
    //    if (!type.IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(HashSet<>)))
    //        throw new Exception($"{nameof(type)} : {type.Name} is not HashSet<T> type");

    //    MethodInfo addMethodInfo = type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
    //    return CreateCollectionJSONSerialized(
    //        type,
    //        (object list, object item) => { addMethodInfo?.Invoke(list, new object[] { item }); },
    //        out genericArgument);
    //}
}