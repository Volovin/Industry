using System;
using UnityEngine;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.UI.Elements;
using Industry.UI.Windows;

namespace Industry.Managers
{
    public class RoadManager : Singleton<RoadManager>, IManager
    {
        #region Fields

        private Vector3 m_sPos;
        private Vector3 m_ePos;
        private Vector3 m_sPosLast;
        private Vector3 m_ePosLast;

        private bool m_placeSwitch;
        private bool m_endValid;
        private bool m_startValid;
        private bool m_useD;
        private bool? m_X;

        private Transform m_dummiesContainer;

        private GameObject[] m_dummies;

        private ObjectManager m_objManagerInstance;
        private RoadSystem m_roadSysInstance;

        #endregion

        #region Interface implementation methods

        public void Enable()
        {
            if (!enabled)
            {
                enabled = true;
                m_placeSwitch = true;
                m_startValid = true;
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                enabled = false;

                WindowsController.Instance.WindowsLocked = false;
                ToolBar.Instance.SetButtons(true);
            }
        }


        #endregion

        #region Unity methods

        private void Awake()
        {
            m_placeSwitch = true;

            m_dummies = new GameObject[5];

            m_objManagerInstance = ObjectManager.Instance;
            m_roadSysInstance = RoadSystem.Instance;

            transform.SetParent(GameObject.Find("Controllers").transform);
            gameObject.name = "Road Manager";
        }

        private void Start()
        {
            m_dummiesContainer = new GameObject("Dummies Container").transform;
            m_dummiesContainer.SetParent(GameObject.Find("Roads Container").transform);

            for (int i = 0; i < 5; i++)
            {
                m_dummies[i] = CreateDummy(i % 2 == 0);
            }
        }

        private void Update()
        {
            if (m_placeSwitch)
            {
                Place();
            }
            else
            {
                Extend();
            }
        }

        #endregion

        #region Private methods

        #region Dummy nodes methods

        /// <summary>
        /// Creates a dummy road gameobject.
        /// </summary>
        /// <param name="isNode">Defines if a node or a segment should be created.</param>
        /// <returns></returns>
        private GameObject CreateDummy(bool isNode)
        {
            string dummyName, stripeName, mainSpriteName, stripeSpriteName;

            if (isNode)
            {
                dummyName = "Node Dummy";
                stripeName = "Dot";
                mainSpriteName = "circle gray";
                stripeSpriteName = "circle white";
            }
            else
            {
                dummyName = "Segment Dummy";
                stripeName = "Stripe";
                mainSpriteName = "rect gray";
                stripeSpriteName = "rect white";
            }

            var dummyGO = new GameObject();
            dummyGO.name = dummyName;

            var dummyBody = CreateSprite(mainSpriteName);
            dummyBody.name = "Body";
            dummyBody.transform.SetParent(dummyGO.transform);
            dummyBody.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

            var dummyStripe = CreateSprite(stripeSpriteName, 2);
            dummyStripe.name = stripeName;
            dummyStripe.transform.SetParent(dummyGO.transform);
            dummyStripe.transform.localScale =
                new Vector3(isNode ? 0.1f : 0.05f, isNode ? 0.1f : 0.25f, 1f);

            dummyGO.transform.SetParent(m_dummiesContainer);
            dummyGO.gameObject.SetActive(false);

            return dummyGO;
        }

        /// <summary>
        /// Creates a sprite gameobject with the specified name.
        /// </summary>
        /// <param name="spriteName">The name of the sprite.</param>
        /// <param name="sOrder">The sorting order of the sprite.</param>
        private GameObject CreateSprite(string spriteName, int sOrder = 1)
        {
            if (string.IsNullOrEmpty(spriteName))
                throw new System.ArgumentException("spriteName");

            var obj = new GameObject();
            obj.transform.rotation = Quaternion.Euler(90, 0, 0);

            var sprite = m_objManagerInstance.GetSprite(spriteName);

            var sR = obj.AddComponent<SpriteRenderer>();
            sR.sprite = sprite;
            sR.sortingOrder = sOrder;

            return obj;
        }

        /// <summary>
        /// Changes the color of the specified dummy objects.
        /// </summary>
        /// <param name="color">The new sprite color of the dummies.</param>
        /// <param name="a">The alpha channel (transparency) of the sprite.</param>
        /// <param name="dummies">An array of dummy objects.</param>
        private void PaintDummy(Color color, float a, params GameObject[] dummies)
        {
            if (dummies == null)
                throw new System.ArgumentNullException("dummy");

            foreach (var dummy in dummies)
            {
                var sR = dummy.transform.GetChild(0).GetComponent<SpriteRenderer>();

                color.a = a;
                sR.color = color;
            }
        }

