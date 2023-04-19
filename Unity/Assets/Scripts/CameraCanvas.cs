using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>    
    /// 当前UI相机渲染使用的主要画布
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class CameraCanvas : MonoBehaviour
    {
        /* field */
        private static CameraCanvas m_Instance;

        private List<IDialog> m_DialogList;

        /* inter */
        public static Vector2 ReferenceResolution
        {
            get
            {
                CanvasScaler canvasScaler = m_Instance.GetComponent<CanvasScaler>();
                return canvasScaler.referenceResolution;
            }
        }
        public static Vector2 CanvasSize => (m_Instance.transform as RectTransform).rect.size;
        public static Rect CanvasSafeRect
        {
            get
            {
                Rect safeArea = Screen.safeArea;
                float width = Screen.currentResolution.width;
                float height = Screen.currentResolution.height;
                Vector2 size = (m_Instance.transform as RectTransform).rect.size;
                size.x /= width;
                size.y /= height;
                return new Rect(safeArea.position * size, safeArea.size * size) ;
            }
        }

        /* ctor */
        private void Awake()
        {
            m_Instance = this;
            m_DialogList = new List<IDialog>();
        }

        /* func */
        private void OnDestroy()
        {
            if (m_Instance == this)
                m_Instance = null;
        }

        public static IDialog PushDialog(string prefabPath)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            LogService.System(nameof(PushDialog), $"{nameof(prefabPath)}: {prefabPath}, {nameof(GameObject)}: {prefab}");
            IDialog dialog = Instantiate(prefab, m_Instance.transform).GetComponent<IDialog>();
            dialog.PrefabPath = prefabPath;
            m_Instance.m_DialogList.Add(dialog);
            return dialog;
        }
        public static void PopDialog(IDialog dialog)
        {
            m_Instance.m_DialogList.Remove(dialog);
            if (dialog.DestroyFlag)
                Debug.LogError("dialog.DestroyFlag is true");
            dialog.DestroyFlag = true;
            UnityEngine.Object.Destroy(dialog.gameObject);
        }
        public static void AppendExistsDialog(IDialog dialog)
        {
            if (m_Instance.m_DialogList.Contains(dialog))
                Debug.LogWarning($"{nameof(CameraCanvas)} contains same reference of {dialog.GetType().Name}");
            else
                m_Instance.m_DialogList.Add(dialog);
        }
        /// <summary>
        /// 将此窗体的渲染顺序修改为最高
        /// </summary>
        /// <param name="topDialog">置顶窗体引用</param>
        public static void SetTopMost(IDialog topDialog)
        {
            bool haveTargetDialog = false;
            List<IDialog> dialogList = m_Instance.m_DialogList;
            for (int i = 0; i < (haveTargetDialog ? dialogList.Count - 1 : dialogList.Count); i++)
            {
                IDialog dialog = dialogList[i];
                if (dialog == topDialog)
                {
                    haveTargetDialog = true;
                }
                if (haveTargetDialog && i < dialogList.Count - 1)
                    dialogList[i] = dialogList[i + 1];
                dialog.Canvas.sortingOrder = i * GameDefined.DialogSortingOrderPadding;
            }
            dialogList[dialogList.Count - 1] = topDialog;
            topDialog.Canvas.sortingOrder = (dialogList.Count - 1) * GameDefined.DialogSortingOrderPadding;
        }
        public static IDialog GetTopMost()
        {
            List<IDialog> list = m_Instance.m_DialogList;
            return list.Count > 0 ? list[list.Count - 1] : null;
        }

        /// <summary>
        /// 查找所有的目标类型窗体，不论其是否可见
        /// </summary>
        /// <typeparam name="TDialog">目标类型窗体</typeparam>
        /// <returns></returns>
        public static IEnumerable<IDialog> GetDialogs<TDialog>() where TDialog : IDialog
        {
            foreach (IDialog dialog in m_Instance.m_DialogList)
            {
                if (dialog is TDialog)
                    yield return dialog;
            }
        }

        private void ShowAllDialogs()
        {
            foreach (IDialog dialog in m_DialogList)
            {
                dialog.Show();
            }
        }
    }
}