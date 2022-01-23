using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings.CityBuildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.Utilities;

namespace Industry.Managers
{
    public class CitiesManager : Singleton<CitiesManager>, IManager
    {
        #region Fields

        [SerializeField]
        private bool m_toBuild;

        [SerializeField]
        private bool m_buildInOneFrame;

        private int m_mode;

        [SerializeField]
        [Range(1, 10)]
        private int m_citiesCount;

        [SerializeField]
        [Range(10, 20)]
        private int m_range;

        [SerializeField]
        private List<string> m_buildingNames;

        private List<City> m_cities;

        private List<Rect3> m_rectAreas;

        private Vector3[] m_positions;
        private Rect3[] m_gridsBounds;

        #endregion

        #region Interface implementation methods

        public void Enable()
        {
            // Should never be disabled.
        }

        public void Disable()
        {
            // 
        }


        #endregion

        #region Unity methods

        private void Awake()
        {
            m_cities = new List<City>(8);

            m_rectAreas = new List<Rect3>();

            if (m_buildingNames.Count == 0)
                Debug.LogWarning("BuilingNames list is empty");
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (m_toBuild)
            {

                if (m_buildInOneFrame)
                    BuildSingleFrame();
                else
                    BuildSeparateFrame();
            }

            for (int i = 0; i < m_cities.Count; i++)
            {
                m_cities[i].Update();
            }
        }

        #endregion

        #region Private Methods

        private void ClearMap()
        {
            m_cities.Clear();

            var e = WorldMap.GetEntities(Vector3.zero, WorldMap.Bounds.width);

            for (int i = 0; i < e.Length; i++)
            {
                if (e[i].CompareTag("Water"))
                    continue;

                if (e[i] is RoadBase)
                    RoadSystem.Instance.DisableAndCache(e[i] as RoadBase);
                else
                    e[i].Destroy();
            }
        }

        private void BuildSingleFrame()
        {
            m_toBuild = false;

            ClearMap();

            m_positions = GeneratePositions(m_citiesCount, 50 * m_range * m_range);
            m_gridsBounds = GenerateGrids(m_positions);

            LinkGrids(m_gridsBounds);

            GenerateBuildings(m_gridsBounds);

            for (int i = 0; i < m_gridsBounds.Length; i++)
                m_cities.Add(new City(m_gridsBounds[i]));

            m_positions = null;
            m_gridsBounds = null;
        }

        private void BuildSeparateFrame()
        {
            if (m_mode == 0)
            {
                ClearMap();
            }
            else if (m_mode == 1)
            {
                m_positions = GeneratePositions(m_citiesCount, 50 * m_range * m_range);
                m_gridsBounds = GenerateGrids(m_positions);
            }
            else
            {
                LinkGrids(m_gridsBounds);

                GenerateBuildings(m_gridsBounds);

                for (int i = 0; i < m_gridsBounds.Length; i++)
                    m_cities.Add(new City(m_gridsBounds[i]));

                m_positions = null;
                m_gridsBounds = null;

                m_toBuild = false;
            }

            m_mode = (m_mode + 1) % 3;
        }

        private Vector3[] GeneratePositions(int count, float minSqrDistance, Rect3 bounds = default)
        {
            if (count < 0)
                throw new ArgumentException("count < 0");

            if (bounds.width == 0f) // if default
            {
                var offset = m_range * 4;
                var size = WorldMap.Bounds.size - new Vector3(offset, 0f, offset);
                bounds = new Rect3(Vector3.zero, size, true);
            }

            var min = bounds.min;
            var max = bounds.max;

            var positions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                int p = 0;

                Vector3 center = min;

                while (p++ < count * 10)
                {
                    center = new Vector3(Range(min.x, max.x), 0f, Range(min.z, max.z));

                    if (i == 0) break;

                    float minSqrDist = float.MaxValue;

                    for (int c = 0; c < i; c++)
                    {
                        var sqrDist = (center - positions[c]).sqrMagnitude;

                        if (sqrDist < minSqrDist)
                            minSqrDist = sqrDist;
                    }

                    if (minSqrDist > minSqrDistance)
                        break;
                }

                positions[i] = center;
            }

            return positions;
        }

