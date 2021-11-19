using System;
using System.Globalization;
using UnityEngine;

namespace Industry.Utilities
{
    public struct Rect3
    {
        public Rect3(float x, float z, float width, float height)
        {
            m_rect = new Rect(x, z, width, height);
        }

        public Rect3(Vector3 position, Vector3 size, bool posIsCenter = false)
        {
            var pos = position.ToVector2();
            var s = size.ToVector2();

            m_rect = new Rect(pos, s);

            if (posIsCenter)
                m_rect.center = pos;
        }


        private Rect m_rect;


        public float height
        {
            get { return m_rect.height; }
            set { m_rect.height = value; }
        }

        public float width
        {
            get { return m_rect.width; }
            set { m_rect.width = value; }
        }

        public Vector3 center
        {
            get { return m_rect.center.ToVector3(); }
            set { m_rect.center = value.ToVector2(); }
        }

        public Vector3 position
        {
            get { return m_rect.position.ToVector3(); }
            set { m_rect.position = value.ToVector2(); }
        }

        public Vector3 min
        {
            get { return m_rect.min.ToVector3(); }
            set { m_rect.min = value.ToVector2(); }
        }

        public Vector3 max
        {
            get { return m_rect.max.ToVector3(); }
            set { m_rect.max = value.ToVector2(); }
        }

        public Vector3 size
        {
            get { return m_rect.size.ToVector3(); }
            set { m_rect.size = value.ToVector2(); }
        }

        public bool Contains(Vector2 point)
        {
            return m_rect.Contains(point);
        }

        public bool Contains(Vector3 point)
        {
            return m_rect.Contains(point.ToVector2());
        }

        public bool ContainsFully(Rect3 other)
        {
            var _min = other.m_rect.min;
            var _max = other.m_rect.max;

            return m_rect.Contains(_min) && m_rect.Contains(_max);
        }

        public bool Overlaps(Rect3 other)
        {
            var rect2 = new Rect(other.min, other.size);

            return m_rect.Overlaps(rect2);
        }

        public override int GetHashCode()
        {
            return m_rect.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rect3) || obj == null)
                return false;

            return m_rect.Equals(((Rect3)obj).m_rect);
        }

        public override string ToString()
        {
            var ft = "F2";
            var fP = CultureInfo.InvariantCulture.NumberFormat;

            return string.Format("(x:{0}, z:{1}, width:{2}, height:{3})", min.x.ToString(ft, fP), min.z.ToString(ft, fP), width.ToString(ft, fP), height.ToString(ft, fP));
        }

        public static Rect3 zero
        {
            get { return new Rect3(0f, 0f, 0f, 0f); }
        }

        public static Rect3 MinMaxRect(float xMin, float zMin, float xMax, float zMax)
        {
            return new Rect3(xMin, zMin, xMax - xMin, zMax - zMin);
        }

        public static bool operator ==(Rect3 one, Rect3 two)
        {
            return one.Equals(two);
        }
        
        public static bool operator !=(Rect3 one, Rect3 two)
        {
            return !one.Equals(two);
        }
    }
}
