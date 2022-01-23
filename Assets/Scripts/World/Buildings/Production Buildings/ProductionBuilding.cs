using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.UI.Windows.Buildings;

namespace Industry.World.Buildings.ProductionBuildings
{
    public abstract class ProductionBuilding : EntranceBuilding
    {
        [SerializeField]
        private Storage m_storage;

        public Storage storage
        {
            get { return m_storage; }
        }


        protected override void _Awake()
        {
            base._Awake();

            // Init the struct
            var p = m_storage.Products;

            var t = GameObject.Find("Windows").transform;

            m_window = ObjectManager.Instance.Spawn("FactoryBuildingWindow", t) as FactoryBuildingWindow;
            m_window.Building = this;
        }


        [Serializable]
        public struct Storage
        {
            [SerializeField]
            private ProductAmount[] m_products;

            private Dictionary<ResourceAmount, ResourceAmount[]> m_production;
            private Dictionary<string, string[]> m_productionStrings;
            private Dictionary<string, int> m_stockpileIn;
            private Dictionary<string, int> m_stockpileOut;


            private void Init()
            {
                m_production = new Dictionary<ResourceAmount, ResourceAmount[]>();
                m_productionStrings = new Dictionary<string, string[]>();
                m_stockpileIn = new Dictionary<string, int>();
                m_stockpileOut = new Dictionary<string, int>();

                var pairs = new List<KeyValuePair<string, string[]>>();

                foreach (var product in m_products)
                {
                    var ra = new ResourceAmount(product.name, product.amount);
                    m_production.Add(ra, product.resources);

                    m_productionStrings.Add(ra.Name, product.resources.Select(x => x.Name).ToArray());

                    foreach (var res in product.resources)
                    {
                        m_stockpileIn[res.Name] = 0;
                    }

                    m_stockpileOut[ra.Name] = 0;
                }

                m_products = null;
            }


            private int GetResourceAmount(string resName)
            {
                if (string.IsNullOrEmpty(resName))
                    throw new ArgumentException("resname");

                if (m_stockpileIn.ContainsKey(resName))
                    return m_stockpileIn[resName];
                
                if (m_stockpileOut.ContainsKey(resName))
                    return m_stockpileOut[resName];

                throw new InvalidOperationException($"The resource \"{resName}\" doesn't exist in this building");
            }

            private void SetResourceAmount(string resName, int value)
            {
                if (string.IsNullOrEmpty(resName))
                    throw new ArgumentException("resname");

                if (m_stockpileIn.ContainsKey(resName))
                {
                    m_stockpileIn[resName] = value;
                    return;
                }
                if (m_stockpileOut.ContainsKey(resName))
                {
                    m_stockpileOut[resName] = value;
                    return;
                }

                throw new InvalidOperationException($"The resource \"{resName}\" doesn't exist in this building");
            }


            public KeyValuePair<string, string[]>[] Products
            {
                get
                {
                    if (m_production == null)
                        Init();

                    return m_productionStrings.ToArray();
                }
            }

            public string[] ResourcesOf(string productName)
            {
                if (!m_productionStrings.TryGetValue(productName, out var res))
                    throw new InvalidOperationException("There is no product " + productName);

                return res;
            }


            public int this[string resName]
            {
                get
                {
                    return GetResourceAmount(resName);
                }
                set
                {
                    SetResourceAmount(resName, value);
                }
            }
        }

        [Serializable]
        private struct ResourceAmount
        {
            public ResourceAmount(string name, int amount)
            {
                m_name = name;
                m_amount = amount;
            }

            [SerializeField]
            private string m_name;
            [SerializeField]
            private int m_amount;


            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }

            public int Amount
            {
                get { return m_amount; }
                set { m_amount = value; }
            }


            public override string ToString()
            {
                return $"{m_name}: {m_amount}";
            }
        }

        [Serializable]
        private struct ProductAmount
        {
            public string name;
            public int amount;

            public ResourceAmount[] resources;


            public override string ToString()
            {
                return $"{name}: {amount} from {resources.Length} res types";
            }
        }
    }
}
