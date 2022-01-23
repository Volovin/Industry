using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Buildings;
using Industry.World.Map;

namespace Industry.World.Roads
{
    [RequireComponent(typeof(SphereCollider))]
    public class RoadNode : RoadBase
    {
        private bool[] m_blocks;

        private List<RoadNode> m_links;


        [SerializeField]
        private bool m_isEntrance;
        [SerializeField]
        private EntranceBuilding m_eBuilding;


        public bool IsEntrance
        {
            get
            {
                return m_isEntrance;
            }
        }

        public int LinksCount
        {
            get
            {
                return m_links.Count;
            }
        }

        public RoadNode[] Links
        {
            get
            {
                return m_links.ToArray();
            }
        }

        public EntranceBuilding Building
        {
            get
            {
                return m_eBuilding;
            }
        }


        private int IndexOfBlock(Vector2Int xz)
        {
            return IndexOfBlock(xz.x, xz.y);
        }

        private int IndexOfBlock(int x, int z)
        {
            if (x < -1 || x > 1 || z < -1 || z > 1)
                throw new ArgumentException($"X: {x}; Z: {z}");

            if (x == 0)
                return z == 1 ? 0 : 2;
            else if (z == 0)
                return x == 1 ? 1 : 3;
            else if (x == 1)
                return z == 1 ? 4 : 5;
            else
                return z == 1 ? 7 : 6;
        }

        private Vector2Int RelativePos2XZ(Vector3 relativePos)
        {
            var norm = relativePos.normalized;

            float tS = WorldMap.CellSize;

            int x = (int)Mathf.Clamp(norm.x * 2 * tS, -tS, tS);
            int z = (int)Mathf.Clamp(norm.z * 2 * tS, -tS, tS);

            return new Vector2Int(x, z);
        }

        private void SetBlockActive(int index, bool active)
        {
            m_blocks[index] = active;

            int len = m_blocks.Length;

            for (int i = 0; i < len / 2; i++)
            {
                if (i >= len - 2)
                {
                    return;
                }

                if (m_blocks[i] && m_blocks[i + 2])
                {
                    m_sr.enabled = false;
                    return;
                }

                if (m_blocks[(i + 4) % len] && m_blocks[(i + 6) % len])
                {
                    m_sr.enabled = false;
                    return;
                }
            }

            m_sr.enabled = true;


            //m_stripes[0].SetActive(m_links.Count > 2);
        }

        public void AddLink(params RoadNode[] roads)
        {
            if (roads == null)
                throw new ArgumentNullException("roads");

            foreach (var road in roads)
            {
                if (road == null)
                    throw new ArgumentNullException("road");

                if (!road.IsAlive)
                    throw new ArgumentException("road is dead");

                if (m_links.Contains(road))
                {
                    //Debug.LogWarning($"Trying to add an existing link: {road}, {road.Position}");
                    return;
                }

                var xz = RelativePos2XZ(road.Position - this.Position);

                int i1 = IndexOfBlock(xz);
                int i2 = IndexOfBlock(-xz);

                if (!this.m_isEntrance && m_blocks[i1])
                {
                    //Debug.LogWarning($"Trying to add a link to existing position: {road}, {road.Position}");
                    return;
                }

                this.m_links.Add(road);
                road.m_links.Add(this);

                if (!this.m_isEntrance)
                {
                    this.SetBlockActive(i1, true);
                }

                if (!road.m_isEntrance)
                {
                    road.SetBlockActive(i2, true);

                    road.Name = road.ToString();
                }
            }

            if (!m_isEntrance)
            {
                Name = ToString();
            }
        }

        public bool ContainsLink(RoadNode other)
        {
            if (other == null)
                return false;

            return m_links.Contains(other);
        }

        public void ClearLinks()
        {
            if (m_links.Count == 0)
                return;

            var links = m_links.ToArray();

            for (int i = 0; i < links.Length; i++)
            {
                RemoveLink(links[i]);
            }

            if (m_links.Count != 0)
                throw new Exception("Incorrect links clear");

            Name = ToString();
        }

        public void RemoveLink(RoadNode road)
        {
            if (road == null)
                throw new ArgumentNullException("road");

            if (!m_links.Contains(road))
                return;


            var xz = RelativePos2XZ(road.Position - this.Position);

            int i1 = IndexOfBlock(xz);
            int i2 = IndexOfBlock(-xz);

            if (!m_isEntrance && !m_blocks[i1])
            {
                //Debug.LogWarning($"Trying to remove a link from non-existing position: {road}, {road.Position}");
                return;
            }

            this.m_links.Remove(road);
            road.m_links.Remove(this);

            if (!this.m_isEntrance)
            {
                this.SetBlockActive(i1, false);

                this.Name = this.ToString();
            }

            if (!road.m_isEntrance)
            {
                road.SetBlockActive(i2, false);

                road.Name = road.ToString();
            }
        }


        protected override void _Awake()
        {
            m_type = RoadType.Node;

            m_blocks = new bool[8];

            m_links = new List<RoadNode>();

            if (m_isEntrance)
                return;

            base._Awake();
        }

        protected override void _Start()
        {
            gameObject.name = $"[{m_links.Count}] RoadNode";

            base._Start();
        }

        protected override void _Update()
        {
            base._Update();
        }

        public override void _Reset()
        {
            ClearLinks();
        }

        public override void Destroy()
        {
            ClearLinks();

            base.Destroy();
        }

        public override string ToString()
        {
            return $"[{m_links.Count}] RoadNode";
        }
    }
}
