using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Industry.Utilities;
using Industry.World.Roads;
using Industry.AI.Routing;
using Industry.UI.Windows;

namespace Industry.World.Buildings
{
    public abstract class EntranceBuilding : Building, IClickable
    {
        [SerializeField]
        private WayPointContainer m_WPcontainer;

        [SerializeField]
        private RoadNode m_node;

        [SerializeField]
        private Transform[] m_entrances;


        public RoadNode Node
        {
            get
            {
                return m_node;
            }
        }

        public int EntrancesCount
        {
            get
            {
                return m_entrances.Length;
            }
        }

        public int PlatformsCount
        {
            get
            {
                return m_WPcontainer.StopPointsCount;
            }
        }


        public Vector3[] EntrancesPositions
        {
            get
            {
                return m_entrances.Select(x => x.position).ToArray();
            }
        }

        public Vector3[] StopPointsPositions
        {
            get
            {
                return m_WPcontainer.StopPoints;
            }
        }

        private Transform GetEntrance(Vector3 entrPos)
        {
            return m_entrances.FirstOrDefault(x => x.position.EqualsApprox(entrPos));
        }


        public Vector3 DirectionOf(Vector3 entrPos)
        {
            var entrance = GetEntrance(entrPos);

            if (entrance == null)
                throw new ArgumentException($"No entrance at {entrPos}");

            var marker = entrance.GetChild(0);

            return (marker.position - entrance.position).normalized;
        }

        public bool EntranceAt(Vector3 entrPos)
        {
            return GetEntrance(entrPos) != null;
        }

        public bool EntranceAtAndDirectionEquals(Vector3 entrPos, Vector3 dir)
        {
            var entrance = GetEntrance(entrPos);

            if (entrance == null)
                return false;

            var marker = entrance.GetChild(0);

            return (marker.position - entrance.position).normalized == dir;
        }

        public List<Vector3> GetPathBetween(Vector3 from, Vector3 to, int platform, out Vector3 stopPoint)
        {
            return m_WPcontainer.GetPathBetween(from, to, platform, out stopPoint);
        }

        public Vector3 GetEntranceByDirection(Vector3 dir)
        {
            if (m_entrances.Length == 1)
                return m_entrances[0].position;

            foreach (var entr in m_entrances)
            {
                if ((entr.position - m_node.Position).normalized == dir)
                    return entr.position;
            }

            throw new Exception("No such entrance"); // <---
        }

        public void OnClick()
        {
            if (WindowsController.Instance.WindowsLocked)
                return;

            m_window.Show();
        }

        public override void Destroy()
        {
            m_node.ClearLinks();

            base.Destroy();
        }

        protected override void _Awake()
        {
            base._Awake();

            m_node.enabled = false;

            if (m_WPcontainer == null)
                throw new MissingComponentException("WP Container is missing");
        }
        protected override void _Start()
        {
            //if (m_entrances.Length == 2)
            //    m_entrances[0].AddLink(m_entrances[1]);
        }
        protected override void _Update()
        {

        }
    }
}
