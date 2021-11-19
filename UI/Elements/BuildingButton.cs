using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Industry.UI.Windows;
using Industry.World;

namespace Industry.UI.Elements
{
    public class BuildingButton : UIElement
    {
        public static BuildingButton CreateButton(string buildingName, BuildingsGroupWindow parent)
        {
            var button = ObjectManager.Instance.Spawn("Building Button") as BuildingButton;

            button.Icon = ObjectManager.Instance.GetSprite(buildingName + " Icon");
            button.PrefabName = buildingName;

            button.m_window = parent;

            return button;
        }


        [SerializeField]
        private string m_buildingPrefab;

        [SerializeField]
        private Vector2 m_buttonSize;

        [SerializeField]
        private Vector2 m_buttonPos;

        [SerializeField]
        private RectTransform m_parentRect;

        [SerializeField]
        private Image m_parentImage;

        [SerializeField]
        private Sprite m_icon;

        private BuildingsGroupWindow m_window;

        public string PrefabName
        {
            get
            {
                return m_buildingPrefab;
            }
            private set
            {
                m_buildingPrefab = value;
            }
        }

        public Vector2 ButtonSize
        {
            get
            {
                return m_buttonSize;
            }
            set
            {
                m_parentRect.sizeDelta = value;

                m_buttonSize = value;
            }
        }

        public Vector2 ButtonPosition
        {
            get
            {
                return m_buttonPos;
            }
            set
            {
                m_parentRect.anchoredPosition = value;

                m_buttonPos = value;
            }
        }

        public Sprite Icon
        {
            get
            {
                return m_icon;
            }
            set
            {
                m_parentImage.sprite = value;

                m_icon = value;
            }
        }


        private void Awake()
        {
            enabled = false;
        }

        public void OnClick()
        {
            m_window.StartBuildingManager(m_buildingPrefab);
        }

    }
}