        private Rect3[] GenerateGrids(Vector3[] positions)
        {
            if (positions == null)
                throw new ArgumentNullException("positions");

            var gridsBounds = new Rect3[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                gridsBounds[i] = GenerateGrid(positions[i]);
            }

            return gridsBounds;
        }

        private Rect3 GenerateGrid(Vector3 center)
        {
            BuildGrid(center, m_range, m_range + 10, out var min0, out var max0);

            var p1 = new Vector3(min0.x, 0f, min0.z);
            var p2 = new Vector3(min0.x, 0f, max0.z);
            var p3 = new Vector3(max0.x, 0f, min0.z);
            var p4 = new Vector3(max0.x, 0f, max0.z);

            var points = new Vector3[4] { p1, p2, p3, p4 };

            var absmin = WorldMap.Bounds.max;
            var absmax = WorldMap.Bounds.min;

            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];

                int lenX = Range(m_range - i * 2, m_range + i * 3);
                int lenZ = Range(m_range - i * 2, m_range + i * 3);

                BuildGrid(points[i], lenX, lenZ, out var min, out var max);

                float PX_x = i < 2 ? min.x : max.x;
                float PX_z = p.z;
                float PZ_x = p.x;
                float PZ_z = i % 2 == 0 ? min.z : max.z;

                Vector3 px = new Vector3(PX_x, 0f, PX_z);
                Vector3 pz = new Vector3(PZ_x, 0f, PZ_z);

                RoadSystem.Instance.CreateRoad(px, pz, true, false);

