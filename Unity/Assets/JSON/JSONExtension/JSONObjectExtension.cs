using System;
using System.Text;

public static class JSONObjectExtension
{
    public static byte[] ToBytes(this JSONObject @object) => Encoding.UTF8.GetBytes(@object.ToString(false));
}