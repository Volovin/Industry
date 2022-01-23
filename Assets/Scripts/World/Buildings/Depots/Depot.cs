using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Roads;
using Industry.UI.Windows;

namespace Industry.World.Buildings.Depots
{
    public abstract class Depot : EntranceBuilding
    {
        protected string m_vehiclePrefabName;


        protected override void _Awake()
        {
            base._Awake();

            var t = GameObject.Find("Windows").transform;

            m_window = ObjectManager.Instance.Spawn("Depot Window", t) as DepotWindow;
            m_window.Building = this;
        }
    }
}
