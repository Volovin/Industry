using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.Resources;

namespace Industry.Managers
{
    public class ResourcesManager : Singleton<ResourcesManager>, IManager
    {
        [SerializeField]
        private ResourceItem[] m_resources;

        private Dictionary<string, ResourceItem> m_resMap;

        public void Disable()
        {
            // should never be disabled
        }

        public void Enable()
        {
            //
        }


        private void Awake()
        {
            enabled = false;

            m_resMap = new Dictionary<string, ResourceItem>();

            for (int i = 0; i < m_resources.Length; i++)
            {
                var resItem = m_resources[i];

                if (resItem.Type != ResourceType.None)
                    m_resMap.Add(resItem.Name, resItem);
            }

            m_resources = null;
        }

        private void Update()
        {
            
        }




    }
}
