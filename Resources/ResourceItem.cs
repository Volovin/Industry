using System;
using UnityEngine;

namespace Industry.Resources
{
    public enum ResourceType
    {
        None,
        Raw,
        Intermediate,
        CityConsumables,
        BuildMaterials,
        Food,
        Fuel,
        Tools,
        Machinery,
        PureWater
    }

    [Serializable]
    public struct ResourceItem
    {
        [SerializeField]
        private string m_name;

        [SerializeField]
        private ResourceType m_type;

        [SerializeField]
        private bool m_isCritical;

        public bool IsCritical
        {
            get { return m_isCritical; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public ResourceType Type
        {
            get { return m_type; }
        }

        public override string ToString()
        {
            return $"{m_name}: {m_type}";
        }
    }
}
