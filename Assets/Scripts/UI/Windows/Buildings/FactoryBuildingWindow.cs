using System;
using UnityEngine;
using UnityEngine.UI;
using Industry.World.Buildings;
using Industry.World.Buildings.ProductionBuildings;

namespace Industry.UI.Windows.Buildings
{
    public class FactoryBuildingWindow : BuildingWindow
    {
        [SerializeField]
        private Text m_productText;

        [SerializeField]
        private Text m_consumeText;

        [SerializeField]
        private Text m_arrowText;


        public new ProductionBuilding Building
        {
            get
            {
                return m_building as ProductionBuilding;
            }
            set
            {
                m_building = value;
            }
        }






        protected override void OnHiding()
        {
            base.OnHiding();
        }

        protected override void OnShowing()
        {
            base.OnShowing();

            UpdateContent();
        }

        protected override void OnAwakening()
        {
            base.OnAwakening();
        }
        
        protected override void OnStarting()
        {
            m_building = base.m_building as ProductionBuilding;

            m_productText.text = "Produces\n\n";
            m_consumeText.text = "Consumes\n\n";
            m_arrowText.text = "\n\n";

            foreach (var kvp in Building.storage.Products)
            {
                m_productText.text += kvp.Key + "\n";

                if (kvp.Value.Length == 0)
                {
                    m_consumeText.text = "";

                    var pos = m_productText.rectTransform.anchoredPosition;
                    pos.x = 0;
                    m_productText.rectTransform.anchoredPosition = pos;

                    continue;
                }

                m_arrowText.text += ">\n";

                var str = "";

                for (int i = 0; i < kvp.Value.Length; i++)
                    str += $"{kvp.Value[i]}, ";

                m_consumeText.text += str.Substring(0, str.Length - 2) + "\n";
            }

            base.OnStarting();
        }

        public override void UpdateContent()
        {

        }

    }
}
