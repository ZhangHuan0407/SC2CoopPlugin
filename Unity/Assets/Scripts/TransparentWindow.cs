﻿using Game.UI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class TransparentWindow : MonoBehaviour
    {
        [SerializeField]
        private Material m_Material;

        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy,
            int uFlags);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        const uint WS_POPUP = 0x80000000;
        const uint WS_VISIBLE = 0x10000000;

        const uint WS_EX_TOPMOST = 0x00000008;
        const uint WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;

        const int SWP_FRAMECHANGED = 0x0020;
        const int SWP_SHOWWINDOW = 0x0040;
        const int LWA_ALPHA = 2;

        private IntPtr HWND_TOPMOST = new IntPtr(-1);

        private IntPtr _hwnd;

        public WindowState WindowState { get; private set; }

        void Start()
        {
    #if !UNITY_EDITOR
            MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
            _hwnd = GetActiveWindow();
            int fWidth = Screen.width;
            int fHeight = Screen.height;
            // 扩展工作区
            DwmExtendFrameIntoClientArea(_hwnd, ref margins);
            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, fWidth, fHeight, SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            //ShowWindowAsync(_hwnd, 3); //Forces window to show in case of unresponsive app    // SW_SHOWMAXIMIZED(3)
    #endif
        }

        void OnRenderImage(RenderTexture from, RenderTexture to)
        {
            Graphics.Blit(from, to, m_Material);
        }

        public void SetWindowState(WindowState windowState)
        {
            if (windowState == WindowState)
                return;
            WindowState = windowState;
            LogService.System(nameof(SetWindowState), windowState.ToString());
            int fWidth = Screen.width;
            int fHeight = Screen.height;
            CanvasGroup canvasGroup = null;
            if (CameraCanvas.Instance)
                canvasGroup = CameraCanvas.Instance.GetComponent<CanvasGroup>();
            switch (windowState)
            {
                case WindowState.TopMostAndBlockRaycast:
                    if (canvasGroup)
                        canvasGroup.alpha = 1f;
#if !UNITY_EDITOR
                    SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_TOPMOST | WS_EX_LAYERED);
#endif
                    break;
                case WindowState.HideAllAndRaycastIgnore:
                    if (canvasGroup)
                        canvasGroup.alpha = 0f;
#if !UNITY_EDITOR
                    SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT);
#endif
                    break;
                case WindowState.TopMostAndRaycastIgnore:
                    if (canvasGroup)
                        canvasGroup.alpha = 1f;
#if !UNITY_EDITOR
                    SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT);
#endif
                    break;
                default:
                    throw new NotImplementedException(windowState.ToString());
            }
#if !UNITY_EDITOR
            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, fWidth, fHeight, SWP_FRAMECHANGED | SWP_SHOWWINDOW);
#endif
            // WS_EX_LAYERED 去除标记后，据说要 UpdateLayeredWindow 窗体才能正常渲染
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (WindowState == WindowState.TopMostAndRaycastIgnore ||
                    WindowState == WindowState.HideAllAndRaycastIgnore)
                    //SetWindowState(WindowState.HideAllAndRaycastIgnore);
                    SetWindowState(WindowState.TopMostAndBlockRaycast);
                else if (WindowState == WindowState.TopMostAndBlockRaycast)
                    SetWindowState(WindowState.TopMostAndRaycastIgnore);
            }
        }
    }
}