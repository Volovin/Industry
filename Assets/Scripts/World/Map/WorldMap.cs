using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.Utilities;

namespace Industry.World.Map
{
    /// <summary>
    /// Карта Мира.
    /// </summary>
    public static class WorldMap
    {
        private static bool isInitialized;

        private static GameObject m_terrain;

        private static int m_terrainLayerMask;


        /// <summary>
        /// Размер одной клетки.
        /// </summary>
        public static float CellSize
        {
            get; private set;
        }

        public static Rect3 Bounds
        {
            get; private set;
        }

        public static Rect3 SoilBounds
        {
            get; private set;
        }

        /// <summary>
        /// Метод первичной инициализации Карты Мира. Может быть выполнен только 1 раз.
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;

            m_terrain = GameObject.Find("Terrain");
            m_terrainLayerMask = 1 << LayerMask.NameToLayer("Terrain");

            var rect = new Rect3(0f, 0f, 400f, 400f);
            rect.center = Vector3.zero;
            Bounds = rect;

            var soilRect = new Rect3(0f, 0f, 376f, 364f);


            CellSize = 1f;

            isInitialized = true;
        }

        /// <summary>
        /// Возвращает реальную точку-координату мира, на которую указывает мышь.
        /// </summary>
        private static Vector3 MouseToWorld()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Bounds.width * Bounds.height, m_terrainLayerMask))
                return hit.point;
            else
                throw new System.ArgumentException("Map.MouseToWorld(): Mouse pointing nowhere.");
        }

        private static Vector3 GetCenter()
        {
            Vector3 pos = MouseToWorld();

            return GetCenter(pos.x, pos.z);
        }

        private static bool ScanAreaCheck(Vector3 pos, float radius)
        {
            var cols = Physics.OverlapSphere(pos, radius);

            for (int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                return false;
            }

            return true;
        }

        private static bool ScanAreaCheck(Vector3 pos, Vector3 halfExtents)
        {
            var cols = Physics.OverlapBox(pos, halfExtents);

            for (int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                return false;
            }

            return true;
        }

        private static bool ScanLineCheck(Vector3 from, Vector3 to)
        {
            var cols = Physics.OverlapCapsule(from, to, 0.1f);

            for (int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                return false;
            }

            return true;
        }

        private static GameEntity ScanCell(Vector3 pos, float radius)
        {
            var cols = Physics.OverlapSphere(pos, radius);

            foreach (var col in cols)
            {
                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                var ge = col.gameObject.GetComponent<GameEntity>();

                if (ge != null && ge.IsAlive)
                    return ge;
            }

            return null;
        }

        private static GameEntity[] ScanArea(Vector3 pos, float radius)
        {
            var cols = Physics.OverlapSphere(pos, radius);
            var retList = new List<GameEntity>();

            foreach (var col in cols)
            {
                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                var ge = col.gameObject.GetComponent<GameEntity>();

                if (ge != null && ge.IsAlive)
                    retList.Add(ge);
            }

            return retList.ToArray();
        }

        private static GameEntity[] ScanArea(Vector3 pos, Vector3 halfExtents)
        {
            var cols = Physics.OverlapBox(pos, halfExtents);
            var retList = new List<GameEntity>();

            foreach (var col in cols)
            {
                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                var ge = col.gameObject.GetComponent<GameEntity>();

                if (ge != null && ge.IsAlive)
                    retList.Add(ge);
            }

            return retList.ToArray();
        }

        private static GameEntity[] ScanLine(Vector3 from, Vector3 to)
        {
            var cols = Physics.OverlapCapsule(from, to, 0.1f);
            var retList = new List<GameEntity>();

            foreach (var col in cols)
            {
                if (col.gameObject == m_terrain || !col.enabled)
                    continue;

                var ge = col.gameObject.GetComponent<GameEntity>();

                if (ge != null && ge.IsAlive)
                    retList.Add(ge);
            }

            return retList.ToArray();
        }


        public static T FindNearest<T>(Vector3 position, float maxRadius = 0f) where T : GameEntity
        {
            if (maxRadius < 0)
                return null;
            else if (maxRadius == 0f)
                maxRadius = Bounds.width * 1.5f;
            else
                maxRadius = Mathf.Clamp(maxRadius, 0.01f, Bounds.width * 1.5f);

            float step = 1.5f;

            while (step <= maxRadius)
            {
                var entities = GetEntities(position, step);

                var typedEntities = entities.OfType<T>();
                var childTypedEntities = entities.Select(e => e.GetComponentInChildren<T>());

                typedEntities.Concat(childTypedEntities);

                if (typedEntities.Count() > 0)
                {
                    float min = maxRadius * maxRadius * 4f;
                    T nearest = null;

                    foreach (var t in typedEntities)
                    {
                        var curr = (t.Position - position).sqrMagnitude;

                        if (curr < min)
                        {
                            min = curr;
                            nearest = t;
                        }
                    }

                    return nearest;
                }

                step += 5f;
            }

            return null;
        }


        public static bool IsAreaFree(Vector3 centerPos, float radius)
        {
            if (radius < 0.01f)
                radius = 0.01f;

            return ScanAreaCheck(centerPos, radius);
        }

        public static bool IsAreaFree(Vector3 centerPos, Vector3 halfExtents)
        {
            return ScanAreaCheck(centerPos, halfExtents);
        }

        public static bool IsCellFree(Vector3 pos)
        {
            pos = GetCenter(pos);

            return ScanAreaCheck(pos, CellSize * 0.25f);
        }

        public static bool IsLineFree(Vector3 from, Vector3 to)
        {
            from = GetCenter(from);
            to = GetCenter(to);

            if (from.EqualsApprox(to))
                return ScanAreaCheck(from, CellSize * 0.25f);
            else
                return ScanLineCheck(from, to);
        }

        public static GameEntity GetEntity(Vector3 pos)
        {
            pos = GetCenter(pos);

            return ScanCell(pos, CellSize * 0.1f);
        }

        public static GameEntity[] GetEntities(Vector3 pos, float radius)
        {
            if (radius < 0.01f)
                radius = 0.01f;

            return ScanArea(pos, radius);
        }

        public static GameEntity[] GetEntities(Vector3 from, Vector3 to)
        {
            from = GetCenter(from);
            to = GetCenter(to);

            if (from.EqualsApprox(to))
                return ScanArea(from, CellSize * 0.45f);
            else
                return ScanLine(from, to);
        }

        public static GameEntity[] GetEntitiesRect(Rect3 rect)
        {
            var center = rect.center;
            var halfext = new Vector3(rect.width * 0.5f, 1f, rect.height * 0.5f);

            return ScanArea(center, halfext);
        }

        public static GameEntity[] GetEntitiesRect(Vector3 center, Vector3 halfExtents)
        {
            return ScanArea(center, halfExtents);
        }


        public static Vector3 GetCenter(Vector3 pos)
        {
            return GetCenter(pos.x, pos.z);
        }

        public static Vector3 GetCenter(float x, float z)
        {
            float msX = Bounds.width * 0.5f, msZ = Bounds.height * 0.5f;

            if (x < -msX || x > msX || z < -msZ || z > msZ)
                throw new System.ArgumentException($"x = {x}; z = {z}");

            float tS = CellSize;
            float tS2 = tS / 2f;

            float sX = Mathf.Sign(x);
            float sZ = Mathf.Sign(z);

            float X = ((int)(x / tS)) + sX * tS2;
            float Z = ((int)(z / tS)) + sZ * tS2;

            return new Vector3(X, 0f, Z);
        }

        public static bool TryGetCenter(out Vector3 result)
        {
            try
            {
                result = GetCenter();

                return true;
            }
            catch (System.ArgumentException)
            {
                result = new Vector3(float.NaN, float.NaN, float.NaN);

                return false;
            }
        }


        public static List<Vector3> PointsBetween(Vector3 from, Vector3 target)
        {
            if (from == target)
                return null;

            Vector3 diff = target - from;

            Vector3 step = GetStep(diff);

            float tS = CellSize;
            int size = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(diff.x / tS), Mathf.Abs(diff.z / tS))) + 1;

            var points = new List<Vector3>();
            Vector3 current = from;

            for (int i = 0; i < size; i++)
            {
                points.Add(current);
                current += step;
            }

            return points;
        }

        private static Vector3 GetStep(Vector3 vector, bool skipRound = true)
        {
            float tS = CellSize;

            Vector3 norm = vector.normalized;

            norm.x = Mathf.Clamp(norm.x * 2 * tS, -tS, tS);
            norm.z = Mathf.Clamp(norm.z * 2 * tS, -tS, tS);

            if (!skipRound)
            {
                norm.x = Mathf.RoundToInt(norm.x);
                norm.z = Mathf.RoundToInt(norm.z);
            }

            return norm;
        }
    }
}
