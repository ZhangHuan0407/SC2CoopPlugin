// 单线程执行时，如果没有注册此类型，将自动注册
// #define STAThread
// 如果引用类型没有公开静态的序列化支持，将使用强制序列化
#define ForceSerialize

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
#if UNITY_STANDALONE
using UnityEngine;
#endif
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

public static class JSONMap
{
    private static readonly Dictionary<string, JSONSerialized> Map;
    private static readonly HashSet<Type> Blacklist;

    static JSONMap()
    {
        Map = new Dictionary<string, JSONSerialized>(5000);
        Blacklist = new HashSet<Type>();
#if UNITY_EDITOR
        // Unity Editor下，直接使用JSONMap时，自动初始化
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            RegisterDefaultType();
            RegisterAllTypes();
        }
#endif
    }

    /// <summary>
    /// Loading Thread 调用此方法，完成类初始化
    /// </summary>
    public static void CallFromLoadingThread()
    {
        _ = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// 此类型对象都将跳过不进行序列化
    /// </summary>
    public static void AddBlackList(IEnumerable<Type> types) => Blacklist.UnionWith(types);

    public static void RegisterDefaultType()
    {
        RegisterType(BasicValueJSONSerialized.BoolSerialized);
        RegisterType(BasicValueJSONSerialized.FloatSerialized);
        RegisterType(BasicValueJSONSerialized.IntSerialized);
        RegisterType(BasicValueJSONSerialized.LongSerialized);
        RegisterType(BasicValueJSONSerialized.StringSerialized);
        RegisterType(BasicValueJSONSerialized.DateTimeSerialized);
#if UNITY_STANDALONE
        RegisterType(VectorJSONSerialized.Vector4Serialized);
        RegisterType(VectorJSONSerialized.Vector3IntSerialized);
        RegisterType(VectorJSONSerialized.Vector3Serialized);
        RegisterType(VectorJSONSerialized.Vector2IntSerialized);
        RegisterType(VectorJSONSerialized.Vector2Serialized);
#endif
    }
    public static void RegisterAllTypes()
    {
        HashSet<Type> selectTypes = new HashSet<Type>();
        Stack<Type> needVisitTypes = new Stack<Type>();
        // 程序集黑名单
        HashSet<string> blackAssembly = new HashSet<string>()
        {
            "Mono.Security",
            "System.ComponentModel.DataAnnotations",
            "System.Data",
            "System.Drawing",
            "System.Resources.ResourceManager.",
            "System.Runtime.Serialization.",
            "System.Transactions",
            "System.Windows.Forms",
            "System.Web",
            "System.Xml",
            "System.Xml.Linq",
            "netstandard",
            "nunit.framework",
            "ICSharpCode.NRefactory",
            "DOTween",

            "Anonymously Hosted DynamicMethods Assembly",
            "log4net",
            "unityplastic",
            "Unity.Cecil",
            "UnityEditor.TestRunner",
            "ExCSS.Unity",
        };
        HashSet<string> unityEngineAssembly = new HashSet<string>()
        {
        };
        // 命名空间黑名单，平台差异性
        HashSet<string> namespaceBlacklist0 = new HashSet<string>()
        {
            "Mono",
            "XamMac",
            "Microsoft",
            "XLua",
        };
        // 命名空间黑名单，数据序列化无意义
        List<string> namespaceBlacklist1 = new List<string>()
        {
            "System.ComponentModel.",
            "System.Data.",
            "System.Diagnostics.",
            "System.IO.",
            "System.Threading.",
            "System.Text.",
            "System.Dynamic.",
            "System.Runtime.",
            "System.Security.",
            "System.Linq.",
            "System.Net.",
            "System.Windows.",
            "System.Reflection.",
            "System.Web.",
        };
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string assemblyName = assembly.GetName().Name;
            if (blackAssembly.Contains(assemblyName) ||
#if !UNITY_EDITOR
                assemblyName.EndsWith("Editor") ||
                assemblyName.StartsWith("UnityEditor") ||
#endif
                assemblyName.StartsWith("Mono.") ||
                (assemblyName.StartsWith("UnityEngine.") || assemblyName.StartsWith("Unity.")) && !unityEngineAssembly.Contains(assemblyName))
                continue;
            foreach (Type type in assembly.GetTypes())
                needVisitTypes.Push(type);
            while (needVisitTypes.Count > 0)
            {
                Type selectType = needVisitTypes.Pop();
                foreach (Type needVisitType in selectType.GetNestedTypes())
                    needVisitTypes.Push(needVisitType);
                if (((selectType.Attributes & TypeAttributes.Public) != 0 ||
                    (selectType.Attributes & TypeAttributes.NestedPublic) != 0) &&
                    !selectType.IsGenericType &&
                    (selectType.IsEnum ||
                    selectType.IsValueType ||
                    (selectType.IsClass && !selectType.IsAbstract)))
                {
                    if (selectType.FullName.IndexOf('.') != -1 &&
                        namespaceBlacklist0.Contains(selectType.FullName.Split('.')[0]))
                        continue;
                    foreach (string @namespace in namespaceBlacklist1)
                    {
                        if (selectType.FullName.StartsWith(@namespace))
                            goto next;
                    }
                    selectTypes.Add(selectType);
                }
            next:;
            }
        }
        List<Type> registerTypes = new List<Type>();
        List<Type> blacklist = new List<Type>()
        {
#if UNITY_STANDALONE
            typeof(UnityEngine.Object),
            typeof(Component),
            typeof(ScriptableObject),
#endif
        };
        foreach (Type type in selectTypes)
        {
            // 这些类型如果有对应的序列化函数，还是能添加进去
            // 但不会主动为其生成序列化函数
#if UNITY_STANDALONE
            if (type.IsSubclassOf(typeof(Component)) ||
                type.IsSubclassOf(typeof(ScriptableObject)))
                blacklist.Add(type);
#endif
            registerTypes.Add(type);
        }
        AddBlackList(blacklist);
        RegisterTypes(true, registerTypes);
    }
    public static void RegisterTypes(bool recursion, params Type[] types) => RegisterTypes(recursion, types as IEnumerable<Type>);
    public static void RegisterTypes(bool recursion, IEnumerable<Type> types)
    {
#if UNIT_TEST
        try
#endif
        {
            HashSet<Type> fieldTypesSet = null;
            if (recursion)
                fieldTypesSet = new HashSet<Type>();
            foreach (Type type in types)
            {
                if (type == null)
                {
                    Debug.LogError("JSONMap.RegisterType get null");
                    continue;
                }
                // {System.ValueTuple`1[T1]}
                if (type.FullName == null)
                    continue;
                if (recursion && Map.ContainsKey(type.FullName))
                    continue;
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                    continue;

                JSONSerialized? serialized = null;
                if (serialized == null && type.IsEnum)
                {
                    if (type.GetCustomAttribute<FlagsAttribute>() == null)
                        serialized = EnumJSONSerialized.CreateEnumJSONSerialized(type);
                    else
                        serialized = EnumJSONSerialized.CreateEnumFlagsJSONSerialized(type);
                }
                if (serialized == null &&
                    type.GetMethod(nameof(ToJSON), BindingFlags.Public | BindingFlags.Static) is MethodInfo toJSONMethod &&
                    type.GetMethod(nameof(ParseJSON), BindingFlags.Public | BindingFlags.Static) is MethodInfo parseJSONMethod)
                {
                    ParameterInfo[] toJSONParameters = toJSONMethod.GetParameters();
                    if (toJSONParameters.Length != 1 ||
                        toJSONParameters[0].ParameterType != typeof(object) ||
                        !toJSONMethod.ReturnType.Equals(typeof(JSONObject)))
                    {
                        Debug.LogWarning($"{type.FullName}.ToJSON method sign is strange.");
                        continue;
                    }
                    ParameterInfo[] parseJSONParameters = parseJSONMethod.GetParameters();
                    if (parseJSONParameters.Length != 1 ||
                        parseJSONParameters[0].ParameterType != typeof(JSONObject) ||
                        !parseJSONMethod.ReturnType.Equals(typeof(object)))
                    {
                        Debug.LogWarning($"{type.FullName}.ParseJSON method sign is strange.");
                        continue;
                    }
                    serialized = new JSONSerialized(
                                        type.Name,
                                        type.FullName,
                                        toJSONMethod.CreateDelegate(typeof(ToJSON)) as ToJSON,
                                        parseJSONMethod.CreateDelegate(typeof(ParseJSON)) as ParseJSON);
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        bool select = false;
                        if (Blacklist.Contains(fieldInfo.FieldType))
                            select = false;
                        if ((fieldInfo.Attributes & FieldAttributes.Public) != 0 &&
                            fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null)
                            select = true;
                        else if ((fieldInfo.Attributes & FieldAttributes.Private) != 0 &&
                            fieldInfo.GetCustomAttribute<SerializeField>() != null)
                            select = true;
                        if (select)
                        {
                            serialized.Value.Fields.Add(fieldInfo);
                            if (recursion)
                                fieldTypesSet.Add(fieldInfo.FieldType);
                        }
                    }
                }
                // 没有自定义 Array, 使用默认的
                if (serialized == null && type.IsArray)
                {
                    serialized = IListJSONSerialized.CreateArrayJSONSerialized(type);
                    if (recursion)
                        fieldTypesSet.Add(type.GetElementType());
                }
                // 没有自定义 Struct, 使用默认的
                if (serialized == null && type.IsValueType)
                {
                    List<FieldInfo> serializedFieldInfos = new List<FieldInfo>();
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        bool select = false;
                        if (Blacklist.Contains(fieldInfo.FieldType))
                            select = false;
                        if ((fieldInfo.Attributes & FieldAttributes.Public) != 0 &&
                            fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null)
                            select = true;
                        else if ((fieldInfo.Attributes & FieldAttributes.Private) != 0 &&
                            fieldInfo.GetCustomAttribute<SerializeField>() != null)
                            select = true;
                        if (select)
                        {
                            if (recursion)
                                fieldTypesSet.Add(fieldInfo.FieldType);
                            serializedFieldInfos.Add(fieldInfo);
                        }
                    }
                    serialized = ValueTypeJSONSerialized.CreateValueTypeJSONSerialized(type, serializedFieldInfos);
                }
                if (type.IsGenericType)
                {
                    if (serialized == null && type.GetGenericTypeDefinition().Equals(typeof(List<>)))
                    {
                        serialized = IListJSONSerialized.CreateListJSONSerialized(type, out Type genericArgument);
                        if (recursion)
                            fieldTypesSet.Add(genericArgument);
                    }
                    if (serialized == null && type.GetGenericTypeDefinition().Equals(typeof(HashSet<>)))
                    {
                        serialized = IListJSONSerialized.CreateHashSetJSONSerialized(type, out Type genericArgument);
                        if (recursion)
                            fieldTypesSet.Add(genericArgument);
                    }
                    if (serialized == null && type.GetGenericTypeDefinition().Equals(typeof(Dictionary<,>)))
                    {
                        serialized = IDictionaryJSONSerialized.CreateIDictionaryJSONSerialized(type, out Type keyType, out Type valueType);
                        if (recursion)
                        {
                            fieldTypesSet.Add(keyType);
                            fieldTypesSet.Add(valueType);
                        }
                    }
                    if (serialized == null &&
                        (type.GetInterface(typeof(ICollection).FullName, false) != null || type.GetInterface(typeof(ICollection<>).FullName, false) != null) &&
                        ICollectionJSONSerialized.TryCreateCollectionJSONSerialized(type, out Type genericArgument1, out JSONSerialized collectionSerialized))
                    {
                        serialized = collectionSerialized;
                        if (recursion)
                            fieldTypesSet.Add(genericArgument1);
                    }
                }

#if ForceSerialize
                if (serialized == null)
                {
                    List<FieldInfo> serializedFieldInfos = new List<FieldInfo>();
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        bool select = false;
                        if (Blacklist.Contains(fieldInfo.FieldType))
                            select = false;
                        if ((fieldInfo.Attributes & FieldAttributes.Public) != 0 &&
                            fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null)
                            select = true;
                        else if ((fieldInfo.Attributes & FieldAttributes.Private) != 0 &&
                            fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null)
                            select = true;
                        if (select)
                        {
                            if (recursion)
                                fieldTypesSet.Add(fieldInfo.FieldType);
                            serializedFieldInfos.Add(fieldInfo);
                        }
                    }
                    serialized = ObjectSerialized.CreateObjectDefaultSerialized(type, serializedFieldInfos);
                }
