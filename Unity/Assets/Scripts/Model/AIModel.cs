using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class AIModel
    {
        [SerializeField]
        public AmonAIName m_AIName;
        public AmonAIName AIName
        {
            get => m_AIName;
            private set => m_AIName = value;
        }

        public IList<IEventModel> BuildEventModels(CoopTimeline coopTimeline)
        {
            // 读取map的数据，根据隐身、对空、鬼子原子弹 等等因素现场生成提示信息
            return Array.Empty<IEventModel>();
        }
    }
}