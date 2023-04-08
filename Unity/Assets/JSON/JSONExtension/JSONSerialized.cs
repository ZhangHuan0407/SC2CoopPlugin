using System;
using System.Collections.Generic;
using System.Reflection;

public delegate JSONObject ToJSON(object instance);
public delegate object ParseJSON(JSONObject @object);

public struct JSONSerialized
{
    public readonly string TypeName;
    public readonly string TypeFullName;
    public readonly ToJSON ToJSON;
    public readonly ParseJSON ParseJSON;
    public readonly List<FieldInfo> Fields;

    public JSONSerialized(string name, string typeFullName, ToJSON toJSON, ParseJSON parseJSON)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"\"{nameof(name)}\" Can not be null or white space.", nameof(name));
        
        if (string.IsNullOrWhiteSpace(typeFullName))
            throw new ArgumentException($"\"{nameof(typeFullName)}\" Can not be null or white space.", nameof(typeFullName));

        TypeName = name;
        TypeFullName = typeFullName;
        ToJSON = toJSON;
        ParseJSON = parseJSON;
        Fields = new List<FieldInfo>();
    }
    public JSONSerialized(Type type, ToJSON toJSON, ParseJSON parseJSON)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        TypeName = type.Name;
        TypeFullName = type.FullName;
        ToJSON = toJSON;
        ParseJSON = parseJSON;
        Fields = new List<FieldInfo>();
    }
}