using System.Collections.Generic;
using UnityEngine;

namespace Industry.AI.Routing.PathCreator
{
    public static class MathUtility
    {
        private static PosRotScale LockTransformToSpace(Transform t)
        {
            var original = new PosRotScale(t);

            float maxScale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

            t.localScale = Vector3.one * maxScale;

            return original;
        }

        public static Vector3 TransformPoint(Vector3 p, Transform t)
        {
            var original = LockTransformToSpace(t);
            Vector3 transformedPoint = t.TransformPoint(p);
            original.SetTransform(t);

            return transformedPoint;
        }

        public static Vector3 TransformDirection(Vector3 p, Transform t)
        {
            var original = LockTransformToSpace(t);
            Vector3 transformedPoint = t.TransformDirection(p);
            original.SetTransform(t);

            return transformedPoint;
        }

        public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = p - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB == 0)
                return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
            return a + aB * t;
        }

        public static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Angle((a - b), (c - b));
        }


        private class PosRotScale
        {
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Vector3 scale;

            public PosRotScale(Transform t)
            {
                this.position = t.position;
                this.rotation = t.rotation;
                this.scale = t.localScale;
            }

            public void SetTransform(Transform t)
            {
                t.position = position;
                t.rotation = rotation;
                t.localScale = scale;

            }
        }
    }
}