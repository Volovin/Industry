using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Map;
using Industry.UI.Elements;
using Industry.UI.Windows;

namespace Industry.Managers
{
    /// <summary>
    /// The manager that controlls all the managers and the toolbar.
    /// </summary>
    public class MainManager : Singleton<MainManager>, IManager
    {
        #region Fields

        private Dictionary<string, IManager> m_managers;

        #endregion

        #region Interface implemantation methods

        public void Disable()
        {
            // Should never be disabled.
        }
        public void Enable()
        {
            //
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            WorldMap.Initialize();

            ToolBar.Instance.IsShown = !ToolBar.Instance.IsShown;

            m_managers = new Dictionary<string, IManager>
            {
                { nameof(MainManager), MainManager.Instance },
                { nameof(InputManager), InputManager.Instance },
                { nameof(BuildingManager), BuildingManager.Instance },
                { nameof(DestructionManager), DestructionManager.Instance },
                { nameof(RoadManager), RoadManager.Instance },
                { nameof(RoutingManager), RoutingManager.Instance },
                { nameof(CitiesManager), CitiesManager.Instance }
            };

            foreach (var manager in m_managers)
            {
                manager.Value.Disable();
            }
        }

        private void Update()
        {

        }

        #endregion

        #region Public methods

        /// <summary>
        /// Enables the manager of name <paramref name="managerName"/>.
        /// </summary>
        /// <param name="managerName"></param>
        public void EnableManager(string managerName)
        {
            ToolBar.Instance.SetButtons(false);
            WindowsController.Instance.WindowsLocked = true;

            var manager = m_managers[managerName];
            manager.Enable();
        }

        #endregion
    }
}
