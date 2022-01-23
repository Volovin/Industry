using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.Managers;
using Industry.UI.Elements;
using Industry.AI.Routing;

namespace Industry.UI.Windows
{
    public class RouteManagerWindow : UIWindow
    {
        [SerializeField]
        private ScrollBar m_scrollBar;

        private List<string> m_routeStrings;
        private List<RouteLine> m_lines;
        private Vector2 m_lastPosition;

        public RouteLine ActiveRouteLine
        {
            get; set;
        }

        public List<string> RoutesAsStrings
        {
            get
            {
                UpdateStrings();

                return m_routeStrings;
            }
        }

        public void AddRouteLine()
        {
            var line = RouteLine.CreateLine();

            line.transform.SetParent(m_scrollBar.Content, false);

            var lineRect = line.transform as RectTransform;

            int yPos = m_lastPosition == Vector2.zero ?
                (int)(m_scrollBar.ViewportSize.y / 2) :
                (int)(m_lastPosition.y - lineRect.rect.yMax - 10);

            m_lastPosition = new Vector2(0, yPos);
            lineRect.anchoredPosition = m_lastPosition;

            m_lines.Add(line);
            ActiveRouteLine = line;
        }

        public void RemoveRouteLine(RouteLine line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            if (line == ActiveRouteLine)
                ActiveRouteLine = null;

            int idx = m_lines.IndexOf(line);
            m_lines.RemoveAt(idx);

            var currPos = (line.transform as RectTransform).anchoredPosition;

            for (int i = idx; i < m_lines.Count; i++)
            {
                var lineRect = m_lines[i].transform as RectTransform;
                lineRect.anchoredPosition = currPos;

                int yPos = (int)(currPos.y - lineRect.rect.yMax - 10);
                currPos = new Vector2(0, yPos);
            }

            m_lastPosition = m_lines.Count > 0 ? 
                (m_lines[m_lines.Count - 1].transform as RectTransform).anchoredPosition :
                Vector2.zero;

            UpdateRoutes();
        }

        public void EnableRoutingManager()
        {
            AddRouteLine();

            EnableRoutingManagerToSetRoute();
        }

        public void EnableRoutingManagerToSetRoute()
        {
            MainManager.Instance.EnableManager("RoutingManager");
        }

        public void EnableRoutingManagerToEditRoute(ushort routeID, bool rebuildRoute, int platform1, int platform2)
        {
            RoutingManager.Instance.SetRouteEditingInfo(routeID, rebuildRoute, platform1, platform2);

            MainManager.Instance.EnableManager("RoutingManager");
        }

        public void SetButtonsToActiveLine(bool active)
        {
            var arl = ActiveRouteLine;

            if (arl != null)
                arl.SetButtonsActive(active);
        }

        public void SetRouteInfoToActiveLine(ushort ID, int[] platformsCount)
        {
            var arl = ActiveRouteLine;

            if (arl == null)
                return;

            arl.RouteID = ID;

            arl.NodeOnePlatformsCount = platformsCount[0];
            arl.NodeTwoPlatformsCount = platformsCount[1];

            arl.SetButtonsActive(true);

            ActiveRouteLine = null;

            UpdateRoutes();
        }

        private void UpdateRoutes()
        {
            // update active windows
            // inactive windows will update on showing

            var depotWindows = FindObjectsOfType<DepotWindow>();

            foreach (var window in depotWindows)
            {
                window.UpdateContent();
            }
        }

        private void UpdateStrings()
        {
            var strings = new List<string>();

            foreach (var line in m_lines)
            {
                if (!line.RouteID.HasValue)
                    continue;

                strings.Add(line.RouteID + "`" + line.RouteName);
            }

            m_routeStrings = strings;
        }


        protected override void OnAwakening()
        {
            m_routeStrings = new List<string>();
            m_lines = new List<RouteLine>();
        }

        protected override void OnShowing()
        {
            //WindowsController.Instance.WindowsLocked = true;
            MainCamera.Instance.CanZoom = false;
            RouteController.Instance.DrawAll = true;
        }

        protected override void OnHiding()
        {
            //WindowsController.Instance.WindowsLocked = false;
            MainCamera.Instance.CanZoom = true;
            RouteController.Instance.DrawAll = false;
        }

        protected override void OnStarting()
        {
            enabled = false;
        }
    }
}
