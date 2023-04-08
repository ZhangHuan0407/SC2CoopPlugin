using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorJSONSerialized
{
    public static JSONSerialized Vector4Serialized => new JSONSerialized(typeof(Vector4), Vector4ToJSON, Vector4ParseJSON);
    public static JSONObject Vector4ToJSON(object vector4)
    {
        Vector4 value = vector4 is Vector4 ? (Vector4)vector4 : default;
        JSONObject jsValue = new JSONObject(JSONObject.Type.OBJECT);
        jsValue.SetField(nameof(Vector4.x), value.x);
        jsValue.SetField(nameof(Vector4.y), value.y);
        jsValue.SetField(nameof(Vector4.z), value.z);
        jsValue.SetField(nameof(Vector4.w), value.w);
        return jsValue;
    }
    public static object Vector4ParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return new Vector4();
        float x = @object.GetField(nameof(Vector4.x))?.f ?? 0f;
        float y = @object.GetField(nameof(Vector4.y))?.f ?? 0f;
        float z = @object.GetField(nameof(Vector4.z))?.f ?? 0f;
        float w = @object.GetField(nameof(Vector4.w))?.f ?? 0f;
        return new Vector4(x, y, z, w);
    }

    public static JSONSerialized Vector3IntSerialized => new JSONSerialized(typeof(Vector3Int), Vector3IntToJSON, Vector3IntParseJSON);
    public static JSONObject Vector3IntToJSON(object vector3Int)
    {
        Vector3Int value = vector3Int is Vector3Int ? (Vector3Int)vector3Int : default;
        JSONObject jsValue = new JSONObject(JSONObject.Type.OBJECT);
        jsValue.SetField(nameof(Vector3Int.x), value.x);
        jsValue.SetField(nameof(Vector3Int.y), value.y);
        jsValue.SetField(nameof(Vector3Int.z), value.z);
        return jsValue;
    }
    public static object Vector3IntParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return new Vector3Int();
        long x = @object.GetField(nameof(Vector3Int.x))?.i ?? 0L;
        long y = @object.GetField(nameof(Vector3Int.y))?.i ?? 0L;
        long z = @object.GetField(nameof(Vector3Int.z))?.i ?? 0L ;
        return new Vector3Int((int)x, (int)y, (int)z);
    }

    public static JSONSerialized Vector3Serialized => new JSONSerialized(typeof(Vector3), Vector3ToJSON, Vector3ParseJSON);
    public static JSONObject Vector3ToJSON(object vector3)
    {
        Vector3 value = vector3 is Vector3 ? (Vector3)vector3 : default;
        JSONObject jsValue = new JSONObject(JSONObject.Type.OBJECT);
        jsValue.SetField(nameof(Vector3.x), value.x);
        jsValue.SetField(nameof(Vector3.y), value.y);
        jsValue.SetField(nameof(Vector3.z), value.z);
        return jsValue;
    }
    public static object Vector3ParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return new Vector3();
        float x = @object.GetField(nameof(Vector3.x))?.f ?? 0f;
        float y = @object.GetField(nameof(Vector3.y))?.f ?? 0f;
        float z = @object.GetField(nameof(Vector3.z))?.f ?? 0f;
        return new Vector3(x, y, z);
    }

    public static JSONSerialized Vector2IntSerialized => new JSONSerialized(typeof(Vector2Int), Vector2IntToJSON, Vector2IntParseJSON);
    public static JSONObject Vector2IntToJSON(object vector2Int)
    {
        Vector2Int value = vector2Int is Vector2Int ? (Vector2Int)vector2Int : default;
        JSONObject jsValue = new JSONObject(JSONObject.Type.OBJECT);
        jsValue.SetField(nameof(Vector2Int.x), value.x);
        jsValue.SetField(nameof(Vector2Int.y), value.y);
        return jsValue;
    }
    public static object Vector2IntParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return new Vector2Int();
        long x = @object.GetField(nameof(Vector2Int.x))?.i ?? 0L;
        long y = @object.GetField(nameof(Vector2Int.y))?.i ?? 0L;
        return new Vector2Int((int)x, (int)y);
    }

    public static JSONSerialized Vector2Serialized => new JSONSerialized(typeof(Vector2), Vector2ToJSON, Vector2ParseJSON);
    public static JSONObject Vector2ToJSON(object vector2)
    {
        Vector2 value = vector2 is Vector2 ? (Vector2)vector2 : default;
        JSONObject jsValue = new JSONObject(JSONObject.Type.OBJECT);
        jsValue.SetField(nameof(Vector2.x), value.x);
        jsValue.SetField(nameof(Vector2.y), value.y);
        return jsValue;
    }
    public static object Vector2ParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return new Vector2();
        float x = @object.GetField(nameof(Vector2.x))?.f ?? 0f;
        float y = @object.GetField(nameof(Vector2.y))?.f ?? 0f;
        return new Vector2(x, y);
    }
}