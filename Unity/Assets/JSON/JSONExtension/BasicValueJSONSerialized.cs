using System;
using System.Collections.Generic;

public static class BasicValueJSONSerialized
{
    public static JSONSerialized BoolSerialized => new JSONSerialized(typeof(bool), BoolToJSON, BoolParseJSON);
    public static JSONObject BoolToJSON(object integer)
    {
        bool value = integer is bool ? (bool)integer : false;
        return JSONObject.Create(value);
    }
    public static object BoolParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return false;
        return @object.b;
    }

    public static JSONSerialized FloatSerialized => new JSONSerialized(typeof(float), FloatToJSON, FloatParseJSON);
    public static JSONObject FloatToJSON(object single)
    {
        float value = single is float ? (float)single : default;
        return JSONObject.Create(value);
    }
    public static object FloatParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return 0f;
        return @object.f;
    }

    public static JSONSerialized IntSerialized => new JSONSerialized(typeof(int), IntToJSON, IntParseJSON);
    public static JSONObject IntToJSON(object integer)
    {
        int value = integer is int ? (int)integer : default;
        return JSONObject.Create(value);
    }
    public static object IntParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return 0;
        return (int)@object.i;
    }

    public static JSONSerialized LongSerialized => new JSONSerialized(typeof(long), LongToJSON, LongParseJSON);
    public static JSONObject LongToJSON(object @long)
    {
        long value = @long is long ? (long)@long : default;
        return JSONObject.Create(value);
    }
    public static object LongParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return 0L;
        return @object.i;
    }

    public static JSONSerialized StringSerialized => new JSONSerialized(typeof(string), StringToJSON, StringParseJSON);
    public static JSONObject StringToJSON(object @string)
    {
        string value = @string is string ? (string)@string : default;
        JSONObject @object = new JSONObject(JSONObject.Type.STRING)
        {
            str = value
        };
        return @object;
    }
    public static object StringParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return null;
        return @object.str;
    }

    public static JSONSerialized DateTimeSerialized => new JSONSerialized(typeof(DateTime), DateTimeToJSON, DateTimeParseJSON);
    public static JSONObject DateTimeToJSON(object dateTime)
    {
        DateTime value = dateTime is DateTime ? (DateTime)dateTime : default;
        JSONObject @object = new JSONObject(JSONObject.Type.STRING)
        {
            str = value.ToString("yyyy/MM/dd HH:mm:ss"),
        };
        return @object;
    }
    public static object DateTimeParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return null;
        return DateTime.Parse(@object.str);
    }
    
    public static JSONSerialized TimeSpanSerialized => new JSONSerialized(typeof(TimeSpan), TimeSpanToJSON, TimeSpanParseJSON);
    public static JSONObject TimeSpanToJSON(object dateTime)
    {
        TimeSpan value = dateTime is TimeSpan ? (TimeSpan)dateTime : default;
        JSONObject @object = new JSONObject(JSONObject.Type.STRING)
        {
            str = value.ToString(),
        };
        return @object;
    }
    public static object TimeSpanParseJSON(JSONObject @object)
    {
        if (@object is null || @object.IsNull)
            return null;
        return TimeSpan.Parse(@object.str);
    }
}