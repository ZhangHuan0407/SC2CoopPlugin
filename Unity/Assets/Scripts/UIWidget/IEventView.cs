﻿using System;
using UnityEngine;
using Game.Model;

namespace Game.UI
{
    public interface IEventView
    {
        GameObject gameObject { get; }
        Guid Guid { get; }
        void Update(float time);
        void SetModel(IEventModel eventModel);
    }
}