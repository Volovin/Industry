using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Industry.AI.Routing
{
    class WayPointContainer : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private WayPoint[] m_wayPoints;
        [SerializeField]
        private WayPoint[] m_stopPoints;

        #endregion

        #region Properties

        /// <summary>
        /// Total count of the waypoints in the container.
        /// </summary>
        public int Count
        {
            get
            {
                return m_wayPoints.Length;
            }
        }

        /// <summary>
        /// Count of stop points which corresponds to the platforms count.
        /// </summary>
        public int StopPointsCount
        {
            get
            {
                return m_stopPoints.Length;
            }
        }

        /// <summary>
        /// The world positions of all stop points.
        /// </summary>
        public Vector3[] StopPoints
        {
            get
            {
                return m_stopPoints.Select(x => x.Position).ToArray();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the shortest sequence of waypoints between the <paramref name="from"/> and <paramref name="goal"/> waypoints.
        /// </summary>
        /// <param name="from">The start waypoint.</param>
        /// <param name="goal">The end waypoint.</param>
        /// <param name="includeFrom">Defines if the generated sequence includes the <paramref name="from"/> waypoint.</param>
        private List<Vector3> BFS(WayPoint from, WayPoint goal, bool includeFrom)
        {
            var queue = new Queue<WayPoint>(16);
            var parents = new Dictionary<Vector3, Vector3>(16);

            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == goal)
                    break;

                var links = current.Links;

                for (int i = 0; i < links.Length; i++)
                {
                    var link = links[i];

                    if (link == null)
                        continue;

                    var linkPos = link.Position;

                    if (!parents.ContainsKey(linkPos))
                    {
                        parents.Add(linkPos, current.Position);
                        queue.Enqueue(link);
                    }
                }
            }

            var path = new List<Vector3>();

            var currPos = goal.Position;
            var fromPos = from.Position;

            while (currPos != fromPos)
            {
                path.Add(currPos);

                currPos = parents[currPos];
            }

            if (includeFrom)
                path.Add(fromPos);

            //path.Reverse();

            return path;
        }

        /// <summary>
        /// Gets all the Linker waypoints from this container.
        /// </summary>
        private WayPoint[] GetLinkers()
        {
            return m_wayPoints.Where(wp => wp.Type == WayPoint.WPtype.Linker).ToArray();
        }

        /// <summary>
        /// Gets the closest Linker waypoint to the specified position.
        /// </summary>
        private WayPoint WayPointAt(Vector3 pos, bool requireConnector)
        {
            var linkers = m_wayPoints.Where(wp => wp.Type == WayPoint.WPtype.Linker && wp.IsConnector == requireConnector).ToArray();

            for (int i = 0; i < linkers.Length; i++)
            {
                if ((linkers[i].Position - pos).sqrMagnitude < 0.4f)
                    return linkers[i];
            }

            return null;
            //return linkers.First(x => x.IsConnector == requireConnector && (x.Position - pos).sqrMagnitude < 0.4f);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generates the sequence of points based on waypoints positions 
        /// starting at the <paramref name="from"/> position
        /// and ending at the <paramref name="goal"/> position
        /// going through a stop waypoint that corresponds to the specified platform number.
        /// </summary>
        /// <param name="from">The approximate position of the start waypoint.</param>
        /// <param name="goal">The approximate position of the end waypoint.</param>
        /// <param name="platfrom">The index of a platform of an EntranceBuilding.</param>
        /// <param name="stopPoint">The position of the stop waypoint the sequence goes through.</param>
        public List<Vector3> GetPathBetween(Vector3 from, Vector3 goal, int platfrom, out Vector3 stopPoint)
        {
            if (platfrom < 0 || platfrom >= m_stopPoints.Length)
                throw new ArgumentOutOfRangeException("Incorrect platform number: " + platfrom);

            var f = WayPointAt(from, false);
            var g = WayPointAt(goal, true);
            var s = m_stopPoints[platfrom];

            stopPoint = s.Position;

            var path1 = BFS(f, s, true);
            var path2 = BFS(s, g, false);

            path2.AddRange(path1);
            path2.Reverse();

            return path2;
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            if (m_wayPoints == null)
                throw new MissingComponentException("WayPoints are missing");
            if (m_stopPoints == null)
                throw new MissingComponentException("StopPoints are missing");
            
            enabled = false;
            gameObject.SetActive(false);
        }

        #endregion
    }
}
