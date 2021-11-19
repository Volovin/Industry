using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.UI.Windows;
using Industry.World.Vehicles;

namespace Industry.UI.Elements
{
    public class VehicleLine : UIElement
    {
        public static VehicleLine CreateLine(DepotWindow parent, string vName, int dropdown_value = 0)
        {
            var vehicleLine = ObjectManager.Instance.Spawn("VehicleLine") as VehicleLine;

            string n = (Vehicle.LastID + parent.LinesCount + 1).ToString();

            vehicleLine.m_window = parent;
            vehicleLine.m_numberText.text = n;
            vehicleLine.m_IField.text = $"{vName} {n}";

            vehicleLine.VehicleName = vName;

            vehicleLine.SetRoutes(dropdown_value);

            return vehicleLine;
        }

        [SerializeField]
        private Toggle m_toggle;
        [SerializeField]
        private Text m_numberText;
        [SerializeField]
        private InputField m_IField;
        [SerializeField]
        private Dropdown m_dropdownRoutes;
        [SerializeField]
        private Text m_routeOptionText; 

        private DepotWindow m_window;

        private List<ushort> m_routeIDs;


        public string Number
        {
            get
            {
                return m_numberText.text;
            }
            set
            {
                m_numberText.text = value;
            }
        }

        public string VehicleName
        {
            get; private set;
        }

        public bool IsToggled
        {
            get { return m_toggle.isOn; }
        }

        public void SetVehicleToRoute()
        {
            if (m_routeIDs.Count == 0)
                return;

            m_toggle.isOn = true;

            var ID = m_routeIDs[m_dropdownRoutes.value];

            m_window.SetSelectedVehiclesToRoute(ID);
        }

        public void DuplicateLine()
        {
            m_window.AddVehicleLine(VehicleName, m_dropdownRoutes.value);
        }

        public void DeleteLine()
        {
            m_window.RemoveVehicleLine(this);

            Destroy(gameObject);
        }

        public void SetRoutes(int dropdown_value = -1)
        {
            m_dropdownRoutes.options.Clear();
            m_routeIDs.Clear();

            var routes = WindowsController.Instance.Windows.RouteManager.RoutesAsStrings;

            if (routes == null || routes.Count == 0)
                return;


            for (int i = 0; i < routes.Count; i++)
            {
                var split = routes[i].Split('`');

                m_routeIDs.Add(ushort.Parse(split[0]));
                m_dropdownRoutes.options.Add(new Dropdown.OptionData(split[1]));
            }

            m_routeOptionText.text = 
                m_dropdownRoutes.options[m_dropdownRoutes.value % (m_dropdownRoutes.options.Count)].text;

            if (dropdown_value != -1)
                m_dropdownRoutes.value = dropdown_value;

            //m_routeOptionText.text = m_dropdownRoutes.options[dropdown_value].text;
        }


        private void Awake()
        {
            m_routeIDs = new List<ushort>();

            enabled = false;
        }
    }
}
