using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Extension
{
    /// <summary>
    /// UGUI Dropdown 扩展
    /// </summary>
    public static class DropdownExtension
    {
        private static Dictionary<Type, List<string>> EnumNames = new Dictionary<Type, List<string>>();

        /// <summary>
        /// 清除当前下拉框的内容，使用枚举定义重新填充
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        public static void RecreateDataFromEnum<TEnum>(this Dropdown dropdown) where TEnum : Enum
        {
            dropdown.ClearOptions();
            Type type = typeof(TEnum);
            if (!EnumNames.TryGetValue(type, out List<string> list))
            {
                list = new List<string>(Enum.GetNames(type));
                EnumNames.Add(type, list);
            }
            dropdown.AddOptions(list);
        }
        /// <summary>
        /// 为下拉框指定一项与给定数据一致的数据, 或第0项
        /// </summary>
        /// <param name="obj">期望选中的数据项(字符串)</param>
        public static void Select(this Dropdown dropdown, object obj)
        {
            string str = obj.ToString();
            int index = dropdown.options.FindIndex((Dropdown.OptionData item) => item.text == str);
            if (dropdown.options.Count > 0)
                dropdown.value = index < 0 ? 0 : index;
        }
        public static bool TryParseEnum<TEnum>(this Dropdown dropdown, out TEnum value) where TEnum : struct
        {
            string str = dropdown.options[dropdown.value].text;
            // 为啥 Enum.TryParse 的 TEnum 约束是 struct 不是 Enum?
            return Enum.TryParse(str, true, out value);
        }
    }
}