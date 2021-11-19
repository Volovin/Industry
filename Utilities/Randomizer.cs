using System;
using System.Collections.Generic;

namespace Industry.Utilities
{
    public struct Randomizer
    {
        public Randomizer(int size)
        {
            m_values = new Dictionary<int, bool>(size + 1);
            m_size = size;
        }

        private int m_size;
        private readonly Dictionary<int, bool> m_values;

        public int Size
        {
            get { return m_size; }
        }

        public int Next()
        {
            if (m_values.Count >= m_size)
                return -1;

            int value = UnityEngine.Random.Range(0, m_size);

            while (m_values.ContainsKey(value))
            {
                value = (value + 1) % m_size;
            }

            m_values.Add(value, false);

            return value;
        }

        public void Reset(int newSize)
        {
            if (newSize < 1)
                return;

            m_values.Clear();
            m_size = newSize;
        }
    }
}
