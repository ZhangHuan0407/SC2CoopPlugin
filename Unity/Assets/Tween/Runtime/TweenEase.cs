using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public class TweenEase
    {
        public static Dictionary<string, AnimationCurve> CurveMap { get; private set; }

        internal static void InitCurveMap(IEnumerable<TweenConfig.EaseCurve> easeCurves)
        {
            CurveMap = new Dictionary<string, AnimationCurve>();

            foreach (TweenConfig.EaseCurve easeCurve in easeCurves)
            {
                if (!string.IsNullOrEmpty(easeCurve.Name) &&
                    easeCurve.Curve != null)
                    CurveMap[easeCurve.Name] = easeCurve.Curve;
            }
        }
    }
}