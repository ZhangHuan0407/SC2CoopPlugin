using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI.Extension
{
    public static class UIBehaviorExtention
    {
        public static Vector3 GetDrawingGameObjectPosition(this Canvas canvas, GameObject targetGameObject) =>
            GetDrawingGameObjectPosition(canvas, targetGameObject.transform);
        public static Vector3 GetDrawingGameObjectPosition(this Canvas canvas, Transform targetTransform)
        {
            if (canvas is null)
                throw new ArgumentNullException(nameof(canvas));
            if (targetTransform is null)
                throw new ArgumentNullException(nameof(targetTransform));

            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetTransform.position);
            RectTransform canvasRectTransform = canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRectTransform.rect.size;
            Resolution solution = Screen.currentResolution;
            Vector2 delta = canvasRectTransform.TransformPoint(new Vector3(canvasSize.x * (viewportPoint.x / solution.width - 0.5f),
                                                               canvasSize.y * (viewportPoint.y / solution.height - 0.5f),
                                                               0f));
            return canvas.transform.InverseTransformPoint(delta);
        }

        //public static void AA(this RectTransform rectTransform, )
        //{

        //}
    }
}