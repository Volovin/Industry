using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.Managers;
using Industry.UI.Elements;
using Industry.AI.Routing;
using Industry.UI.Windows.Buildings;
using Industry.World.Buildings.Depots;
using Industry.World.Vehicles;

namespace Industry.UI.Windows
{
    public class DepotWindow : BuildingWindow
    {
        [SerializeField]
        private ScrollBar m_linesScrollBar;

        [SerializeField]
        private ScrollBar m_vehicleScrollBar;

        [SerializeField]
        private GameObject m_selectionWindow;

        private List<VehicleLine> m_lines;
        private List<VehicleButton> m_buttons;

        private Vector2 m_lastPosition;

        public int LinesCount
        { 
            get { return m_lines.Count; }
        }


        public void SelectVehicle()
        {
            //Hide();

            OpenSelection();
        }

        private void OpenSelection()
        {
            m_selectionWindow.SetActive(true);
        }

        public void CloseSelection(string result)
        {
            m_selectionWindow.SetActive(false);

            AddVehicleLine(result);

            Unhide();
        }

        public void AddVehicleLine(string vName, int route = 0)
        {
            if (string.IsNullOrEmpty(vName))
                return;

            var line = VehicleLine.CreateLine(this, vName, route);
            line.transform.SetParent(m_linesScrollBar.Content, false);

            var lineRect = line.transform as RectTransform;

            int yPos = m_lastPosition == Vector2.zero ?
                (int)(m_linesScrollBar.ViewportSize.y / 2) :
                (int)(m_lastPosition.y - lineRect.rect.yMax - 10);

            m_lastPosition = new Vector2(0, yPos);
            lineRect.anchoredPosition = m_lastPosition;

            m_lines.Add(line);
        }

        public void RemoveVehicleLine(VehicleLine line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            int idx = m_lines.IndexOf(line);
            m_lines.RemoveAt(idx);

            var currPos = (line.transform as RectTransform).anchoredPosition;

            for (int i = idx; i < m_lines.Count; i++)
            {
                m_lines[i].Number = (i + 1).ToString();

                var lineRect = m_lines[i].transform as RectTransform;
                lineRect.anchoredPosition = currPos;

                int yPos = (int)(currPos.y - lineRect.rect.yMax - 10);
                currPos = new Vector2(0, yPos);
            }

            m_lastPosition = m_lines.Count > 0 ?
                (m_lines[m_lines.Count - 1].transform as RectTransform).anchoredPosition :
                Vector2.zero;
        }


        private void FillSelectionWindow()
        {
            // TODO: actually generate the vehicle list

            AddButton("Truck");
        }



        public void AddButton(string vehicleName)
        {
            var bButton = VehicleButton.CreateButton(vehicleName, this);

            bButton.transform.SetParent(m_vehicleScrollBar.Content, false);

            var vps = m_vehicleScrollBar.ViewportSize;
            var btnSize = bButton.ButtonSize;

            float spaceX = (vps.x - btnSize.x * 2) / 3;
            float spaceY = (vps.y - btnSize.y * 2) / 3;
            float posX, posY;

            if (m_buttons.Count == 0)
            {
                Vector2 topLeft = new Vector2(-vps.x / 2, m_vehicleScrollBar.Content.sizeDelta.y / 2);

                posX = topLeft.x + btnSize.x / 2f + spaceX;
                posY = topLeft.y - btnSize.y / 2f - spaceY;
            }
            else
            {
                Vector2 firstPos = m_buttons[0].ButtonPosition;
                Vector2 lastPos = m_buttons[m_buttons.Count - 1].ButtonPosition;

                if (m_buttons.Count % 2 == 0)
                {
                    posX = firstPos.x;
                    posY = lastPos.y - btnSize.y - spaceY;
                }
                else
                {
                    posX = lastPos.x + btnSize.x + spaceX;
                    posY = lastPos.y;
                }
            }

            bButton.ButtonPosition = new Vector2(posX, posY);
            m_buttons.Add(bButton);
        }


        public void SetSelectedVehiclesToRoute(ushort ID)
        {
            var route = RouteController.Instance.GetRoute(ID);

            foreach (var line in m_lines.ToArray())
            {
                if (!line.IsToggled)
                    continue;

                var vehicle = Vehicle.Spawn(line.VehicleName, route);
                route.AddVehicle(vehicle);

                vehicle.transform.position = (Building as Depot).StopPointsPositions[0];//StopPointsPositions[0];

                RemoveVehicleLine(line);
                Destroy(line.gameObject);
            }
        }



        protected override void OnAwakening()
        {
            m_lines = new List<VehicleLine>();
            m_buttons = new List<VehicleButton>();
        }

        protected override void OnStarting()
        {
            FillSelectionWindow();

            enabled = false;
        }

        protected override void OnShowing()
        {
            base.OnShowing();

            //WindowsController.Instance.WindowsLocked = true;
        }

        protected override void OnHiding()
        {
            //WindowsController.Instance.WindowsLocked = false;
        }

        public override void UpdateContent()
        {
            foreach (var line in m_lines)
            {
                line.SetRoutes();
            }
        }
    }
}
