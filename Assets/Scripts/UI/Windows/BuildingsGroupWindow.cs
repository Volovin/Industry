using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.Managers;
using Industry.UI.Elements;
using Industry.World.Buildings;

namespace Industry.UI.Windows
{
    public class BuildingsGroupWindow : UIWindow
    {
        private List<BuildingButton> m_buttons;

        [SerializeField]
        private ScrollBar m_scrollBar;



        public void AddButton(string buildingName)
        {
            var bButton = BuildingButton.CreateButton(buildingName, this);

            bButton.transform.SetParent(m_scrollBar.Content, false);

            var vps = m_scrollBar.ViewportSize;
            var btnSize = bButton.ButtonSize;

            float spaceX = (vps.x - btnSize.x * 2) / 3;
            float spaceY = (vps.y - btnSize.y * 2) / 3;
            float posX, posY;

            if (m_buttons.Count == 0)
            {
                Vector2 topLeft = new Vector2(-vps.x / 2, m_scrollBar.Content.sizeDelta.y / 2);

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




        public void StartBuildingManager(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return;

            Close();

            BuildingManager.Instance.SetBuilding(prefabName);
            MainManager.Instance.EnableManager("BuildingManager");
        }




        protected override void OnHiding()
        {
            //WindowsController.Instance.WindowsLocked = false;
            MainCamera.Instance.CanZoom = true;
        }

        protected override void OnShowing()
        {
            //WindowsController.Instance.WindowsLocked = true;
            MainCamera.Instance.CanZoom = false;
        }

        protected override void OnAwakening()
        {
            m_buttons = new List<BuildingButton>();

        }

        protected override void OnStarting()
        {

            //AddButton(BuildingButton.CreateButton("House 2x2"));
            //AddButton(BuildingButton.CreateButton("Garage 3x3"));
            //AddButton(BuildingButton.CreateButton("Warehouse 4x4"));
            //AddButton(BuildingButton.CreateButton("Gas Station 3x3"));
            //AddButton(BuildingButton.CreateButton("Saw Mill"));
            //AddButton(BuildingButton.CreateButton("Depot"));
            
            //m_scrollBar.AddSeparator(250);

            enabled = false;
        }
    }
}
