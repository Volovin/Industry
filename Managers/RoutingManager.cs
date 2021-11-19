using UnityEngine;
using Industry.AI.Routing;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.UI.Elements;
using Industry.UI.Windows;

namespace Industry.Managers
{
    /// <summary>
    /// The manager that controlls the gathering of information required to build a route.
    /// </summary>
    public class RoutingManager : Singleton<RoutingManager>, IManager
    {
        #region Fields

        private RoadNode m_from;
        private RoadNode m_to;

        private RouteEditingInfo? m_routeEditingInfo;

        #endregion

        #region Unity methods

        private void Awake()
        {
            transform.SetParent(GameObject.Find("Controllers").transform);
            gameObject.name = "Routing Manager";
        }

        private void Update()
        {
            InputRoute();

            if (m_routeEditingInfo.HasValue)
            {
                EditRoute();
            }
            else
            {
                CreateNewRoute();
            }
        }

        #endregion

        #region Interface implementation methods

        public void Enable()
        {
            if (!enabled)
            {
                enabled = true;

                //Cursor.SetCursor(Prefabs.Instance.Get<Sprite>("Cursor Pointer").texture, new Vector2(250, 500), CursorMode.Auto);
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                enabled = false;

                m_from = null;
                m_to = null;
                m_routeEditingInfo = null;

                //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                WindowsController.Instance.Windows?.RouteManager.
                    SetButtonsToActiveLine(true);

                WindowsController.Instance.WindowsLocked = false;
                ToolBar.Instance.SetButtons(true);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates a new Route between two road nodes.
        /// </summary>
        private void CreateNewRoute()
        {
            if (m_from == null || m_to == null)
                return;

            var route = Route.Create(m_from.Position, m_to.Position, 0, 0);

            if (route != null)
            {
                SendFeedback(route);
            }

            Disable();
        }

        /// <summary>
        /// Edits an existing route using the Route Editing Info.
        /// </summary>
        private void EditRoute()
        {
            var info = m_routeEditingInfo.Value;
            var route = RouteController.Instance.GetRoute(info.routeID);

            if (info.rebuildRoute)
            {
                if (m_from == null || m_to == null)
                    return;

                if (route.startNode == m_from && route.endNode == m_to)
                {
                    Debug.Log("Trying to set the same Nodes");

                    Disable();
                    
                    return;
                }

                if (route.Set(m_from, m_to, 0, 0))
                {
                    SendFeedback(route);
                }
                else
                {
                    Debug.LogWarning("Failed to set the route " + route.ID);
                }
            }
            else
            {
                // daemon check?
                if (route.Set(route.startNode, route.endNode, info.platform1, info.platform2))
                {
                    // No need to send feedback
                }
                else
                {
                    Debug.LogWarning("Failed to set the route " + route.ID);
                }

            }

            Disable();
        }

        /// <summary>
        /// Hadles the user input to define the start and end route points.
        /// </summary>
        private void InputRoute()
        {
            if (Input.GetMouseButtonDown(1)) { Disable(); return; }

            if (Input.GetMouseButtonDown(0))
            {
                if (!WorldMap.TryGetCenter(out var mPos))
                    return;

                var entity = WorldMap.GetEntity(mPos);

                if (entity == null)
                    return;

                RoadNode road;

                if (entity is EntranceBuilding)
                {
                    road = (entity as EntranceBuilding).Node; // <----
                }
                else if (entity is RoadNode)
                {
                    road = entity as RoadNode;
                }
                else
                {
                    return;
                }


                if (road.LinksCount != 1 && !road.IsEntrance)
                    return;

                if (m_from == null)
                {
                    m_from = road;
                }
                else if (m_to == null)
                {
                    m_to = road;
                }
            }
        }

        /// <summary>
        /// Sends the route data to a UI Route Line that represents the route.
        /// </summary>
        /// <param name="route"></param>
        private void SendFeedback(Route route)
        {
            int[] platformsCount = new int[2];

            var building1 = route.startNode.Building;
            var building2 = route.endNode.Building;

            if (building1 != null)
                platformsCount[0] = building1.PlatformsCount;

            if (building2 != null)
                platformsCount[1] = building2.PlatformsCount;

            WindowsController.Instance.Windows.RouteManager.
                SetRouteInfoToActiveLine(route.ID, platformsCount);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the Route Ending Info that used to edit an existing route.
        /// </summary>
        /// <param name="routeID">The unique ID of the routeto edit.</param>
        /// <param name="rebuildRoute">Defines if the route needs to be rebuilt with different points.</param>
        /// <param name="platform1">The index of a platform of the Building the start entry node is part of.</param>
        /// <param name="platform2">The index of a platform of the Building the end entry node is part of.</param>
        public void SetRouteEditingInfo(ushort routeID, bool rebuildRoute, int platform1, int platform2)
        {
            m_routeEditingInfo = new RouteEditingInfo(routeID, rebuildRoute, platform1, platform2);
        }

        #endregion

        #region Nested structs

        /// <summary>
        /// Used as a communication mediator between <see cref="RouteManagerWindow"/> UI and the <see cref="RoutingManager"/>.
        /// </summary>
        private struct RouteEditingInfo
        {
            public RouteEditingInfo(ushort routeID, bool rebuildRoute, int platform1, int platform2)
            {
                this.routeID = routeID;
                this.rebuildRoute = rebuildRoute;
                this.platform1 = platform1;
                this.platform2 = platform2;
            }

            public readonly ushort routeID;
            public readonly bool rebuildRoute;
            public readonly int platform1;
            public readonly int platform2;
        }

        #endregion
    }
}
