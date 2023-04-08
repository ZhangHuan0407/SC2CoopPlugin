using System;
using System.Text;

public static class JSONObjectExtension
{
    public static byte[] ToBytes(this JSONObject @object) => Encoding.UTF8.GetBytes(@object.ToString(false));
    public static JSONObject ToJSONObject(this byte[] bytes) => JSONObject.Create(Encoding.UTF8.GetString(bytes));
}