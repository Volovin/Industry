using System;
using UnityEngine;

namespace Industry.AI.Routing
{
    /// <summary>
    /// Represents a world position with a collection of neigbouring positions a vehicle can traverse between.
    /// </summary>
    class WayPoint : MonoBehaviour
    {
        /// <summary>
        /// The behavior type of a WayPoint.
        /// </summary>
        internal enum WPtype
        {
            None,
            Linker,
            Stop
        }

        #region Fields

        [SerializeField]
        private WPtype m_type;
        [SerializeField]
        private WayPoint[] m_neighbours;

        #endregion

        #region Properties

        #region Internal properties

        /// <summary>
        /// The type of the waypoint.
        /// </summary>
        internal WPtype Type
        { 
            get
            {
                return m_type;
            }
        }

        /// <summary>
        /// An array of neighbouring waypoints connected to this waypoint.
        /// </summary>
        internal WayPoint[] Links
        {
            get
            {
                return m_neighbours;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Is this waypoint a connector?
        /// </summary>
        public bool IsConnector
        {
            get
            {
                return m_type == WPtype.Linker && gameObject.name.Contains("(C)");
            }
        }
        
        /// <summary>
        /// is this waypoint a receiver?
        /// </summary>
        public bool IsReceiver
        {
            get
            {
                return m_type == WPtype.Linker && gameObject.name.Contains("(R)");
            }
        }

        /// <summary>
        /// The world position of this waypoint.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }

        #endregion

        #endregion

        #region Unity methods

        private void Awake()
        {
            enabled = false;
        }

        #endregion
    }
}
