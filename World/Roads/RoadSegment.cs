using System;
using UnityEngine;
using Industry.Utilities;
using Industry.World.Map;

namespace Industry.World.Roads
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class RoadSegment : RoadBase
    {
        private Transform m_body;

        public RoadNode[] Nodes
        {
            get; private set;
        }

        public float Length
        {
            get; private set;
        }

        public void SetNodes(RoadNode one, RoadNode two)
        {
            if (one == null || two == null)
                throw new ArgumentNullException("one | two");

            var stripes = gameObject.GetComponentsInChildren<SpriteRenderer>();

            Nodes[0] = one;
            Nodes[1] = two;

            Vector3 pos1 = one.Position;
            Vector3 pos2 = two.Position;
            Vector3 diff = pos2 - pos1;

            float dX = Mathf.Abs(diff.x);
            float dZ = Mathf.Abs(diff.z);
            float scale = Mathf.RoundToInt(Mathf.Max(dX, dZ) / WorldMap.CellSize);

            Length = scale;

            if (dX == dZ)
                scale *= 1.4142f;

            var lS = m_body.localScale;
            lS.y = scale * 1.01f;
            m_body.localScale = lS;

            (m_collider as CapsuleCollider).height = scale - 0.5f;

            transform.position = Vector3.Lerp(pos1, pos2, 0.5f);
            transform.rotation = Quaternion.LookRotation(diff);

            Name = $"[{scale + 1}] RoadSegment";

            int stripesActualCount = stripes.Length - 1;
            int stripesTargetCount = Mathf.RoundToInt(scale) - Mathf.RoundToInt(scale - (int)scale);

            float _add = dX == dZ ? 0.7071f : 0.5f;

            Vector3 cur = new Vector3(0, 0, (-scale / 2f) + _add);
            Vector3 add = new Vector3(0, 0, 1);

            if (stripesActualCount > stripesTargetCount)
            {
                for (int i = stripesTargetCount + 1; i <= stripesActualCount; i++)
                {
                    Destroy(stripes[i].gameObject);
                }
            }

            for (int i = 0; i < stripesTargetCount; i++)
            {
                if (i < stripesActualCount)
                {
                    stripes[i + 1].gameObject.transform.localPosition = cur;
                }
                else
                {
                    var stripe = new GameObject();

                    var sR = stripe.AddComponent<SpriteRenderer>();
                    sR.sprite = ObjectManager.Instance.GetSprite("rect white");
                    sR.sortingOrder = 2;

                    stripe.name = "Stripe";
                    stripe.transform.SetParent(gameObject.transform);

                    stripe.transform.localScale = new Vector3(0.05f, 0.25f, 1f);
                    stripe.transform.localRotation = stripes[0].gameObject.transform.localRotation;
                    stripe.transform.localPosition = cur;
                }

                cur += add;
            }
        }

        public Rect GetBuildingArea(Vector3 side)
        {
            var node1pos = Nodes[0].Position;
            var node2pos = Nodes[1].Position;

            //if (side.IsBetweenOrOnOneLine(node1pos, node2pos))
            //    throw new InvalidOperationException("side cannot lay on the segment line");

            var theVec = node2pos - node1pos;
            var cs = WorldMap.CellSize;

            var cross = -Vector3.Cross(theVec, side - node2pos).y;
            var norm = (Vector3.Cross(theVec, Vector3.up) * cross).normalized;

            norm *= cs;

            theVec.Normalize();
            theVec *= cs;

            var p1 = node1pos + theVec;
            var p2 = node2pos - theVec;

            var min = p1 + norm;
            var max = p2 + norm * 4f;

            Rect rect = Rect.MinMaxRect(min.x, min.z, max.x, max.z);

            var center = rect.center;

            rect.width = Mathf.Abs(rect.width);
            rect.height = Mathf.Abs(rect.height);

            rect.center = center;

            return rect;
        }

        public Rect[] GetBuildingAreas()
        {
            var node1pos = Nodes[0].Position;
            var node2pos = Nodes[1].Position;

            var theVec = node2pos - node1pos;
            var cs = WorldMap.CellSize;

            var norm = Vector3.Cross(theVec, Vector3.up).normalized;
            
            norm *= cs;

            theVec.Normalize();
            theVec *= cs;

            var p1 = node1pos + theVec;
            var p2 = node2pos - theVec;

            var areas = new Rect[2];

            for (int i = 0; i < 2; i++)
            {
                if (i == 1) norm = -norm;

                var min = p1 + norm;
                var max = p2 + norm * 4f;

                Rect rect = Rect.MinMaxRect(min.x, min.z, max.x, max.z);

                var center = rect.center;

                rect.width = Mathf.Abs(rect.width);
                rect.height = Mathf.Abs(rect.height);

                rect.center = center;

                areas[i] = rect;
            }

            return areas;
        }

        public override void Destroy()
        {
            var nodes = Nodes;

            bool node1_valid = nodes[0] != null && nodes[0].IsAlive;
            bool node2_valid = nodes[1] != null && nodes[1].IsAlive;

            if (node1_valid && node2_valid)
                nodes[0].RemoveLink(nodes[1]);

            base.Destroy();
        }

        public override void _Reset()
        {
            var node1 = Nodes[0];
            var node2 = Nodes[1];

            // MUST be linked
            node1.RemoveLink(node2);

            Nodes[0] = null;
            Nodes[1] = null;

            Length = 0f;

            var lS = m_body.localScale;
            lS.y = 1.01f;
            m_body.localScale = lS;

            Name = "[0] RoadSegment";
        }

        protected override void _Awake()
        {
            m_type = RoadType.Segment;

            m_body = GetComponentInChildren<SpriteRenderer>().transform;

            Nodes = new RoadNode[2];

            base._Awake();
        }

        protected override void _Start()
        {
            base._Start();
        }

        protected override void _Update()
        {
            base._Update();
        }

        public override string ToString()
        {
            return $"[{Length}] RoadSegment";
        }
    }
}
