using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game
{
    public class MouseTracking : MonoBehaviour
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        private const int VK_LBUTTON = 0x01; //鼠标左键
        private const int VK_RBUTTON = 0x02; //鼠标右键
        private const int VK_MBUTTON = 0x04; //鼠标中键

        private bool _isLeftDown;
        private bool _isRightDown;
        private bool _isMiddleDown;

        public event Action<MouseKey, Vector3> MouseKeyDownEvent;
        public event Action<MouseKey, Vector3> MouseKeyUpEvent;
        public event Action<MouseKey, Vector3> MouseDragEvent;
        public event Action<MouseKey> MouseKeyClickEvent;

        public Vector3 MousePos{ get; private set; }

        private bool _hasDragged;
        private Vector3 _leftDownPos;
        private Vector3 _rightDownPos;
        private Vector3 _middleDownPos;

        public static MouseTracking Instance;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Init();
        }

        private void Update() 
        {
            // 按下左键
            if (GetAsyncKeyState(VK_LBUTTON) != 0)
            {
                if (!_isLeftDown)
                {
                    _isLeftDown = true;
                    _leftDownPos = MouseKeyDown(MouseKey.Left);
                }
                else if (MousePos != Input.mousePosition)
                {
                    MouseKeyDrag(MouseKey.Left);
                    if (!_hasDragged)
                    {
                        _hasDragged = true;
                    }
                }
            }
            // 按下右键
            if (GetAsyncKeyState(VK_RBUTTON) != 0)
            {
                if (!_isRightDown)
                {
                    _isRightDown = true;
                    _rightDownPos = MouseKeyDown(MouseKey.Right);
                }
                else if(MousePos != Input.mousePosition)
                {
                    MouseKeyDrag(MouseKey.Right);
                    if (!_hasDragged)
                    {
                        _hasDragged = true;
                    }
                }
            }
            // 按下中键
            if (GetAsyncKeyState(VK_MBUTTON) != 0)
            {
                if (!_isMiddleDown)
                {
                    _isMiddleDown = true;
                    _middleDownPos = MouseKeyDown(MouseKey.Middle);
                }
                else if(MousePos != Input.mousePosition)
                {
                    MouseKeyDrag(MouseKey.Middle);
                    if (!_hasDragged)
                    {
                        _hasDragged = true;
                    }
                }
            }
            // 抬起左键
            if (GetAsyncKeyState(VK_LBUTTON) == 0 && _isLeftDown)
            {
                _isLeftDown = false;
                MouseKeyUp(MouseKey.Left);

                if (!_hasDragged && _leftDownPos == MousePos)
                {
                    MouseKeyClick(MouseKey.Left);
                }

                _hasDragged = false;
            }
            // 抬起右键
            if (GetAsyncKeyState(VK_RBUTTON) == 0 && _isRightDown)
            {
                _isRightDown = false;
                MouseKeyUp(MouseKey.Right);

                if (!_hasDragged && _rightDownPos == MousePos)
                {
                    MouseKeyClick(MouseKey.Right);
                }

                _hasDragged = false;
            }
            // 抬起中键
            if (GetAsyncKeyState(VK_MBUTTON) == 0 && _isMiddleDown)
            {
                _isMiddleDown = false;
                MouseKeyUp(MouseKey.Middle);

                if (!_hasDragged && _middleDownPos == MousePos)
                {
                    MouseKeyClick(MouseKey.Middle);
                }

                _hasDragged = false;
            }
        }

        private void OnDestroy()
        {
            Destroy();
        }
    
        public void Init()
        {
            _isLeftDown = false;
            _isRightDown = false;
            _isMiddleDown = false;
            _hasDragged = false;
        }
    
        public void Destroy()
        {
        
        }

        private Vector3 MouseKeyDown(MouseKey mouseKey)
        {
            RefreshMousePos();
            MouseKeyDownEvent?.Invoke(mouseKey, MousePos);

            return MousePos;
        }
        private Vector3 MouseKeyUp(MouseKey mouseKey)
        {
            RefreshMousePos();
            MouseKeyUpEvent?.Invoke(mouseKey, MousePos);

            return MousePos;
        }

        private Vector3 MouseKeyDrag(MouseKey mouseKey)
        {
            RefreshMousePos();
            MouseDragEvent?.Invoke(mouseKey, MousePos);

            return MousePos;
        }

        private void MouseKeyClick(MouseKey mouseKey)
        {
            MouseKeyClickEvent?.Invoke(mouseKey);
        }

        private Vector3 RefreshMousePos()
        {
            MousePos = Input.mousePosition;
            return MousePos;
        }

        public Vector3 MousePosToWorldPos(Vector3 mousePos)
        {
            var screenInWorldPos = Camera.main.WorldToScreenPoint(mousePos);
            var refPos = new Vector3(mousePos.x, mousePos.y, screenInWorldPos.z);
            var pos = Camera.main.ScreenToWorldPoint(refPos);
            return pos;
        }
    }

    public enum MouseKey
    { 
        None,
        Left,
        Right,
        Middle
    }
}