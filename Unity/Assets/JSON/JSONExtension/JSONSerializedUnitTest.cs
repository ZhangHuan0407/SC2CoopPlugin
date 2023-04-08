#if UNITTEST && UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class JSONSerializedUnitTest : MonoBehaviour
{
    // 基础类型序列化
    [Serializable]
    public class Test1Class
    {
        /* field */
        public Vector3 vector3;
        public Vector2Int vector2Int;
        public float single;
        public int integer;
        public string str;

        /* func */
        public static JSONObject ToJSON(object instance) => JSONMap.FieldsToJSON(instance, null);
        public static object ParseJSON(JSONObject @object)
        {
            Test1Class result = new Test1Class();
            JSONMap.FieldsParseJSON(result, @object);
            return result;
        }

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            Test1Class test1Class = obj as Test1Class;
            return this.vector3 == test1Class.vector3 &&
                   this.vector2Int == test1Class.vector2Int &&
                   this.single == test1Class.single &&
                   this.integer == test1Class.integer &&
                   this.str == test1Class.str;
        }
    }
    [MenuItem("UnitTest/JSON Serialized/Test1")]
    public static void Test1()
    {
        JSONMap.RegisterDefaultType();
        JSONMap.RegisterAllTypes();

        Test1Class instance = new Test1Class()
        {
            integer = 1,
            single = 2.3f,
            vector2Int = new Vector2Int(4, 5),
            vector3 = new Vector3(6, 7, 8),
            str = "strContent!@#$%^&*()_+{}:<>?",
        };
        string content = JSONMap.ToJSON(instance).ToString(true);
        JSONObject @object = JSONObject.Create(content);
        Test1Class instance2 = JSONMap.ParseJSON(typeof(Test1Class).FullName, @object) as Test1Class;
        Debug.Log(content);
        if (!instance.Equals(instance2))
        {
            Debug.LogError("JSON Test1 => not equal!");
        }
    }

    // 多维数组序列化
    [Serializable]
    public class Test2Class
    {
        /* field */
        public int[] Array1;
        public int[,] Array2_3;
        public int[,,] Array4_3_2;

        /* func */
        public static JSONObject ToJSON(object instance) => JSONMap.FieldsToJSON(instance, null);
        public static object ParseJSON(JSONObject @object)
        {
            Test2Class result = new Test2Class();
            JSONMap.FieldsParseJSON(result, @object);
            return result;
        }
    }
    [MenuItem("UnitTest/JSON Serialized/Test2")]
    public static void Test2()
    {
        Test2Class test2Class = new Test2Class()
        {
            Array1 = new int[] { 1 },
            Array2_3 = new int[2, 3]
            {
                { 0, 1, 2 },
                { 3, 4, 5 },
            },
            Array4_3_2 = new int[4, 3, 2]
            {
                {
                    { 0, 1 },
                    { 2, 3 },
                    { 4, 5 },
                },
                {
                    { 6, 7 },
                    { 8, 9 },
                    { 10, 11 },
                },
                {
                    { 12, 13 },
                    { 14, 15 },
                    { 16, 17 },
                },
                {
                    { 18, 19 },
                    { 20, 21 },
                    { 22, 23 },
                },
            },
        };
        string content = JSONMap.ToJSON(test2Class).ToString(true);
        Debug.Log(content);
        Test2Class instance = JSONMap.ParseJSON(typeof(Test2Class), new JSONObject(content)) as Test2Class;
        content = JSONMap.ToJSON(instance).ToString(true);
        Debug.Log(content);
    }

    public void OnEnable()
    {
        Test2();
    }
}
#endif