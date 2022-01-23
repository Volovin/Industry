using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.World.Vehicles;

namespace Industry.AI.Routing
{
    public class RouteController : Singleton<RouteController>
    {
        #region Fields

        private bool m_shouldRebuildRoutes;

        private ushort m_frames;

        private Dictionary<ushort, Route> m_routes;

        #endregion

        #region Properties

        /// <summary>
        /// Determines if all routes should be drawn.
        /// </summary>
        public bool DrawAll
        {
            get; set;
        }

        /// <summary>
        /// The total count of routes.
        /// </summary>
        public int RoutesCount
        {
            get
            {
                return m_routes.Count;
            }
        }

        /// <summary>
        /// Gets the list of all non-temporary routes.
        /// </summary>
        public Route[] Routes
        {
            get
            {
                return m_routes.Values.Where(x => !x.IsTemp).ToArray();
            }
        }

        #endregion

        #region Unity monobehaviour methods

        private void Awake()
        {
            m_routes = new Dictionary<ushort, Route>();
        }

        private void Start()
        {
            transform.SetParent(GameObject.Find("Controllers").transform);
            gameObject.name = "Route Controller";
        }

        private void Update()
        {
            m_frames++;

            CheckRoutes();
        }

        private void OnDrawGizmos()
        {
            //if (frames % 2 != 0)
            //    return;

            if (m_routes == null)
                return;

            foreach (var _route in m_routes)
            {
                var route = _route.Value;

                if (route.IsDaemon)
                    return;

                if (DrawAll || route.Draw)
                    route.Display();
            }

            Gizmos.color = Color.white;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Performs the check procedure on every route and rebuilds the routes that need to be rebuilt if possible.
        /// </summary>
        private void CheckRoutes()
        {
            // Should probably check routes in different frames
            // Multi-threading ???
            if (!m_shouldRebuildRoutes && m_frames != 30)
                return;

            m_frames = 0;

            foreach (var _route in m_routes)
            {
                var route = _route.Value;

                if (route.IsDaemon)
                {
                    UpdateRouteAsDaemon(route);
                }
                else if (m_shouldRebuildRoutes || !route.IsIntact())
                {
                    //Debug.LogWarning($"Recalculating Route {route.ID}...");

                    var start = route.startNode;
                    var end = route.endNode;

                    bool startDead = start == null || !start.IsAlive || start.LinksCount == 0;
                    bool endDead = end == null || !end.IsAlive || end.LinksCount == 0;

                    if (!route.IsTemp)
                    {
                        if (!startDead && !endDead)
                        {
                            UpdateRoute(start, end, route);
                        }
                        else
                        {
                            UpdateRouteAsDaemon(route);
                        }
                    }
                    else
                    {
                        if (endDead)
                        {
                            // both main and temp routes require an end Entrance node
                            Debug.LogWarning($"Unable to recalculate route {route.ID}: End  is destroyed.");

                            continue;
                        }

                        // the vehicle should handle this
                        route.Vehicles[0].UpdateRoute();
                    }
                }
            }

            m_shouldRebuildRoutes = false;
        }

        /// <summary>
        /// Searches for an appropriate road node (entry node) at the specified location.
        /// </summary>
        /// <param name="nodePos">The position of a road node to search at.</param>
        /// <param name="node">The found appropriate road node (if one).</param>
        private bool TryGetNodeAt(Vector3 nodePos, out RoadNode node)
        {
            node = null;

            var entity = WorldMap.GetEntity(nodePos);

            if (entity == null)
                return false;

            if (entity is EntranceBuilding)
            {
                var eb = entity as EntranceBuilding;

                //node = eb.EntranceAt(nodePos);
                node = eb.Node;
            }

            return node != null;
        }

        /// <summary>
        /// Tries to rebuild a damaged route based on the stored internal information.
        /// </summary>
        /// <param name="route">The route to rebuild.</param>
        private void UpdateRouteAsDaemon(Route route)
        {
            // daemon route is one that has no path between
            // its start & end node OR one/both of nodes are destroyed

            // a temporary route cannot be daemon

            if (route.IsTemp)
                return;


            if (!TryGetNodeAt(route.startNodePosition, out var start))
            {
                route.SetDaemon(true);
                return;
            }

            if (!TryGetNodeAt(route.endNodePosition, out var end))
            {
                route.SetDaemon(true);
                return;
            }

            if ((!start.IsEntrance && start.LinksCount != 1) ||
                (!end.IsEntrance && end.LinksCount != 1))
            {
                route.SetDaemon(true);
                return;
            }

            route.SetDaemon(!UpdateRoute(start, end, route));
        }

        /// <summary>
        /// Rebuilds the <paramref name="route"/> between the specified <paramref name="start"/> and <paramref name="end"/> road nodes.
        /// Sets the route to the daemon state if rebuilding operation was not successful.
        /// </summary>
        /// <param name="start">The new start road node.</param>
        /// <param name="end">The new end road node.</param>
        /// <param name="route">The route to rebuild.</param>
        /// <returns></returns>
        private bool UpdateRoute(RoadNode start, RoadNode end, Route route)
        {
            if (route.Set(start, end, route.IsTemp))
            {
                route.SetDaemon(false);

                return true;
            }
            else
            {
                if (route.IsTemp)
                {
                    route.Vehicles[0].UpdateRoute();
                }
                else
                {
                    route.SetDaemon(true);
                }

                return false;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Adds the <paramref name="route"/> to the internal route list.
        /// </summary>
        /// <param name="route">The route to add.</param>
        internal void AddRoute(Route route)
        {
            if (route == null)
                throw new ArgumentNullException("route");

            m_routes.Add(route.ID, route);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the route from the internal route list by its ID.
        /// </summary>
        /// <param name="ID">The unique ID of the route.</param>
        public Route GetRoute(ushort ID)
        {
            m_routes.TryGetValue(ID, out var route);

            return route;
        }

        /// <summary>
        /// Removes the <paramref name="route"/> from the internal route list.
        /// </summary>
        /// <param name="route"></param>
        public void RemoveRoute(Route route)
        {
            if (route == null)
                throw new ArgumentNullException("route");

            route.RemoveAllVehicles();
            m_routes.Remove(route.ID);
        }

        /// <summary>
        /// Removes the route by its unique <paramref name="ID"/> from the internal route list.
        /// </summary>
        /// <param name="ID">The unique ID of the route.</param>
        public void RemoveRoute(ushort ID)
        {
            var route = GetRoute(ID);

            RemoveRoute(route);
        }

        /// <summary>
        /// Sets all the routes to be forcibly updated during the next frame.
        /// </summary>
        public void UpdateRoutes()
        {
            m_shouldRebuildRoutes = true;
        }

        #endregion
    }
}
