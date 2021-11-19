using System;
using UnityEngine;


namespace Industry.Utilities
{
    public static class VectorUtils
    {
        public static Vector3 Absified(this Vector3 vec)
        {
            vec.x = vec.x < 0f ? -vec.x : vec.x;
            vec.y = vec.y < 0f ? -vec.y : vec.y;
            vec.z = vec.z < 0f ? -vec.z : vec.z;
            
            return vec;
        }

        public static bool EqualsApprox(this Vector3 one, Vector3 two)
        {
            return (one - two).sqrMagnitude < 0.001f;
        }

        public static bool IsBetween(this Vector3 vec, Vector3 one, Vector3 two)
        {
            return
                Vector3.Dot((two - one).normalized, (vec - two).normalized) < 0f &&
                Vector3.Dot((one - two).normalized, (vec - one).normalized) < 0f;
        }

        public static bool IsBetweenOrOnOneLine(this Vector3 vec, Vector3 one, Vector3 two)
        {
            return Vector3.Cross(two - one, vec - two).y == 0f;
        }

        public static float DistancePtLine(this Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 n = b - a;
            Vector3 pa = a - p;
            Vector3 c = n * (Vector3.Dot(pa, n) / Vector3.Dot(n, n));
            Vector3 d = pa - c;

            return Mathf.Sqrt(Vector3.Dot(d, d));
        }

        public static Vector3 ClosestPointOnLineSegment(this Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = p - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB == 0)
                return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);

            return a + aB * t;
        }

        public static void RoundToDecimal(this ref Vector3 vec)
        {
            vec.x = Mathf.Round(vec.x * 10) / 10f;
            vec.y = Mathf.Round(vec.y * 10) / 10f;
            vec.z = Mathf.Round(vec.z * 10) / 10f;
        }

        public static Vector3 DirectionTo(this Vector3 fromVec, Vector3 toVec)
        {
            return (toVec - fromVec).normalized;
        }

        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static Vector3 ToVector3(this Vector2 vec)
        {
            return new Vector3(vec.x, 0f, vec.y);
        }

        public static Vector3 Random(float max, bool round = true)
        {
            var vec2 = UnityEngine.Random.insideUnitCircle * max;
            var vec3 = new Vector3(vec2.x, 0f, vec2.y);

            if (round)
                vec3.RoundToDecimal();

            return vec3;
        }
    }
}
