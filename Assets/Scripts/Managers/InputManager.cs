using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.UI.Elements;

namespace Industry.Managers
{
    public class InputManager : Singleton<InputManager>, IManager
    {
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToolBar tb = ToolBar.Instance;
                tb.IsShown = !tb.IsShown;
            }
        }

        #endregion
    }
}
