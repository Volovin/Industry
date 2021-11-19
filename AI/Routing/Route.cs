using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.AI.Routing.PathCreator;
using Industry.Utilities;
using Industry.Utilities.Collections;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.World.Vehicles;

namespace Industry.AI.Routing
{
    /// <summary>
    /// Represents the Route of a <see cref="Vehicle"/> 
    /// which contains a collection of road nodes and points 
    /// as well as information about a vehicle's movement behaviour.
    /// </summary>
    public class Route
    {
        #region Contructors

        private Route()
        {
            routeIDs++;
            ID = routeIDs;

            m_vehicles = new HashSet<Vehicle>();
        }

        #endregion

        #region Fields

        private Path m_path;

        private List<RoadNode> m_nodes;

        private HashSet<Vehicle> m_vehicles;

        #endregion

        #region Properties

        #region Internal Properties

        /// <summary>
        /// The total length of the route.
        /// </summary>
        //internal float Length
        //{
        //    get
        //    {
        //        return m_path.Length;
        //    }
        //}

        /// <summary> 
        /// The first out of 2 nodes in the route.
        /// </summary>
        internal RoadNode startNode
        {
            get; private set;
        }

        /// <summary> 
        /// The second out of 2 nodes in the route.
        /// </summary>
        internal RoadNode endNode
        {
            get; private set;
        }

        //internal List<Vector3> Points
        //{
        //    get
        //    {
        //        return m_path.Points;
        //    }
        //}

        /// <summary>
        /// The number of Vector3 points in the route.
        /// </summary>
        internal int PointsCount
        { 
            get { return m_path.PointsCount; }
        }

        #endregion

        #region Public Properties

        public Color Color
        {
            get; set;
        }

        public bool Draw
        {
            get; set;
        }

        public bool IsDaemon
        {
            get; private set;
        }

        public bool IsTemp
        {
            get; private set;
        }

        public ushort ID
        {
            get; private set;
        }

        public int Platform1
        {
            get; private set;
        }

        public int Platform2
        {
            get; private set;
        }

        public Vector3 startPoint
        {
            get; private set;
        }

        public Vector3 endPoint
        {
            get; private set;
        }

        public Vector3 startNodePosition
        {
            get; private set;
        }

        public Vector3 endNodePosition
        {
            get; private set;
        }

        public Vehicle[] Vehicles
        {
            get
            {
                return m_vehicles.ToArray();
            }
        }

        #endregion

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a <see cref="Vehicle"/> to the route.
        /// </summary>
        /// <param name="vehicle">The vehicle to add.</param>
        public void AddVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException("vehicle");

            m_vehicles.Add(vehicle);
        }

        /// <summary>
        /// Checks if the route contains the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        public bool Contains(Vehicle vehicle)
        {
            return m_vehicles.Contains(vehicle);
        }

        /*
        /// <summary>
        /// Checks if the route path contains the specified point.
        /// </summary>
        /// <param name="worldPos">The point to check.</param>
        //public bool Contains(Vector3 worldPos)
        //{
        //    return GetClosestPointOnPath(worldPos).EqualsApprox(worldPos);
        //}

        /// <summary>
        /// Gets the closest distance value the specified point is located along the route path.
        /// </summary>
        //public float GetClosestDistanceAlongPath(Vector3 point)
        //{
        //    return m_path.GetClosestDistanceAlongPath(point);
        //}

        /// <summary>
        /// Gets the closest to the specified point location along the route path. 
        /// </summary>
        //public Vector3 GetClosestPointOnPath(Vector3 point)
        //{
        //    return m_path.GetClosestPointOnPath(point);
        //}

        /// <summary>
        /// Gets the closest to the specified distance value location along the route path.
        /// </summary>
        //public Vector3 GetPointAtDistance(float distance)
        //{
        //    var instruction = IsTemp ?
        //        EndOfPathInstruction.Stop :
        //        EndOfPathInstruction.Loop;

        //    return m_path.GetPointAtDistance(distance, instruction);
        //}
        */

