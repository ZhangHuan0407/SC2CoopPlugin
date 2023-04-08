using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game
{
    public static class GameDefined
    {
        public const int Version = 1;

        /// <summary>
        /// 用户索引 <see cref="ObjectID"/> 长度
        /// </summary>
        public const byte UserIDLength = 16;

        public const int DialogSortingOrderPadding = 100;

        /// <summary>
        /// JSON 序列化接口注册
        /// </summary>
        public static readonly JSONSerialized[] JSONSerializedRegisterTypes = new JSONSerialized[]
        {
        };

    }
}