using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.UI.Elements;

namespace Industry.UI.Windows
{
    public class WindowsController : Singleton<WindowsController>
    {
        [SerializeField]
        private PopupController popupController;

        [SerializeField]
        private bool navigationHistory = true;

        private List<IWindow> openedWindows;


        public bool NavigationHistory
        {
            get { return navigationHistory; }
        }

        public bool WindowsLocked
        {
            get; set;
        }

        public UIWindow ActiveWindow
        {
            get
            {
                return FindObjectOfType<UIWindow>();
            }
        }

        public WindowsCollection Windows
        {
            get; private set;
        }

        public PopupController PopupController
        {
            get { return popupController; }
        }


        private void Awake()
        {
            openedWindows = new List<IWindow>();

            popupController.Setup(FindObjectOfType<UIPopup>(), this);
        }

        private void Start()
        {
            Windows = new WindowsCollection();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                ReturnToLastWindow();
        }


        private void RemoveWindow()
        {
            IWindow window = openedWindows[openedWindows.Count - 1];
            window.Hide();
            openedWindows.Remove(window);
        }


        public void RegisterWindow(IWindow window)
        {
            HideLastWindow();
            openedWindows.Add(window);
        }

        public void ReturnToLastWindow()
        {
            if (navigationHistory && openedWindows.Count > 1)
            {
                RemoveWindow();
                openedWindows[openedWindows.Count - 1].Unhide();
            }
            else if (navigationHistory && openedWindows.Count > 0)
                RemoveWindow();
        }

        public void UnhideLastWindow()
        {
            if (openedWindows.Count > 0)
                openedWindows[openedWindows.Count - 1].Unhide();
        }

        public void HideLastWindow()
        {
            if (openedWindows.Count > 0)
                openedWindows[openedWindows.Count - 1].Hide();
        }

        public void DeleteNavigationHistory()
        {
            openedWindows.Clear();
        }

        public bool VerifyInputPosition()
        {
            var tr_mPos = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f);

            if (ToolBar.Instance.BoundsRect.Contains(tr_mPos))
            {
                return false;
            }

            var activeWindow = ActiveWindow;

            if (activeWindow != null && activeWindow.BoundsRect.Contains(tr_mPos))
            {
                return false;
            }


            return true;
        }


        public sealed class WindowsCollection
        {
            public WindowsCollection()
            {
                RouteManager = FindObjectOfType<RouteManagerWindow>();

                var windows = GameObject.Find("Windows").
                    GetComponentsInChildren<BuildingsGroupWindow>(true);

                RawResources = windows[0];
                ResourcesProcessing = windows[1];
                ProductFactories = windows[2];
                UnloadingPoints = windows[3];
                Depots = windows[4];

                RawResources.AddButton("Saw Mill");

                UnloadingPoints.AddButton("Warehouse 4x4");

                Depots.AddButton("Depot");
            }

            public RouteManagerWindow RouteManager
            {
                get;
            }

            public BuildingsGroupWindow RawResources
            {
                get;
            }

            public BuildingsGroupWindow ResourcesProcessing
            {
                get;
            }

            public BuildingsGroupWindow ProductFactories
            {
                get;
            }

            public BuildingsGroupWindow UnloadingPoints
            {
                get;
            }

            public BuildingsGroupWindow Depots
            {
                get;
            }


        }
    }
}