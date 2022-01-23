using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.UI.Elements;
using Industry.UI.Windows;
using Industry.Utilities;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;

namespace Industry.Managers
{
    /// <summary>
    /// The manager that controlls the construction of Buildings.
    /// </summary>
    public class BuildingManager : Singleton<BuildingManager>, IManager
    {
        #region Fields

        private int m_rotation;

        private bool m_startValid;
        private bool m_forceCheck;

        private string m_currentPrefabName;

        private Vector3 m_mPos;
        private Vector3 m_mPosLast;

        private Building m_building;

        private RoadNode[] m_toReplace;

        #endregion

        #region Unity methods

        private void Awake()
        {
            transform.SetParent(GameObject.Find("Controllers").transform);
            gameObject.name = "Building Manager";
        }

        public void Update()
        {
            if (m_building == null)
            {
                m_building = Spawn(m_mPos);
            }

            MoveBuilding();
        }

        #endregion

        #region Interface implementation methods


        /// <summary>
        /// Enables the manager.
        /// </summary>
        public void Enable()
        {
            if (!enabled)
            {
                enabled = true;
            }
        }

        /// <summary>
        /// Disables the manager.
        /// </summary>
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

        #region Private methods

        /// <summary>
        /// Reverts the internal state to default and deactivates the manager.
        /// </summary>
        private void Cancel()
        {
            if (m_building != null)
                m_building.Destroy();

            m_building = null;

            m_startValid = false;

            Disable();
        }

        /// <summary>
        /// Rotates the building according to the user input.
        /// </summary>
        private void CheckRotation()
        {
            bool rotateClockWise = Input.GetKeyDown(KeyCode.E), rotateAntiClockWise = Input.GetKeyDown(KeyCode.Q);

            if (rotateClockWise ^ rotateAntiClockWise)
            {
                if (rotateClockWise)
                {
                    m_building.Rotate(true);
                    m_rotation++;
                }

                if (rotateAntiClockWise)
                {
                    m_building.Rotate(false);
                    m_rotation--;
                }

                m_forceCheck = true;
            }
        }

        /// <summary>
        /// Moves the building around according to the user input.
        /// </summary>
        private void MoveBuilding()
        {
            if (Input.GetMouseButtonDown(1)) { Cancel(); return; }

            if (!WorldMap.TryGetCenter(out m_mPos))
                return;

            CheckRotation();

            if (m_mPos != m_mPosLast || m_forceCheck)
            {
                m_mPosLast = m_mPos;
                m_forceCheck = false;
                m_toReplace = null;

                if (m_building is EntranceBuilding)
                {
                    (m_building as EntranceBuilding).Position = m_mPos;
                }
                else
                {
                    m_building.Position = m_mPos;
                }

                m_startValid = Validate();

                m_building.Color = m_startValid ? BuildingColor.Default : BuildingColor.Red;
            }

            if (m_startValid && Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(Input.GetKey(KeyCode.LeftShift));
            }

        }

        /// <summary>
        /// Applies the building and replaces all nessesary existing road nodes.
        /// </summary>
        /// <param name="placeMore">Defines if the manager should immediately spawn another building and control its placement instead of disabling itself.</param>
        private void PlaceBuilding(bool placeMore)
        {
            var eb = m_building as EntranceBuilding;

            if (eb == null)
                return;

            if (m_toReplace != null)
            {
                foreach (var road in m_toReplace)
                {
                    var links = road.Links;

                    RoadSystem.Instance.DisableAndCache(road);

                    eb.Node.AddLink(links);
                }
            }

            eb.ColliderEnabled = true;
            m_building = null;
            m_toReplace = null;

            if (!placeMore)
            {
                Disable();
            }
        }

        /// <summary>
        /// Spawns a new builing of a currently set prefab name at the spesified position. 
        /// </summary>
        /// <param name="pos">The position to spawn the building at.</param>
        private Building Spawn(Vector3 pos)
        {
            if (string.IsNullOrEmpty(m_currentPrefabName))
                throw new ArgumentException("prefabName is not set");

            var building = ObjectManager.Instance.Spawn(m_currentPrefabName, pos) as Building;

            m_rotation %= 8;

            if (m_rotation != 0)
            {
                for (int i = 0; i < Mathf.Abs(m_rotation); i++)
                    building.Rotate(m_rotation > 0);
            }

            return building;
        }

        /// <summary>
        /// Determines if current position of the building satisfies all conditions to be applied.
        /// </summary>
        private bool Validate()
        {
            var eb = m_building as EntranceBuilding;

            if (eb == null)
                return false;

            var rect = eb.Rect;

            if (!WorldMap.Bounds.ContainsFully(rect))
                return false;

            var entities = WorldMap.GetEntitiesRect(rect);

            if (entities.Length == 0)
                return true;

            m_toReplace = entities.OfType<RoadNode>().ToArray();

            if (m_toReplace.Length * 2 != entities.Length) // Nodes + Segments at each node
                return false;

            if (m_toReplace.Length == 0 || m_toReplace.Length > eb.EntrancesCount)
                return false;

            var roadsPositions = new Vector3[m_toReplace.Length];
            var entrsPositions = eb.EntrancesPositions;

            for (int i = 0; i < entrsPositions.Length; i++)
            {
                entrsPositions[i].RoundToDecimal();

                if (i >= roadsPositions.Length)
                    break;

                var currRoad = m_toReplace[i];
                var currPos = currRoad.Position;

                roadsPositions[i] = currPos;
                roadsPositions[i].RoundToDecimal();

                var linkDir = currPos.DirectionTo(currRoad.Links[0].Position);

                if (currRoad.LinksCount != 1 || !eb.EntranceAtAndDirectionEquals(currPos, linkDir))
                    return false;
            }

            int diff = entrsPositions.Except(roadsPositions).Count();

            return diff >= 0 && diff < eb.EntrancesCount;
        }

        #endregion

        #region Public methods

        public void SetBuilding(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                throw new ArgumentNullException("prefabName");

            m_currentPrefabName = prefabName;
        }

        public void AutoCreate(string prefabName, Vector3 pos, int rot)
        {
            SetBuilding(prefabName);

            m_rotation = rot;

            m_building = Spawn(pos);

            if (Validate())
            {
                PlaceBuilding(false);
            }
            else
            {
                m_building.Destroy();

                m_building = null;
                m_currentPrefabName = null;
            }
        }

        #endregion
    }
}
