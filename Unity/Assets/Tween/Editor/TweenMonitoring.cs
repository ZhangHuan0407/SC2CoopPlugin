using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tween.Editor
{
    public class TweenMonitoring
    {
        /* field */
        private static FieldInfo BehaviourHistoryFieldInfo;
        public static TweenerBehaviourRecord AverageRecord;

        /* ctor */

        /* inter */

        /* func */
        public static void UpdateAverage()
        {
            BehaviourHistoryFieldInfo = BehaviourHistoryFieldInfo
                ?? typeof(TweenService).GetField("BehaviourHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!TweenService.Instance
                || BehaviourHistoryFieldInfo is null)
                return;
            else if (BehaviourHistoryFieldInfo.GetValue(TweenService.Instance) is Queue<TweenerBehaviourRecord> history)
            {
                AverageRecord = default;
                foreach (TweenerBehaviourRecord behaviourRecord in history)
                    AverageRecord += behaviourRecord;
                AverageRecord /= history.Count;
            }
        }
    }
}