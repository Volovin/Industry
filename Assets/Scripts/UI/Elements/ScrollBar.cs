using Industry.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Industry.UI.Elements
{
    public class ScrollBar : UIElement
    {
        [SerializeField]
        protected RectTransform m_Content;
        [SerializeField]
        protected RectTransform m_Viewport;



        protected List<Separator> m_separators;


        public RectTransform ViewPort
        {
            get
            {
                return m_Viewport;
            }
        }

        public RectTransform Content
        { 
            get
            {
                return m_Content;
            }
        }

        public Vector2 ViewportSize
        {
            get
            {
                return m_Viewport.sizeDelta;
            }
        }


        public void AddSeparator(float y)
        {
            //var sepObj = Instantiate(ObjectManager.Instance.GetPrefab<UIElement>("Separator")).gameObject;
            //sepObj.transform.SetParent(m_Content, false);

            var sepObj = ObjectManager.Instance.Spawn("Separator", m_Content) as Separator;

            var rectT = sepObj.transform as RectTransform;

            rectT.anchoredPosition = new Vector2(0, y);
            m_separators.Add(sepObj);
        }


        void Awake()
        {
            m_separators = new List<Separator>();
            enabled = false;
        }
    }
}