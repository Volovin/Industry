using UnityEngine;
using System.Collections;

namespace Industry.UI.Windows
{
    public interface IWindow
    {
        void Unhide();
        void Hide();
        void Show();
        void Close();
    }
}