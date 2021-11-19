using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.AI.Routing;

namespace Industry.World.Roads
{
    public class RoadSystem : Singleton<RoadSystem>
    {
        private int m_maxCacheCapacity;
        private HashSet<RoadBase> m_cachedRoads;

        private Transform m_roadsContainer;


        private RoadBase GetFromCache(RoadType type)
        {
            var item = m_cachedRoads.FirstOrDefault(x => x.Type == type);

            if (item != null)
            {
                m_cachedRoads.Remove(item);
                item.gameObject.SetActive(true);
            }

            return item;
        }

        private List<Vector3> GeneratePoints(Vector3[] convertedPoints, out int idxB)
        {
            if (convertedPoints == null)
                throw new ArgumentNullException("convertedPoints");

            if (convertedPoints.Length < 2 || convertedPoints.Length > 3)
                throw new InvalidOperationException("Length must be 2 or 3");

            List<Vector3> points = WorldMap.PointsBetween(convertedPoints[0], convertedPoints[1]);

            idxB = -1;

            if (convertedPoints.Length == 3)
            {
                idxB = points.Count - 1;
                points.RemoveAt(points.Count - 1);
                points.AddRange(WorldMap.PointsBetween(convertedPoints[1], convertedPoints[2]));
            }

            return points;
        }

        private bool PreProcess(List<Vector3> points, int idxB, out List<Vector3> nodesPoints)
        {
            nodesPoints = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];

                var entity = WorldMap.GetEntity(point);

                bool createNode = i == 0 || i == points.Count - 1 || i == idxB;
                bool cell_free = entity == null;

                if (cell_free && createNode)
                {
                    nodesPoints.Add(point);
                }
                else if (!cell_free)
                {
                    if (entity is RoadBase)
                    {
                        nodesPoints.Add(point);
                    }
                    else if (entity is EntranceBuilding)
                    {
                        var eb = entity as EntranceBuilding;

                        if (!eb.EntranceAt(point))
                            return false;

                        // assuming direction is ok 
                        // since the check must have been done by now

                        nodesPoints.Add(point);
                    }
                    else
                    {
                        nodesPoints = null;

                        return false;
                    }
                }
            }

