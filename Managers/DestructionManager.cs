using System;
using System.Linq;
using UnityEngine;
using Industry.UI.Elements;
using Industry.UI.Windows;
using Industry.Utilities;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;

namespace Industry.Managers
{
    /// <summary>
    /// The manager that controlls the destruction of GameEntitites.
    /// </summary>
    public class DestructionManager : Singleton<DestructionManager>, IManager
    {
        #region Fields

        private RoadSystem m_roadSysInstance;

        private GameObject m_redFrame;

        private Vector3 m_pos;

        #endregion

        #region Unity methods

        private void Awake()
        {
            m_redFrame = GameObject.Find("RedFrame");
            m_roadSysInstance = RoadSystem.Instance;

            transform.SetParent(GameObject.Find("Controllers").transform);
            gameObject.name = "Destruction Manager";
        }

        private void Update()
        {
            if (!WorldMap.TryGetCenter(out m_pos))
                return;

            m_redFrame.transform.position = m_pos;

            if (Input.GetMouseButtonDown(1))
            {
                Cancel();

                return;
            }
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                var gameEntities = WorldMap.GetEntities(m_pos, WorldMap.CellSize * 0.4f);

                if (gameEntities.Length == 0)
                    return;

                for (int i = 0; i < gameEntities.Length; i++)
                {
                    var entity = gameEntities[i];

                    if (entity is RoadNode)
                        DestroyObject(entity as RoadNode);
                    else if (entity is RoadSegment)
                        DestroyObject(entity as RoadSegment);
                    else if (entity is Building)
                        DestroyObject(entity as Building);
                }
            }
        }

        #endregion

        #region Interface implementation methods

        public void Enable()
        {
            if (!enabled)
            {
                enabled = true;
                m_redFrame.SetActive(true);
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                enabled = false;
                m_redFrame.SetActive(false);

                WindowsController.Instance.WindowsLocked = false;
                ToolBar.Instance.SetButtons(true);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Reverts the internal state to default and deactivates the manager.
        /// </summary>
        private void Cancel()
        {
            Disable();
        }

        /// <summary>
        /// Checks if the specified <paramref name="node"/> became redundant due to deleting its linked nodes.
        /// </summary>
        /// <param name="node"></param>
        private void CheckNodeRedundancy(RoadNode node)
        {
            if (!(node.IsAlive && node.gameObject.activeInHierarchy))
                return;

            if (node.LinksCount == 0 && !node.IsEntrance)
            {
                m_roadSysInstance.DisableAndCache(node);
            }
            else if (node.LinksCount == 2)
            {
                var links = node.Links;

                var link1 = links[0];
                var link2 = links[1];

                var nodePos = node.Position;

                var diff1 = (links[0].Position - nodePos).normalized;
                var diff2 = (links[1].Position - nodePos).normalized;

                if (diff1 + diff2 != Vector3.zero)
                    return;

                var segments =
                    WorldMap.GetEntities(nodePos, WorldMap.CellSize / 2.5f).
                    OfType<RoadSegment>().ToArray();

                if (segments.Length != 2)
                    throw new System.Exception("Segments count != Links count (2)");

                var seg1 = segments[0];
                var seg2 = segments[1];

                m_roadSysInstance.DisableAndCache(node);
                m_roadSysInstance.DisableAndCache(seg2);

                link1.AddLink(link2);
                seg1.SetNodes(link1, link2);
            }
        }

        /// <summary>
        /// Destroys the specified <paramref name="node"/> and checks the affected linked nodes and segments.
        /// </summary>
        private void DestroyObject(RoadNode node)
        {
            if (!node.gameObject.activeInHierarchy || !node.IsAlive)
                return;

            var links = node.Links;

            m_roadSysInstance.DisableAndCache(node);

            foreach (var link in links)
            {
                if (link.LinksCount == 0 && !link.IsEntrance)
                    m_roadSysInstance.DisableAndCache(link);
            }
        }

        /// <summary>
        /// Destroys the specified <paramref name="segment"/> and checks the affected linked nodes and segments.
        /// </summary>
        private void DestroyObject(RoadSegment segment)
        {
            if (!segment.gameObject.activeInHierarchy || !segment.IsAlive)
                return;

            var nodes = segment.Nodes.ToArray(); // clone
            var node1_pos = nodes[0].Position;
            var node2_pos = nodes[1].Position;
            var sqrLen = (node2_pos - node1_pos).sqrMagnitude;

            m_roadSysInstance.DisableAndCache(segment);

            if (Input.GetKey(KeyCode.LeftShift) || sqrLen <= Mathf.Pow(3 * WorldMap.CellSize, 2))
            {
                foreach (var node in nodes)
                {
                    CheckNodeRedundancy(node);
                }
            }
            else
            {
                var dir = (node2_pos - node1_pos).normalized;
                var t_point = dir * WorldMap.CellSize * 3f;

                var p1 = m_pos - t_point;
                var p2 = m_pos + t_point;


                if (p1.IsBetween(node1_pos, m_pos))
                    m_roadSysInstance.CreateRoad(node1_pos, p1, true, false); // <---- diagonals
                else
                    CheckNodeRedundancy(nodes[0]);

                if (p2.IsBetween(node2_pos, m_pos))
                    m_roadSysInstance.CreateRoad(node2_pos, p2, true, false); // <---- diagonals
                else
                    CheckNodeRedundancy(nodes[1]);
            }
        }

        /// <summary>
        /// Destroys the specified <paramref name="building"/> and checks the affected linked nodes and segments.
        /// </summary>
        private void DestroyObject(Building building)
        {
            if (building is EntranceBuilding)
            {
                var node = (building as EntranceBuilding).Node;

                foreach (var link in node.Links)
                {
                    if (!link.IsEntrance && link.LinksCount <= 1)
                        m_roadSysInstance.DisableAndCache(link);

                    var seg =
                        WorldMap.GetEntities(node.Position, WorldMap.CellSize / 2.5f).
                        OfType<RoadSegment>().FirstOrDefault();

                    // if null then it means the segment was destroyed before
                    if (seg != null)
                        m_roadSysInstance.DisableAndCache(seg);
                }
            }

            building.Destroy();
        }

        #endregion
    }
}
