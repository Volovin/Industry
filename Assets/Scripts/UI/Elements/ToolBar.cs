using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Industry.UI.Elements
{
    public class ToolBar : Singleton<ToolBar>
    {
        private bool m_shown;
        private bool m_buttonsEnabled;
        
        private Button[] m_buttons;

        [SerializeField]
        private GameObject m_buildingsTab;

        [SerializeField]
        private RectTransform m_rectTransform;


        public bool IsShown
        {
            get
            {
                return m_shown;
            }
            set
            {
                if (value != m_shown && value != m_buttonsEnabled)
                {
                    m_shown = value;
                    m_buttonsEnabled = value;
                    gameObject.SetActive(value);
                }
            }
        }

        public Rect BoundsRect
        {
            get; private set;
        }


        private void Awake()
        {
            var corners = new Vector3[4];

            m_rectTransform.GetWorldCorners(corners);

            float sw = Screen.width / 2f, sh = Screen.height / 2f;
            float x0 = corners[0].x - sw, y0 = corners[0].y - sh;
            float x1 = corners[2].x - sw, y1 = corners[2].y - sh;

            BoundsRect = new Rect(x0, y0, x1 - x0, y1 - y0);

            m_rectTransform = null;
        }

        private void Start()
        {
            m_buttons = GetComponentsInChildren<Button>();

            m_buildingsTab.SetActive(false);

            m_shown = true;
            m_buttonsEnabled = true;
        }
        
        public void ButtonBuild_OnClicked()
        {
            bool self = m_buildingsTab.activeSelf;

            m_buildingsTab.SetActive(!self);

            var rect = BoundsRect;
            rect.height *= self ? 0.5f : 2f;
            BoundsRect = rect;
        }



        public void SetButtons(bool enabled)
        {
            if (!m_shown || m_buttonsEnabled == enabled)
                return;

            foreach (Button b in m_buttons)
                //if (b.interactable != enabled)
                    b.interactable = enabled;

            m_buttonsEnabled = enabled;
        }

        public void SetTransparency(float value)
        {
            if (value < 0.0f || value > 1.0f)
                throw new ArgumentOutOfRangeException("Transparency value must be from 0 to 1.");

            var color = GetComponentInChildren<SpriteRenderer>().color;
            color.a = value;
            GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }
}