            return true;
        }

        private RoadBase Spawn(RoadType type, Vector3 position)
        {
            var item = GetFromCache(type);

            if (item != null)
            {
                item.Position = position;

                return item;
            }

            if (type == RoadType.Node)
                return ObjectManager.Instance.Spawn("Road Node", position, Quaternion.identity, m_roadsContainer) as RoadNode;
            else
                return ObjectManager.Instance.Spawn("Road Segment", position, Quaternion.identity, m_roadsContainer) as RoadSegment;
        }


        public Vector3[] Convert(Vector3 pos1, Vector3 pos2, bool xFirst, bool useDiagonal)
        {
            if (pos1 == pos2)
                return null;

            Vector3 diff = pos2 - pos1;
            Vector3 A = pos1, C = pos2;
            Vector3? B = null;

            float tS = WorldMap.CellSize;

            float sX = Mathf.Sign(diff.x);
            float sZ = Mathf.Sign(diff.z);

            int tX = Mathf.Abs(Mathf.RoundToInt(diff.x / tS));
            int tZ = Mathf.Abs(Mathf.RoundToInt(diff.z / tS));

            if (tX != 0 && tZ != 0)
            {
                float _x, _y = A.y, _z;

                if (!useDiagonal)
                {
                    _x = xFirst ? C.x : A.x;
                    _z = xFirst ? A.z : C.z;

                    B = new Vector3(_x, _y, _z);
                }
                else if (tX != tZ)
                {
                    if (xFirst)
                    {
                        _x = tX > tZ ? C.x - sX * sZ * diff.z : C.x;
                        _z = tX > tZ ? A.z : A.z + sZ * sX * diff.x;
                    }
                    else
                    {
                        _x = tX > tZ ? A.x + sX * sZ * diff.z : A.x;
                        _z = tX > tZ ? C.z : C.z - sZ * sX * diff.x;
                    }

                    B = new Vector3(_x, _y, _z);
                }
            }

            List<Vector3> points = new List<Vector3>() { A, C };

            if (B.HasValue)
                points.Insert(1, B.Value);

            return points.ToArray();
        }

        public void CreateRoad(Vector3 pos1, Vector3 pos2, bool xFirst, bool useDiagonal)
        {
            if (pos1 == pos2)
                return;

            // Pre Process
            var points_V = Convert(pos1, pos2, xFirst, useDiagonal);
            var points_A = GeneratePoints(points_V, out int idxB);

            if (points_A == null)
                throw new InvalidOperationException("Failed to generate points");

            var nodesSeq = new List<RoadNode>();

            if (!PreProcess(points_A, idxB, out var nodesPoints))
            {
                Debug.LogWarning("PreProcess failed.");
                return;
            }

            // Nodes sequence formation and existing segments division
            foreach (var point in nodesPoints)
            {
                var entity = WorldMap.GetEntity(point);

                bool cell_free = entity == null;

                if (cell_free)
                {
                    var node = Spawn(RoadType.Node, point) as RoadNode;

                    nodesSeq.Add(node);
                }
                else
                {
                    if (entity is RoadNode)
                    {
                        nodesSeq.Add(entity as RoadNode);
                    }
                    else if (entity is RoadSegment)
                    {
                        var segment = entity as RoadSegment;
                        var links = segment.Nodes;

                        var link1 = links[0];
                        var link2 = links[1];

                        var node = Spawn(RoadType.Node, point) as RoadNode;
                        nodesSeq.Add(node);

                        link1.RemoveLink(link2);

                        link1.AddLink(node);
                        link2.AddLink(node);

                        segment.SetNodes(link1, node);

                        var newSegment = Spawn(RoadType.Segment, point) as RoadSegment;
                        newSegment.SetNodes(node, link2);
                        newSegment.ColliderEnabled = true;
                    }
                    else if (entity is EntranceBuilding)
                    {
                        var eb = entity as EntranceBuilding;

                        nodesSeq.Add(eb.Node);
                    }
                    else throw new Exception($"Entity type {entity.GetType()} is not allowed.");
                }
            }

            // Main Process
            for (int i = 0; i < nodesSeq.Count - 1; i++)
            {
                RoadNode node1 = nodesSeq[i];
                RoadNode node2 = nodesSeq[i + 1];

                if (node1.ContainsLink(node2))
                    continue;

                node1.AddLink(node2);

                node1.ColliderEnabled = true;
                node2.ColliderEnabled = true;

                var newSegment = Spawn(RoadType.Segment, Vector3.zero) as RoadSegment;
                newSegment.SetNodes(node1, node2);
                newSegment.ColliderEnabled = true;
            }

            // Post Process
            for (int i = 0; i < nodesSeq.Count; i++)
            {
                RoadNode node = nodesSeq[i];

                if (node.IsEntrance || node.LinksCount != 2)
                    continue;

                var links = node.Links;

                var link1 = links[0];
                var link2 = links[1];

                var pos_node = node.Position;
                var pos_link1 = link1.Position;
                var pos_link2 = link2.Position;

                var diff1 = pos_link1 - pos_node;
                var diff2 = pos_link2 - pos_node;

                if (diff1.normalized + diff2.normalized != Vector3.zero)
                    continue;

                var middlePoint1 = Vector3.Lerp(pos_node, pos_link1, 0.5f);
                var middlePoint2 = Vector3.Lerp(pos_node, pos_link2, 0.5f);

                var segment1 = WorldMap.GetEntities(middlePoint1, 0.01f)[0] as RoadSegment;
                var segment2 = WorldMap.GetEntities(middlePoint2, 0.01f)[0] as RoadSegment;

                if (segment1 == null || segment2 == null)
                    throw new Exception("Well some stupid shit occured");

                DisableAndCache(node);
                DisableAndCache(segment2);

                link1.AddLink(link2);
                segment1.SetNodes(link1, link2);
            }

            RouteController.Instance.UpdateRoutes();
        }

        public void DisableAndCache(RoadBase item)
        {
            if (item == null || !item.IsAlive)
                throw new ArgumentException("item is null or dead");

            if (!item.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Item is already disabled");
                return;
            }

            if (m_cachedRoads.Count < m_maxCacheCapacity)
            {
                item._Reset();

                item.gameObject.SetActive(false);

                m_cachedRoads.Add(item);
            }
            else
            {
                item.Destroy();
            }
        }

        private bool QuickCheck(Vector3 point)
        {
            var entity = WorldMap.GetEntity(point);

            if (entity == null)
                return true;

            if (entity is RoadBase)
                return true;

            if (entity is EntranceBuilding)
            {
                var eb = entity as EntranceBuilding;

                if (eb.EntranceAt(point)) // inaccurate but ok
                    return true;
            }

            return false;
        }

        private bool QuickCheck(Vector3 from, Vector3 to)
        {
            var entities = WorldMap.GetEntities(from, to);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[0];

                if (entity is RoadBase)
                    continue;

                if (entity is EntranceBuilding)
                {
                    var eb = entity as EntranceBuilding;

                    // accurate enough
                    var lineDir = (to - from).normalized;

                    if (eb.EntranceAtAndDirectionEquals(from, lineDir))
                        continue;
                    else if (eb.EntranceAtAndDirectionEquals(to, -lineDir))
                        continue;
                    else return false;
                }

                return false;
            }

            return true;
        }

        public bool QuickCheck(params Vector3[] points)
        {
            if (points == null || points.Length == 0)
                throw new ArgumentException("empty points");

            if (points.Length == 1)
                return QuickCheck(points[0]);

            for (int i = 1; i < points.Length; i++)
                if (!QuickCheck(points[i - 1], points[i]))
                    return false;

            return true;
        }

        private void Awake()
        {
            m_maxCacheCapacity = 16;
            m_cachedRoads = new HashSet<RoadBase>();
        }

        private void Start()
        {
            m_roadsContainer = new GameObject("Roads Container").transform;
            m_roadsContainer.SetParent(GameObject.Find("Main Container").transform);

            enabled = false;
        }
    }
}