#endif

                if (serialized != null)
                    RegisterType(serialized.Value);
            }
            if (recursion && fieldTypesSet.Count > 0)
            {
                RegisterTypes(recursion, fieldTypesSet);
            }
#if UNIT_TEST
        }
        catch (Exception e)
        {
            Debug.LogError(e);
#endif
        }
    }

    public static void RegisterType(JSONSerialized serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized.TypeFullName))
            throw new ArgumentException(nameof(JSONSerialized.TypeFullName));
        if (serialized.ToJSON is null)
            throw new ArgumentNullException(nameof(JSONSerialized.ToJSON));
        if (serialized.ParseJSON is null)
            throw new ArgumentNullException(nameof(JSONSerialized.ParseJSON));
        Map[serialized.TypeFullName] = serialized;
    }
    public static JSONObject ToJSON(Type type, object instance)
    {
        if (instance == null)
            return new JSONObject(JSONObject.Type.NULL);
        if (!Map.TryGetValue(type.FullName, out JSONSerialized serialized))
        {
#if STAThread
            RegisterTypes(true, type);
            if (!Map.TryGetValue(type.FullName, out serialized))
#endif
            {
                Debug.LogError($"instance {type.FullName} can not be serialized.");
                return new JSONObject(JSONObject.Type.NULL);
            }
        }
        return serialized.ToJSON(instance);
    }
    public static JSONObject ToJSON(object instance)
    {
        if (instance == null)
            return new JSONObject(JSONObject.Type.NULL);
        Type instanceType = instance.GetType();
        if (!Map.TryGetValue(instanceType.FullName, out JSONSerialized serialized))
        {
            RegisterTypes(false, instanceType);
            if (!Map.TryGetValue(instanceType.FullName, out serialized))
            {
                Debug.LogError($"instance {instanceType.FullName} can not be serialized.");
                return new JSONObject(JSONObject.Type.NULL);
            }
        }
        return serialized.ToJSON(instance);
    }
    public static JSONObject FieldsToJSON(object instance, JSONObject @object)
    {
        if (instance == null)
            return new JSONObject(JSONObject.Type.NULL);
        Type instanceType = instance.GetType();
        if (!Map.TryGetValue(instanceType.FullName, out JSONSerialized serialized))
        {
#if STAThread
            RegisterTypes(true, instanceType);
            if (!Map.TryGetValue(instanceType.FullName, out serialized))
#endif
            {
                Debug.LogError($"instance {instance.GetType().FullName} can not be serialized.");
                return new JSONObject(JSONObject.Type.NULL);
            }
        }
        @object = @object ?? new JSONObject(JSONObject.Type.OBJECT);
        foreach (FieldInfo fieldInfo in serialized.Fields)
        {
            if (!Map.TryGetValue(fieldInfo.FieldType.FullName, out JSONSerialized fieldSerialized))
            {
#if STAThread
                RegisterTypes(true, instanceType);
                if (!Map.TryGetValue(fieldInfo.FieldType.FullName, out fieldSerialized))
#endif
                {
                    Debug.LogError($"{instance.GetType().FullName}.field : ({fieldInfo.FieldType.FullName}){fieldInfo.Name} can not be serialized.");
                    continue;
                }
            }
            @object[fieldInfo.Name] = fieldSerialized.ToJSON(fieldInfo.GetValue(instance));
        }
        return @object;
    }
    public static T ParseJSON<T>(JSONObject @object) => (T)ParseJSON(typeof(T), @object);
    public static object ParseJSON(Type type, JSONObject @object)
    {
        if (@object == null || @object.IsNull)
            return null;
        if (!Map.TryGetValue(type.FullName, out JSONSerialized serialized))
        {
#if STAThread
            RegisterTypes(true, type);
            if (!Map.TryGetValue(type.FullName, out serialized))
#endif
            {
                Debug.LogError($"instance {type} can not parse.");
                return null;
            }
        }
        return serialized.ParseJSON(@object);
    }
    public static object ParseJSON(string typeFullName, JSONObject @object)
    {
        if (@object == null || @object.IsNull)
            return null;
        if (!Map.TryGetValue(typeFullName, out JSONSerialized serialized))
        {
            Debug.LogError($"instance {typeFullName} can not parse.");
            return null;
        }
        return serialized.ParseJSON(@object);
    }
    public static void FieldsParseJSON<T>(ref T instance, JSONObject @object)where T : struct
    {
        object boxInstance = instance;
        FieldsParseJSON(boxInstance, @object);
        instance = (T)boxInstance;
    }
    public static void FieldsParseJSON<T>(T instance, JSONObject @object) where T : class
    {
        if (@object == null || @object.IsNull)
            return;
        Type instanceType = instance.GetType();
        if (!Map.TryGetValue(instanceType.FullName, out JSONSerialized serialized))
        {
#if STAThread
            RegisterTypes(true, instanceType);
            if (!Map.TryGetValue(instanceType.FullName, out serialized))
#endif
            {
                Debug.LogError($"instance {instance.GetType().FullName} can not parse.");
                return;
            }
        }
        foreach (FieldInfo fieldInfo in serialized.Fields)
        {
            if (!Map.TryGetValue(fieldInfo.FieldType.FullName, out JSONSerialized fieldSerialized))
            {
#if STAThread
                RegisterTypes(true, instanceType);
                if (!Map.TryGetValue(fieldInfo.FieldType.FullName, out fieldSerialized))
#endif
                {
                    Debug.LogError($"{instance.GetType().FullName}.field : ({fieldInfo.FieldType.FullName}){fieldInfo.Name} can not parse.");
                    continue;
                }
            }
            JSONObject fieldObject = @object.GetField(fieldInfo.Name);
            if (fieldObject is null || fieldObject.IsNull)
                continue;
            fieldInfo.SetValue(instance, fieldSerialized.ParseJSON(fieldObject));
        }
    }

    public static T JSONDeepClone<T>(in T instance)
    {
        T t = default;
        JSONObject @object = ToJSON(typeof(T), instance);
        t = ParseJSON<T>(@object);
        return t;
    }

#if !UNITY_STANDALONE
    internal static class Debug
    {
        public static void LogError(string info) => WindowsOCR.LogService.Error("JSON", info);
        public static void LogWarning(string info) => Console.WriteLine(info);
    }
#endif
}