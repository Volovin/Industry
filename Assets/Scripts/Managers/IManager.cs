using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Industry.Managers
{
    /// <summary>
    /// The basic functionalily of all Managers.
    /// </summary>
    internal interface IManager
    {
        /// <summary>
        /// Enables the manager.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the manager.
        /// </summary>
        void Disable();
    }
}
