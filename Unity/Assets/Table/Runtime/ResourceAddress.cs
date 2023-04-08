namespace Table
{
    /// <summary>
    /// 资源定位地址
    /// </summary>
    public sealed class ResourceAddress
    {
        public enum Package
        {
            Unknown = 0,
            UnityResource = 1,
            UnityAssetBundle = 2,
            DiskFile = 3,
        }

        /* field */
        public Package MainPackage;
        public string PackagePath;
        public string AssetPath;

        /* ctor */
        public ResourceAddress(string value)
        {
            string[] lines = value.Split(':');
            if (lines.Length == 1)
            {
                MainPackage = Package.UnityResource;
                PackagePath = string.Empty;
                AssetPath = lines[0];
            }
            else if (lines.Length >= 3)
            {
                int.TryParse(lines[0], out int package);
                MainPackage = (Package)package;
                PackagePath = lines[1];
                AssetPath = lines[2];
            }
            else
            {
                MainPackage = Package.Unknown;
                PackagePath = string.Empty;
                AssetPath = string.Empty;
            }
        }

        /* func */
        public override bool Equals(object obj)
        {
            return obj is ResourceAddress resourceAddress &&
                   MainPackage == resourceAddress.MainPackage &&
                   PackagePath == resourceAddress.PackagePath &&
                   AssetPath == resourceAddress.AssetPath;
        }
        public override int GetHashCode()
        {
            return (int)MainPackage +
                    PackagePath?.GetHashCode() ?? 0 +
                    AssetPath?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{(int)MainPackage}:{PackagePath}:{AssetPath}";
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            return JSONObject.CreateStringObject(instance.ToString());
        }
        public static object ParseJSON(JSONObject @object)
        {
            string content = @object.str ?? string.Empty;
            ResourceAddress resourceAddress = new ResourceAddress(content);
            return resourceAddress;
        }
        #endregion
    }
}