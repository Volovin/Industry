using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.Managers;
using Industry.UI.Elements;
using Industry.World.Buildings;

namespace Industry.UI.Windows.Buildings
{
    public abstract class BuildingWindow : UIWindow
    {
        protected Building m_building;


        public Building Building
        {
            get
            {
                return m_building;
            }
            set
            {
                m_building = value;
            }
        }



        public abstract void UpdateContent();


        protected override void OnHiding()
        {
            MainCamera.Instance.CanZoom = true;
        }

        protected override void OnShowing()
        {
            UpdateContent();
        }

        protected override void OnAwakening()
        {

        }

        protected override void OnStarting()
        {
            if (m_building == null)
                throw new Exception("A building must be set.");

            Tittle = m_building.Name;

            //IsShown = false;
        }
    }
}
