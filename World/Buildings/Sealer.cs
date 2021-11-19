using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.UI.Windows.Buildings;

namespace Industry.World.Buildings
{
    public class Sealer : EntranceBuilding
    {
        protected override void _Awake()
        {
            base._Awake();

            var t = GameObject.Find("Windows").transform;

            m_window = ObjectManager.Instance.Spawn("BuildingWindow", t) as BuildingWindow;
            m_window.Building = this;
        }

        protected override void _Start()
        {
            base._Start();
        }
        protected override void _Update()
        {
            base._Update();

        }
    }
}