                if (min.x < absmin.x) absmin.x = min.x;
                if (min.z < absmin.z) absmin.z = min.z;
                if (max.x > absmax.x) absmax.x = max.x;
                if (max.z > absmax.z) absmax.z = max.z;
            }

            absmin -= new Vector3(4.5f, 0f, 4.5f);
            absmax += new Vector3(4.5f, 0f, 4.5f);

            return new Rect3(absmin, absmax - absmin);
        }

        private void LinkGrids(Rect3[] gridsBounds)
        {
            if (gridsBounds == null)
                throw new ArgumentNullException("gridBounds");

            var linkingRoadsVectors = new List<ValuesPair<Vector3>>(gridsBounds.Length * 3 + 1);

            for (int i = 0; i < gridsBounds.Length; i++)
            {
                var pair = new ValuesPair<Rect3>(gridsBounds[i], gridsBounds[(i + 1) % gridsBounds.Length]);

                LinkGrids(pair, linkingRoadsVectors);

                if (i % 2 == 0)
                {
                    pair.B = gridsBounds[(i + 2) % gridsBounds.Length];
                    LinkGrids(pair, linkingRoadsVectors);
                }
            }
        }

        private void LinkGrids(ValuesPair<Rect3> gridsPair, List<ValuesPair<Vector3>> linkingRoadsVectors)
        {
            var g1nodes =
                WorldMap.GetEntitiesRect(gridsPair.A).
                OfType<RoadNode>().
                Where(x => x.LinksCount > 1 && x.LinksCount < 4).
                ToArray();

            var g2nodes =
                WorldMap.GetEntitiesRect(gridsPair.B).
                OfType<RoadNode>().
                Where(x => x.LinksCount > 1 && x.LinksCount < 4).
                ToArray();

            float minDist = float.MaxValue;
            var minPair = new ValuesPair<Vector3>();

            for (int i = 0; i < g1nodes.Length; i++)
            {
                var posI = g1nodes[i].Position;

                for (int j = 0; j < g2nodes.Length; j++)
                {
                    var posJ = g2nodes[j].Position;

                    float dist = (posJ - posI).sqrMagnitude;

                    if (dist < minDist)
                    {
                        minDist = dist;
                        minPair.A = posI;
                        minPair.B = posJ;
                    }

                }
            }

            if (linkingRoadsVectors != null)
            {
                for (int i = 0; i < linkingRoadsVectors.Count; i++)
                {
                    var a = linkingRoadsVectors[i].A;
                    var b = linkingRoadsVectors[i].B;

                    var p = minPair.B.ClosestPointOnLineSegment(a, b);

                    float dist = (minPair.B - p).sqrMagnitude;

                    if (dist < minDist && !gridsPair.A.Contains(p))
                    {
                        minDist = dist;

                        minPair.A = p;
                    }
                }
            }

            var p01 = minPair.A;
            var p02 = minPair.B;
            var cc = WorldMap.GetCenter(Vector3.Lerp(p01, p02, 0.5f));
            var p11 = new Vector3(cc.x, 0f, p01.z);
            var p22 = new Vector3(cc.x, 0f, p02.z);
            var diff = p02 - p01;
            var xF = diff.x > diff.z;

            RoadSystem.Instance.CreateRoad(p01, p11, xF, false);
            RoadSystem.Instance.CreateRoad(p02, p22, xF, false);
            RoadSystem.Instance.CreateRoad(p11, p22, !xF, false);


            if (linkingRoadsVectors != null)
            {
                if (p01 != p11)
                    linkingRoadsVectors.Add(new ValuesPair<Vector3>(p01, p11));
                if (p11 != p22)
                    linkingRoadsVectors.Add(new ValuesPair<Vector3>(p11, p22));
                if (p02 != p22)
                    linkingRoadsVectors.Add(new ValuesPair<Vector3>(p02, p22));
            }
        }

        private void GenerateBuildings(Rect3[] cityBounds)
        {
            if (cityBounds == null)
                throw new ArgumentNullException("cityBounds");

            for (int i = 0; i < cityBounds.Length; i++)
            {
                PlaceBuildings(GenerateAreas(cityBounds[i]));
            }
        }

        private void BuildGrid(Vector3 center, int lenX, int lenZ, out Vector3 min, out Vector3 max)
        {
            center = WorldMap.GetCenter(center);

            min = center - new Vector3(lenX / 2, 0f, lenZ / 2);
            max = center + new Vector3(lenX / 2, 0f, lenZ / 2);

            var rs = RoadSystem.Instance;

            rs.CreateRoad(min, max, true, false);
            rs.CreateRoad(min, max, false, false);
        }

        private void PlaceBuildings(Dictionary<RoadSegment, Rect3[]> areas)
        {
            if (areas == null || areas.Count == 0)
            {
                Debug.LogError("Error initializing areas");

                return;
            }

            int bCount = m_buildingNames.Count;
            int div = areas.Count / bCount;
            int targetBuild = div * bCount;


            var segments = areas.Keys.ToArray();

            CityBuilding building = null;

            var r = new Randomizer(segments.Length);

            for (int i = 0; i < targetBuild; i++)
            {
                int b = Range(0, m_buildingNames.Count);
                var bName = m_buildingNames[b];

                if (building == null)
                    building = ObjectManager.Instance.Spawn(bName) as CityBuilding;

                r.Reset(segments.Length);

                while (true)
                {
                    int s = r.Next();

                    if (s == -1)
                    {
                        building.Destroy();
                        break;
                    }

                    var segment = segments[s];
                    var currAreas = areas[segment];

                    if (currAreas.Length == 0)
                        continue;

                    int a = Range(0, currAreas.Length);

                    var area = currAreas[a];

                    if (FindPosition(segment, area, building.Rect, out var pos))
                    {
                        var clp = pos.ClosestPointOnLineSegment(segment.Nodes[0].Position, segment.Nodes[1].Position);
                        var segDir = (clp - pos).normalized;

                        PlaceBuilding(building, pos, segDir);

                        building = null;

                        break;
                    }
                }
            }
        }

        private void PlaceBuilding(CityBuilding building, Vector3 position, Vector3 directionToSegment)
        {
            if (directionToSegment == Vector3.back)
            {
                // do nothing
            }
            else if (directionToSegment == Vector3.forward)
            {
                building.Rotate(true);
                building.Rotate(true);
            }
            else if (directionToSegment == Vector3.right)
            {
                building.Rotate(false);
            }
            else
            {
                building.Rotate(true);
            }

            building.SetPositionNoCheck(position);
            building.ColliderEnabled = true;
        }

        private bool FindPosition(RoadSegment segment, Rect3 area, Rect3 subarea, out Vector3 position)
        {
            var subAreaHalfSize = subarea.size * 0.5f;
            var subAreaCheckSize = subarea.size * 0.6f;

            subAreaHalfSize.y = 1f;
            subAreaCheckSize.y = 1f;

            position = Vector3.zero;

            if (area.width < subarea.width || area.height < subarea.height)
                return false;

            if (area.width == subarea.width && area.height == subarea.height)
            {
                if (WorldMap.IsAreaFree(area.center, subAreaHalfSize))
                {
                    position = area.center;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            var curr = area.min;

            var up = Vector3.forward * WorldMap.CellSize;
            var right = Vector3.right * WorldMap.CellSize;

            int lenX = Mathf.RoundToInt(area.width - subarea.width) + 1;
            int lenZ = Mathf.RoundToInt(area.height - subarea.height) + 1;

            for (int i = 0; i < lenZ; i++)
            {
                for (int j = 0; j < lenX; j++)
                {
                    subarea.position = curr;

                    curr += right;

                    var center = subarea.center;

                    if (!WorldMap.IsAreaFree(center, subAreaHalfSize))
                        continue;

                    var e = WorldMap.GetEntitiesRect(center, subAreaCheckSize);

                    position = center;

                    for (int k = 0; k < e.Length; k++)
                        if (e[k] == segment)
                            return true;
                }

                curr.x = area.min.x;
                curr += up;
            }

            return false;
        }

        private Dictionary<RoadSegment, Rect3[]> GenerateAreas(Rect3 cityBounds)
        {
            var entities = WorldMap.GetEntitiesRect(cityBounds);

            m_rectAreas = new List<Rect3>(entities.Length + 1);

            var segAreas = new Dictionary<RoadSegment, Rect3[]>(entities.Length + 1);

            for (int i = 0; i < entities.Length; i++)
            {
                var seg = entities[i] as RoadSegment;

                if (seg == null ||
                    seg.Length < 2 ||
                    !cityBounds.Contains(seg.Nodes[0].Position) ||
                    !cityBounds.Contains(seg.Nodes[1].Position))
                    continue;

                var areas = ProcessSegmentAreas(seg);
                segAreas.Add(seg, areas);
            }

            return segAreas;
        }

        private Rect3[] ProcessSegmentAreas(RoadSegment segment)
        {
            var cs = WorldMap.CellSize;

            var segmentNode1 = segment.Nodes[0].Position;
            var segmentNode2 = segment.Nodes[1].Position;

            var dir = segmentNode2 - segmentNode1;

            var normal = Vector3.Cross(dir, Vector3.up);

            var length = Mathf.Abs(Mathf.RoundToInt(normal.x == 0 ? normal.z : normal.x)) - 1;

            normal.Normalize();
            dir.Normalize();
            dir *= cs;

            var p0 = segmentNode1 + dir;

            var areas = new List<Rect3>();

            for (int k = 0; k < 2; k++)
            {
                if (k == 1)
                    normal = -normal;

                var p = p0 + normal;

                var prevLen = -1;

                for (int i = 0; i < length; i++)
                {
                    int linelen = 4;

                    while (linelen > 0)
                    {
                        if (WorldMap.IsLineFree(p, p + (linelen - 1) * cs * normal))
                            break;

                        linelen--;
                    }

                    if (linelen == 0)
                    {
                        prevLen = 0;
                        continue;
                    }
                    else if (linelen == prevLen)
                    {
                        var prevRect = areas[areas.Count - 1];
                        var prevCenter = prevRect.center;

                        if (dir.x == 0f)
                        {
                            prevRect.height += cs;
                            prevRect.center = new Vector3(prevCenter.x, 0f, prevCenter.z + dir.z * cs * 0.5f);
                        }
                        else
                        {
                            prevRect.width += cs;
                            prevRect.center = new Vector3(prevCenter.x + dir.x * cs * 0.5f, 0f, prevCenter.z);
                        }

                        areas[areas.Count - 1] = prevRect;
                    }
                    else
                    {
                        var centerpos = p + (linelen - 1) * normal * cs * 0.5f;
                        var boxsize = normal.Absified() * cs * linelen;

                        if (boxsize.x == 0f)
                            boxsize.x = cs;
                        else
                            boxsize.z = cs;

                        var rect = new Rect3(centerpos, boxsize, true);

                        areas.Add(rect);
                    }

                    prevLen = linelen;
                    p += dir;
                }
            }

            m_rectAreas.AddRange(areas);

            return areas.ToArray();
        }


        private int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        private float Range(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        #endregion

        #region Public Methods



        #endregion
    }
}
