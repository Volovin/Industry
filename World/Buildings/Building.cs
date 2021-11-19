using System;
using System.Linq;
using UnityEngine;
using Industry.UI.Windows.Buildings;
using Industry.World.Map;
using Industry.Utilities;

namespace Industry.World.Buildings
{
    public enum BuildingColor
    {
        Default, Red
    }

    public enum PlacingEnvironment
    { 
        Soil,
        Water,
        All
    }

    [RequireComponent(typeof(BoxCollider))]
    public abstract class Building : GameEntity
    {
        [SerializeField]
        protected BuildingColor m_color;
        [SerializeField]
        protected PlacingEnvironment m_environment;
        [SerializeField]
        protected bool m_unlocked;

        protected BuildingWindow m_window;

        [SerializeField]
        private int m_code;
        [SerializeField]
        private int m_countX;
        [SerializeField]
        private int m_countZ;
        [SerializeField]
        private Vector3 posOffset;
        [SerializeField]
        private BoxCollider m_collider;

        private MeshRenderer m_meshRenderer;
        private Color[] m_defaultColors;
        private Color m_cColor;

        public bool ColliderEnabled
        { 
            get
            {
                return m_collider.enabled;
            }
            set
            {
                m_collider.enabled = value;
            }
        }

        public Rect3 Rect
        {
            get; private set;
        }

        public new Vector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                int mX = 1 - CountX % 2;
                int mZ = 1 - CountZ % 2;
                float mts2 = WorldMap.CellSize / 2;

                float X = value.x + mts2 * mX;
                float Z = value.z + mts2 * mZ;

                transform.position = new Vector3(X, value.y, Z) + posOffset;

                var rect = Rect;
                rect.center = transform.position;
                Rect = rect;
            }
        }


        public BuildingColor Color
        {
            get
            {
                return m_color;
            }
            set
            {
                if (m_color == value)
                    return;

                if (m_meshRenderer != null)
                {
                    switch (value)
                    {
                        case BuildingColor.Default:

                            for (int i = 0; i < m_defaultColors.Length; i++)
                                m_meshRenderer.materials[i].color = m_defaultColors[i];
                            break;
                        case BuildingColor.Red:

                            for (int i = 0; i < m_defaultColors.Length; i++)
                                m_meshRenderer.materials[i].color = UnityEngine.Color.red;
                            break;
                        default:

                            for (int i = 0; i < m_defaultColors.Length; i++)
                                m_meshRenderer.materials[i].color = m_defaultColors[i];
                            break;
                    }
                }

                this.m_color = value;
            }
        }

        public Color CustomColor
        {
            get
            {
                return m_cColor;
            }
            set
            {
                for (int i = 0; i < m_defaultColors.Length; i++)
                    m_meshRenderer.materials[i].color = value;
            }
        }


        public int Code
        {
            get
            {
                return m_code;
            }
            protected set
            {
                m_code = value;
            }
        }
        public int CountX
        {
            get
            {
                return m_countX;
            }
            protected set
            {
                m_countX = value;
            }
        }
        public int CountZ
        {
            get
            {
                return m_countZ;
            }
            protected set
            {
                m_countZ = value;
            }
        }


        protected void SetPositionNoCheck(Vector3 position)
        {
            transform.position = position + posOffset;

            var rect = Rect;
            rect.center = transform.position;
            Rect = rect;
        }

        public virtual void Rotate(bool clockwise)
        {
            int temp = CountX;
            CountX = CountZ;
            CountZ = temp;

            float angle = clockwise ? 90 : -90;

            var pos = new Vector3(Position.x, 0f, Position.z);
            var size = new Vector3(m_countX, 0f, m_countZ);

            Rect = new Rect3(pos, size, true);

            transform.rotation = Quaternion.Euler(0f, Mathf.RoundToInt(transform.rotation.eulerAngles.y + angle), 0.0f);
        }


        protected override void _Awake()
        {
            if (Code < 1)
                throw new ArgumentException(gameObject.name + ": Code is not set!");
            if (CountX < 1 || CountZ < 1)
                throw new ArgumentException(gameObject.name + ": Size is not set!");

            var pos = new Vector3(Position.x, 0f, Position.z);
            var size = new Vector3(m_countX, 0f, m_countZ);

            Rect = new Rect3(pos, size, true);

            m_collider.enabled = false;

            m_meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (m_meshRenderer != null)
            {
                m_defaultColors = m_meshRenderer.materials.Select(x => x.color).ToArray();
            }
        }        
    }
}