        /// <summary>
        /// Checks if the route path contains the specified point.
        /// </summary>
        /// <param name="worldPos">The point to check.</param>
        /// <param name="index">The index of the leading point in the path for the <paramref name="point"/>.</param>
        public bool Contains(Vector3 worldPos, out int index)
        {
            return m_path.Contains(worldPos, out index);
        }

        /// <summary>
        /// Gets the closest to the specified point location within the path, starting the search from the <paramref name="index"/>. 
        /// </summary>
        /// <param name="point">The world point to search for the closest point to.</param>
        /// <param name="index">The index of the leading point in the path for the <paramref name="point"/>.</param>
        public Vector3 GetClosestPointOnPath(Vector3 point, out int index)
        {
            return m_path.GetClosestPointOnPath(point, out index);
        }

        /// <summary>
        /// Gets a rotation smoothly aligned with the following path segments.
        /// </summary>
        /// <param name="point">The point to get a rotaion at.</param>
        /// <param name="knownIdx">[CAUTION IN USE] The index of the leading point in the path. Used to skip the search of it.</param>
        public Quaternion GetRotationAt(Vector3 point, int knownIdx = -1)
        {
            return m_path.GetRotationAt(point, knownIdx);
        }

        /// <summary>
        /// Gets a point that is the <paramref name="from"/> point moved towards its leading point by <paramref name="distance"/>.
        /// </summary>
        /// <param name="from">The point to move.</param>
        /// <param name="distance">The value to move the <paramref name="from"/> point by.</param>
        /// <param name="newIndex">The index of the moved point's leading point. Changes if the leading point was passed by after the movement.</param>
        /// <param name="knownIndex">[CAUTION IN USE] The index of the leading point in the path. Used to skip the search of it.</param>
        public Vector3 MovePoint(Vector3 from, float distance, out int newIndex, int knownIndex = -1)
        {
            newIndex = knownIndex;

            if (distance <= 0f)
                return from;

            return m_path.Move(from, distance, out newIndex, knownIndex);
        }

