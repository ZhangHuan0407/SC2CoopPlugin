using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public interface IDialog
    {
        Canvas Canvas { get; }
        GameObject gameObject { get; }
        bool DestroyFlag { get; set; }
        string PrefabPath { get; set; }

        void Hide();
        void Show();
    }
}