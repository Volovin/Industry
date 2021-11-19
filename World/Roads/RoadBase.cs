using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.AI.Routing;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.Utilities;

namespace Industry.World.Roads
{
    public enum RoadColor
    {
        Default,
        Transparent,
        Red
    }

    public enum RoadType
    {
        Node,
        Segment
    }

    public abstract class RoadBase : GameEntity
    {
        [SerializeField]
        protected GameObject[] m_stripes;
        [SerializeField]
        protected Collider m_collider;
        [SerializeField]
        protected SpriteRenderer m_sr;

        private Color m_defaultColor;
        private RoadColor m_color;
        protected RoadType m_type;

        internal bool ColliderEnabled
        {
            get
            {
                return m_collider != null && m_collider.enabled;
            }
            set
            {
                if (m_collider != null)
                    m_collider.enabled = value;
            }
        }


        public RoadColor Color
        {
            get
            {
                return m_color;
            }
            set
            {
                if (m_color == value)
                    return;

                Color col;

                switch (value)
                {
                    case RoadColor.Default:
                        col = m_defaultColor;
                        break;

                    case RoadColor.Red:
                        col = UnityEngine.Color.red;
                        break;

                    case RoadColor.Transparent:
                        col = m_defaultColor;
                        col.a = 0.5f;
                        break;

                    default:
                        throw new NotSupportedException("RColor " + value.ToString());
                }

                m_sr.color = col;
                m_color = value;
            }
        }
        public RoadType Type
        {
            get
            {
                return m_type;
            }
        }

        public new Vector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }

        public abstract void _Reset();

        protected override void _Awake()
        {
            m_collider.enabled = false;

            m_defaultColor = m_sr.color;
        }

        protected override void _Start()
        {
            enabled = false;
        }

        protected override void _Update()
        {
            Debug.LogWarning("Should not be updating!");
        }

        public override void Destroy()
        {
            ColliderEnabled = false;

            base.Destroy();
        }
    }
}