        /// <summary>
        /// Checks if the road nodes sequence is damaged.
        /// </summary>
        /// <returns>True if the road nodes sequence is intact.</returns>
        public bool IsIntact()
        {
            int a;
            int count = m_nodes.Count;

            if (!IsTemp)
            {
                a = count / 2 + 1;

                if (a == count)
                    a--;
            }
            else
            {
                a = count - 1;
            }

            for (int i = 0; i < a; i++)
            {
                var node1 = m_nodes[i];
                var node2 = m_nodes[i + 1];

                if (node1 == null || !node1.IsAlive ||
                    node2 == null || !node2.IsAlive ||
                    node1.LinksCount == 0 ||
                    node2.LinksCount == 0)
                    return false;

                if (!node1.ContainsLink(node2))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Recalculates and updates the route to be the path between <paramref name="from"/> and <paramref name="to"/> road nodes.
        /// </summary>
        /// <param name="from">The start road node.</param>
        /// <param name="to">The end road node.</param>
        /// <param name="isTemp">Defines if the route becomes temporary.</param>
        /// <returns>True if route recalculation is successful.</returns>
        public bool Set(RoadNode from, RoadNode to, bool isTemp = false)
        {
            return Set(from, to, Platform1, Platform2, isTemp);
        }

        /// <summary>
        /// Recalculates and updates the route to be the path between the road nodes 
        /// located at <paramref name="from"/> and <paramref name="to"/> positions.
        /// </summary>
        /// <param name="from">The start road node position.</param>
        /// <param name="to">The end road node position.</param>
        /// <param name="isTemp">Defines if the route becomes temporary.</param>
        /// <returns>True if route recalculation is successful.</returns>
        public bool Set(Vector3 from, Vector3 to, bool isTemp = false)
        {
            return Set(from, to, Platform1, Platform2, isTemp);
        }

        /// <summary>
        /// Recalculates and updates the route to be the path between <paramref name="from"/> and <paramref name="to"/> road nodes
        /// ending at the specified platforms of an <see cref="EntranceBuilding"/>.
        /// </summary>
        /// <param name="from">The start road node.</param>
        /// <param name="to">The end road node.</param>
        /// <param name="platform1">The index of a Building platform the <paramref name="from"/> entry node is part of.</param>
        /// <param name="platform2">The index of a Building platform the <paramref name="to"/> entry node is part of.</param>
        /// <param name="isTemp">Defines if the route becomes temporary.</param>
        /// <returns>True if route recalculation is successful.</returns>
        public bool Set(RoadNode from, RoadNode to, int platform1, int platform2, bool isTemp = false)
        {
            if (from == null || to == null)
                throw new ArgumentNullException("from/to");

            var path = AStarSearch(from, to);

            if (path == null)
                return false;

            var points = CreatePoints(path, platform1, platform2, isTemp, out var s, out var e);

            if (points == null)
                return false;

            m_path = new Path(points, !isTemp);
            m_nodes = path;

            startNode = from;
            endNode = to;

            startNodePosition = from.Position;
            endNodePosition = to.Position;

            startPoint = s;
            endPoint = e;

            Platform1 = platform1;
            Platform2 = platform2;

            IsTemp = isTemp;

            foreach (Vehicle v in m_vehicles)
            {
                v.UpdateRoute();
            }

            return true;
        }

        /// <summary>
        /// Recalculates and updates the route to be the path between <paramref name="from"/> and <paramref name="to"/> road nodes
        /// ending at the specified platforms of an <see cref="EntranceBuilding"/>.
        /// </summary>
        /// <param name="from">The start road node position.</param>
        /// <param name="to">The end road node position.</param>
        /// <param name="platform1">The index of a Building platform the entry node located at <paramref name="from"/> is part of.</param>
        /// <param name="platform2">The index of a Building platform the entry node located at <paramref name="to"/> is part of.</param>
        /// <param name="isTemp">Defines if the route becomes temporary.</param>
        /// <returns>True if route recalculation is successful.</returns>
        public bool Set(Vector3 from, Vector3 to, int platform1, int platform2, bool isTemp = false)
        {
            from = WorldMap.GetCenter(from);
            to = WorldMap.GetCenter(to);

            if (from == to)
                return false;

            var f_node = GetNodeAt(from);
            var e_node = GetNodeAt(to);

            if (f_node == null || e_node == null)
                return false;

            return Set(f_node, e_node, platform1, platform2, isTemp);
        }

        /// <summary>
        /// Sets the route state to be daemon or not.
        /// </summary>
        public void SetDaemon(bool isDaemon)
        {
            if (IsTemp)
                return;

            if (isDaemon != IsDaemon)
            {
                if (isDaemon)
                {
                    foreach (var veh in m_vehicles)
                        veh.Abort("The route of this vehicle is daemon.");
                }
                else
                {
                    foreach (var veh in m_vehicles)
                        veh.Restart("The route of this vehicle was rebuilt.");
                }

                IsDaemon = isDaemon;
            }
        }

        /// <summary>
        /// Sets another route to all vehicles the route contains.
        /// </summary>
        /// <param name="route">The route to set. If <see cref="null"/>, all vehicles of this route will be suspended.</param>
        public void SetRouteToAllVehicles(Route route)
        {
            foreach (var veh in m_vehicles)
            {
                if (route != null)
                    veh.SetRoute(route);
                else
                    veh.Abort("Removed from the main route.");
            }

            m_vehicles.Clear();
        }

        /// <summary>
        /// Removes the specified vehicle from the route.
        /// </summary>
        public void RemoveVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException("vehicle");

            m_vehicles.Remove(vehicle);
        }

        /// <summary>
        /// Removes all vehicles from the route and suspends them.
        /// </summary>
        public void RemoveAllVehicles()
        {
            if (!IsTemp)
            {
                SetRouteToAllVehicles(null);
            }
        }

        /// <summary>
        /// Displays the route with the GizmosDraw call.
        /// </summary>
        public void Display()
        {
            m_path.Display(Color);
        }

        #endregion

        #region Static fields

        // Keeps track of the routes.
        private static ushort routeIDs;

        #endregion

        #region Private static methods

        /// <summary>
        /// Performs the search for a path between the <paramref name="from"/> and <paramref name="goal"/> road nodes.
        /// </summary>
        /// <returns>The sequence of road nodes between the <paramref name="from"/> and <paramref name="goal"/> and back.</returns>
        private static List<RoadNode> AStarSearch(RoadNode from, RoadNode goal)
        {
            if (from == goal)
                return null;

            var cameFrom = new Dictionary<RoadNode, RoadNode>();
            var costSoFar = new Dictionary<RoadNode, float>();

            var frontier = new PriorityQueue<RoadNode, float>();
            frontier.Enqueue(from, 0f);

            cameFrom[from] = from;
            costSoFar[from] = 0f;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                var links = current.Links;

                for (int i = 0; i < links.Length; i++)
                {
                    var link = links[i];
                    float newCost = costSoFar[current] + Cost(current, link);

                    if (!costSoFar.ContainsKey(link) || newCost < costSoFar[link])
                    {
                        costSoFar[link] = newCost;

                        float priority = newCost + Heuristic(link, goal);

                        frontier.Enqueue(link, priority);
                        cameFrom[link] = current;
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal))
            {
                //No path was found.
                return null;
            }

            var path = new List<RoadNode>();

            var curr = goal;

            while (curr != from)
            {
                path.Add(curr);
                curr = cameFrom[curr];
            }

            var backPath = new List<RoadNode>(path); // copy
            backPath.RemoveAt(0);

            path.Add(from);
            path.Reverse();
            path.AddRange(backPath);

            return path;
        }

        /// <summary>
        /// Calculates the sequence of points along the <paramref name="path"/>,
        /// ending at the specified platforms of the Buildings the start and end entry road nodes are part of.
        /// </summary>
        /// <param name="path">The sequence of road nodes.</param>
        /// <param name="platform1">The index of a Building platform the entry node located at <paramref name="from"/> is part of.</param>
        /// <param name="platform2">The index of a Building platform the entry node located at <paramref name="to"/> is part of.</param>
        /// <param name="isTemp">Defines if points sequence of 'back' path is generated based on if the route is temporary. If true, <paramref name="platform1"/> is disregarded.</param>
        /// <param name="startPoint">The generated position of the stop point at the <paramref name="platform1"/></param>
        /// <param name="endPoint">The generated position of the stop point at the <paramref name="platform2"/></param>
        private static List<Vector3> CreatePoints(List<RoadNode> path, int platform1, int platform2, bool isTemp, out Vector3 startPoint, out Vector3 endPoint)
        {
            startPoint = Vector3.zero;
            endPoint = Vector3.zero;

            var nodes = path.Select(x => x.Position).ToList();
            var points = new List<Vector3>();

            int indexOfEnd = -1;
            int count = nodes.Count;

            int eIdx = count == 2 ? 1 : count / 2;

            var s_node = path[0];
            var e_node = path[eIdx];

            var pointPairs = new List<Vector3[]>();

            float dist = 0.375f * 0.5f * WorldMap.CellSize;

            for (int i = 0; i < count; i++)
            {
                int prevIdx = i - 1 < 0 ? count - 1 : i - 1;
                int nextIdx = (i + 1) % count;
                int overNextIdx = (i + 2) % count;

                var prevNode = nodes[prevIdx];
                var currNode = nodes[i];
                var nextNode = nodes[nextIdx];
                var overNextNode = nodes[overNextIdx];

                var prevDir = (currNode - prevNode).normalized;
                var currDir = (nextNode - currNode).normalized;
                var nextDir = (overNextNode - nextNode).normalized;

                var norm = Vector3.Cross(-currDir, Vector3.up).normalized;

                Vector3 point1, point2;
                Vector3? point3 = null;

                if (!prevDir.EqualsApprox(currDir))
                {
                    float dir1 = Vector3.Cross(prevDir, currDir).y;

                    if (dir1 == -1f || dir1 == 0f)
                        point1 = currNode + norm * dist;
                    else if (dir1 > -1f && dir1 < 0f)
                        point1 = currNode + norm * dist;
                    else if (dir1 > 0f && dir1 < 1f)
                        point1 = currNode + norm * dist + currDir * dist * 3.5f;
                    else
                        point1 = currNode + norm * dist + currDir * dist * 2f;

                    if (path[i].IsEntrance)
                    {
                        var dir = (prevNode - currNode).normalized;
                        var entrPos = path[i].Building.GetEntranceByDirection(dir);
                        var diff = currNode - entrPos;

                        point1 -= diff;

                        var pointOpp = point1 - norm * dist * 2f;
                        pointPairs.Add(new Vector3[2] { pointOpp, point1 });
                    }
                    else
                    {
                        points.Add(point1 - currDir * 0.05f);
                        points.Add(point1);
                    }
                }

                if (!currDir.EqualsApprox(nextDir))
                {
                    float dir2 = Vector3.Cross(currDir, nextDir).y;

                    if (dir2 == -1f)
                        point2 = nextNode + norm * dist;
                    else if (dir2 > -1f && dir2 < 0f)
                        point2 = nextNode + norm * dist;
                    else if (dir2 == 0f)
                    {
                        point2 = nextNode + norm * dist;
                        point3 = nextNode + currDir * dist;

                        if (indexOfEnd == -1)
                            indexOfEnd = points.Count - 1;
                    }
                    else if (dir2 > 0f && dir2 < 1f)
                        point2 = nextNode + norm * dist - currDir * dist * 3.5f;
                    else
                        point2 = nextNode + norm * dist - currDir * dist * 2f;


                    if (!path[nextIdx].IsEntrance)
                    {
                        points.Add(point2);
                        points.Add(point2 + currDir * 0.05f);

                        if (point3.HasValue)
                        {
                            if (endPoint == Vector3.zero)
                            {
                                endPoint = point3.Value;
                                points.Add(point3.Value);
                            }
                            else
                            {
                                startPoint = point3.Value;
                                points.Insert(0, point3.Value);
                            }
                        }
                    }
                }
            }

            if (pointPairs.Count > 0)
            {
                int last = pointPairs.Count - 1;

                var pts2 = e_node.Building.GetPathBetween(pointPairs[last][0], pointPairs[last][1], platform2, out endPoint);

                int _idx = points.Count / 2;
                indexOfEnd = _idx + pts2.Count / 2;

                points.InsertRange(_idx, pts2);

                if (isTemp)
                {
                    if (s_node.IsEntrance)
                    {
                        var pts1 = s_node.Building.GetPathBetween(pointPairs[0][0], pointPairs[0][1], platform1, out startPoint);
                        points.InsertRange(0, pts1);

                        var start = pts1.Count / 2 - 1;

                        return points.GetRange(start, indexOfEnd + start);
                    }
                    else
                    {
                        return points.GetRange(0, indexOfEnd + 1);
                    }
                }

                if (pointPairs.Count == 2)
                {
                    var pts1 = s_node.Building.GetPathBetween(pointPairs[0][0], pointPairs[0][1], platform1, out startPoint);

                    points.AddRange(pts1);
                }

                return points;
            }
            else
            {
                return isTemp ? points.GetRange(2, indexOfEnd + 2) : points;
            }
        }

        /// <summary>
        /// Retrieves the road node, a regular or an entry one, located at the <paramref name="pos"/> position if one is there indeed.
        /// If a road segment is located at the specified position, the nearest road node linked to it is returned.
        /// </summary>
        private static RoadNode GetNodeAt(Vector3 pos)
        {
            var entity = WorldMap.GetEntity(pos);

            if (entity == null)
                return null;

            if (entity is RoadNode)
            {
                return entity as RoadNode;
            }
            else if (entity is RoadSegment)
            {
                var nodes = (entity as RoadSegment).Nodes;

                var dist1 = (nodes[0].Position - pos).sqrMagnitude;
                var dist2 = (nodes[1].Position - pos).sqrMagnitude;

                return dist1 < dist2 ? nodes[0] : nodes[1];
            }
            else if (entity is EntranceBuilding)
            {
                return (entity as EntranceBuilding).Node; // <----
            }

            return null;
        }

        /// <summary>
        /// Gets the cost of traversing between <paramref name="a"/> and <paramref name="b"/> road nodes.
        /// </summary>
        private static float Cost(RoadNode a, RoadNode b)
        {
            return Heuristic(a, b);
        }

        /// <summary>
        /// Gets the heuristic estimate of <paramref name="a"/> and <paramref name="b"/> road nodes relative position.
        /// </summary>
        private static float Heuristic(RoadNode a, RoadNode b)
        {
            //return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
            return (a.Position - b.Position).sqrMagnitude;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Generates the route between <paramref name="from"/> and <paramref name="to"/> road nodes based on specified parameters.
        /// </summary>
        /// <param name="from">The start road node.</param>
        /// <param name="to">The end road node.</param>
        /// <param name="platform1">The index of a Building platform the entry node located at <paramref name="from"/> is part of.</param>
        /// <param name="platform2">The index of a Building platform the entry node located at <paramref name="to"/> is part of.</param>
        /// <param name="isTemp">Defines if the route is temporary. If true, <paramref name="platform1"/> is disregarded.</param>
        public static Route Create(RoadNode from, RoadNode to, int platform1, int platform2, bool isTemp = false, bool spawnTruck = false)
        {
            if (from == null || to == null || from == to)
                return null;

            var path = AStarSearch(from, to);

            if (path == null)
                return null;

            var points = CreatePoints(path, platform1, platform2, isTemp, out var s, out var e);

            var _path = new Path(points, !isTemp);

            var route = new Route()
            {
                m_path = _path,
                m_nodes = path,

                startNode = from,
                endNode = to,

                startNodePosition = from.Position,
                endNodePosition = to.Position,

                startPoint = s,
                endPoint = e,

                Platform1 = platform1,
                Platform2 = platform2,

                IsTemp = isTemp,
                Color = Color.white
            };

            RouteController.Instance.AddRoute(route);

            if (spawnTruck)
            {
                var truck = Vehicle.Spawn("Truck", route);

                route.AddVehicle(truck);
            }

            return route;
        }

        /// <summary>
        /// Generates the route between the road nodes located at <paramref name="from"/> and <paramref name="to"/> based on specified parameters.
        /// </summary>
        /// <param name="from">The start road node.</param>
        /// <param name="to">The end road node.</param>
        /// <param name="platform1">The index of a Building platform the entry node located at <paramref name="from"/> is part of.</param>
        /// <param name="platform2">The index of a Building platform the entry node located at <paramref name="to"/> is part of.</param>
        /// <param name="isTemp">Defines if the route is temporary. If true, <paramref name="platform1"/> is disregarded.</param>
        public static Route Create(Vector3 from, Vector3 to, int platform1, int platform2, bool isTemp = false, bool spawnTruck = false)
        {
            from = WorldMap.GetCenter(from);
            to = WorldMap.GetCenter(to);

            if (from == to)
                return null;

            var f_node = GetNodeAt(from);
            var e_node = GetNodeAt(to);

            return Create(f_node, e_node, platform1, platform2, isTemp, spawnTruck);
        }

        #endregion
    }
}
