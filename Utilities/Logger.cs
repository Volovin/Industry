using System;
using UnityEngine;

namespace Industry.Utilities
{
    public static class Logger
    {
        public static void Log(object message, string color = "white")
        {
            Debug.Log($"<color={color}>{message}</color>");
        }
    }
}