        /// <summary>
        /// Stretches the specified segment dummy object to have the length of 
        /// the difference between <paramref name="pos1"/> and <paramref name="pos2"/> positions. 
        /// </summary>
        private void ScaleDummy(GameObject dummySegment, Vector3 pos1, Vector3 pos2)
        {
            if (dummySegment == null)
                throw new System.ArgumentNullException("dummySegment");

            float scale = Vector3.Distance(pos1, pos2);

            if (scale < 1f)
                throw new System.ArgumentException("scale < 1");


            var body = dummySegment.transform.GetChild(0);

            var S = body.localScale;
            S.y = scale;
            body.localScale = S;

            var diff = pos2 - pos1;

            dummySegment.transform.position = Vector3.Lerp(pos1, pos2, 0.5f);
            dummySegment.transform.rotation = Quaternion.LookRotation(diff);

            var stripes = dummySegment.GetComponentsInChildren<SpriteRenderer>();

            int stripesActualCount = stripes.Length - 1;
            int stripesTargetCount = Mathf.RoundToInt(scale);

            float _add = diff.x == diff.z ? 0.7071f : 0.5f;

            Vector3 cur = new Vector3(0, 0, (-scale / 2f) + _add);
            Vector3 add = new Vector3(0, 0, 1);


            if (stripesActualCount > stripesTargetCount)
            {
                for (int i = stripesTargetCount + 1; i <= stripesActualCount; i++)
                {
                    Destroy(stripes[i].gameObject);
                }
            }


            for (int i = 0; i < stripesTargetCount; i++)
            {
                if (i < stripesActualCount)
                {
                    stripes[i + 1].gameObject.transform.localPosition = cur;
                }
                else
                {
                    var newStripe = CreateSprite("rect white", 2);

                    newStripe.name = "Stripe";
                    newStripe.transform.SetParent(dummySegment.transform);

                    newStripe.transform.localScale = new Vector3(0.05f, 0.25f, 1f);
                    newStripe.transform.localRotation = stripes[0].gameObject.transform.localRotation;
                    newStripe.transform.localPosition = cur;
                }

                cur += add;
            }
        }

        #endregion

        #region Manager-specific methods

        /// <summary>
        /// Reverts the internal state to default and deactivates the manager.
        /// </summary>
        private void Cancel()
        {
            foreach (var dummy in m_dummies)
                dummy.SetActive(false);

            m_placeSwitch = true;
            m_endValid = false;
            m_X = null;

            Disable();
        }

        /// <summary>
        /// Places the first dummy node object according to the user input and checks if its position is valid.
        /// </summary>
        private void Place()
        {
            if (Input.GetMouseButtonDown(1)) { Cancel(); return; }

            if (!WorldMap.TryGetCenter(out m_sPos))
                return;

            m_ePosLast = m_sPos;

            var dummy1 = m_dummies[0];

            if (!dummy1.activeInHierarchy)
                dummy1.SetActive(true);

            if (m_sPos != m_sPosLast)
            {
                m_sPosLast = m_sPos;

                dummy1.transform.position = m_sPos;

                m_startValid = m_roadSysInstance.QuickCheck(m_sPos);

                if (m_startValid)
                    PaintDummy(Color.green, 0.75f, dummy1);
                else
                    PaintDummy(Color.red, 1f, dummy1);
            }

            if (m_startValid && Input.GetMouseButtonDown(0))
            {
                m_placeSwitch = false;
            }
        }

        /// <summary>
        /// Gathers the information nessesary to create the road according to the user input.
        /// </summary>
        private void Extend()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Cancel();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
                m_useD = !m_useD;

            if (!WorldMap.TryGetCenter(out m_ePos))
                return;

            if (m_ePos != m_sPos && m_ePos != m_ePosLast)
            {
                m_ePosLast = m_ePos;

                if (!m_X.HasValue)
                {
                    var diff = m_ePos - m_sPos;
                    m_X = diff.z == 0;
                }

                CreateRoad();
            }
            else if (m_ePos == m_sPos && m_dummies[2].activeInHierarchy)
            {
                for (int i = 0; i < m_dummies.Length; i++)
                {
                    m_dummies[i].SetActive(i == 0);
                }

                m_dummies[0].transform.position = m_sPos;

                m_ePosLast = m_sPos;

                m_X = null;
                m_endValid = false;
            }

            if (m_endValid)
            {
                PaintDummy(Color.green, 0.75f, m_dummies);

                if (Input.GetMouseButtonDown(0))
                {
                    m_roadSysInstance.CreateRoad(m_sPos, m_ePos, m_X.Value, m_useD);

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        m_sPos = m_ePos;
                        m_ePosLast = m_sPos;
                    }
                    else
                    {
                        m_placeSwitch = true;

                        for (int i = 0; i < m_dummies.Length; i++)
                        {
                            m_dummies[i].SetActive(i == 0);
                        }
                    }

                    m_endValid = false;
                }
            }
            else
            {
                PaintDummy(Color.red, 1f, m_dummies);
            }
        }

        /// <summary>
        /// Creates the dummy road by calculating its control points and checks if the resulting road is valid.
        /// </summary>
        private void CreateRoad()
        {
            var points = m_roadSysInstance.Convert(m_sPos, m_ePos, m_X.Value, m_useD);

            m_endValid = true;

            if (points.Length == 2)
            {
                m_dummies[3].SetActive(false);
                m_dummies[4].SetActive(false);
            }

            for (int i = 1; i < points.Length; i++)
            {
                int idx = i * 2;

                var node = m_dummies[idx];
                var segment = m_dummies[idx - 1];

                node.SetActive(true);
                node.transform.position = points[i];

                segment.SetActive(true);
                ScaleDummy(segment, points[i - 1], points[i]);

                if (m_endValid && !m_roadSysInstance.QuickCheck(points[i - 1], points[i]))
                {
                    m_endValid = false;
                }
            }
        }

        #endregion

        #endregion
    }
}
