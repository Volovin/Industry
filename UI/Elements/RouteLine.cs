using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.AI.Routing;
using Industry.World;
using Industry.UI.Windows;

namespace Industry.UI.Elements
{
    public class RouteLine : UIElement
    {
        public static RouteLine CreateLine()
        {
            var routeLine = ObjectManager.Instance.Spawn("RouteLine") as RouteLine;

            string n = (RouteController.Instance.RoutesCount + 1).ToString();

            routeLine.m_RMwindow = WindowsController.Instance.Windows.RouteManager;
            routeLine.m_numberText.text = n;
            routeLine.m_IField.text = "Route " + n;

            return routeLine;
        }


        [SerializeField]
        private Text m_numberText;
        [SerializeField]
        private InputField m_IField;
        [SerializeField]
        private Dropdown m_dropdownColor;
        [SerializeField]
        private Dropdown m_dropdownPlatform1;
        [SerializeField]
        private Dropdown m_dropdownPlatform2;
        [SerializeField]
        private Button m_editButton;
        [SerializeField]
        private Button m_deleteButton;

        private RouteManagerWindow m_RMwindow;

        private ushort? m_routeID;
        private int m_p1Count;
        private int m_p2Count;

        private int m_p1current;
        private int m_p2current;

        public int NodeOnePlatformsCount
        {
            get
            {
                return m_p1Count;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("value < 0");

                if (m_p1Count != value)
                {
                    m_p1Count = value;

                    SetPlatformDropdown(0, value);
                }
            }
        }

        public int NodeTwoPlatformsCount
        {
            get
            {
                return m_p2Count;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("value < 0");

                if (m_p2Count != value)
                {
                    m_p2Count = value;

                    SetPlatformDropdown(1, value);
                }
            }
        }

        public ushort? RouteID
        {
            get
            {
                return m_routeID;
            }
            set
            {
                if (!m_routeID.HasValue)
                    m_routeID = value;
            }
        }

        public string RouteName
        {
            get
            {
                return m_IField.text;
            }
            set
            {
                m_IField.text = value;
            }
        }


        private void Awake()
        {
            m_dropdownPlatform1.gameObject.SetActive(false);
            m_dropdownPlatform2.gameObject.SetActive(false);

            SetButtonsActive(false);

            enabled = false;
        }

        public void ColorDropdown_IndexChanged(int value)
        {
            if (!RouteID.HasValue)
                return;

            var color = m_dropdownColor.options[value].image.texture.GetPixel(0, 0);
            RouteController.Instance.GetRoute(RouteID.Value).Color = color;
        }

        public void Platform1Dropdown_IndexChanged(int value)
        {
            //NodeOnePlatformsCount = value;
            m_p1current = value;

            EditRoute(false);
        }

        public void Platform2Dropdown_IndexChanged(int value)
        {
            //NodeTwoPlatformsCount = value;
            m_p2current = value;

            EditRoute(false);
        }

        public void SetRouteButton_OnClick()
        {
            SetButtonsActive(false);

            if (RouteID.HasValue)
                EditRoute(true);
            else
                SetRoute();
        }

        public void DeleteRouteButton_OnClick()
        {
            DeleteRoute();
        }

        private void SetPlatformDropdown(int dropdown, int platforms)
        {
            var dd = dropdown == 0 ? m_dropdownPlatform1 : m_dropdownPlatform2;

            dd.options.Clear();

            dd.gameObject.SetActive(platforms > 0);

            if (platforms == 0)
                return;

            var options = new List<string>();

            for (int i = 1; i <= platforms; i++)
                options.Add(i.ToString());

            dd.AddOptions(options);
        }

        private void SetRoute()
        {
            m_RMwindow.ActiveRouteLine = this;

            m_RMwindow.EnableRoutingManagerToSetRoute();
        }

        private void EditRoute(bool rebuildRoute)
        {
            m_RMwindow.ActiveRouteLine = this;

            m_RMwindow.EnableRoutingManagerToEditRoute(RouteID.Value, rebuildRoute, m_p1current, m_p2current);
        }

        private void DeleteRoute()
        {
            if (RouteID.HasValue)
                RouteController.Instance.RemoveRoute(RouteID.Value);

            m_RMwindow.RemoveRouteLine(this);

            Destroy(gameObject);
        }


        public void SetButtonsActive(bool active)
        {
            m_editButton.interactable = active;
            m_deleteButton.interactable = active;
        }
    }
}
