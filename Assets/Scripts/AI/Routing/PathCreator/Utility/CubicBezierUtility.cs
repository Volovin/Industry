using System.Collections.Generic;
using UnityEngine;

namespace Industry.AI.Routing.PathCreator
{

    /// Collection of functions related to cubic bezier curves
    /// (a curve with a start and end 'anchor' point, and two 'control' points to define the shape of the curve between the anchors)
    public static class CubicBezierUtility
    {
        /// Returns point at time 't' (between 0 and 1) along bezier curve defined by 4 points (anchor_1, control_1, control_2, anchor_2)
        public static Vector3 EvaluateCurve(Vector3[] points, float t)
        {
            return EvaluateCurve(points[0], points[1], points[2], points[3], t);
        }

        /// Returns point at time 't' (between 0 and 1)  along bezier curve defined by 4 points (anchor_1, control_1, control_2, anchor_2)
        public static Vector3 EvaluateCurve(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
        {
            t = Mathf.Clamp01(t);

            float t_1 = 1 - t;
            float t_s = t * t;
            float t_1_s = t_1 * t_1;

            return t_1_s * t_1 * a1 + 3 * t_1_s * t * c1 + 3 * t_1 * t_s * c2 + t * t_s * a2;
        }

        /// Returns a vector tangent to the point at time 't'
        /// This is the vector tangent to the curve at that point
        public static Vector3 EvaluateCurveDerivative(Vector3[] points, float t)
        {
            return EvaluateCurveDerivative(points[0], points[1], points[2], points[3], t);
        }

        /// Calculates the derivative of the curve at time 't'
        /// This is the vector tangent to the curve at that point
        public static Vector3 EvaluateCurveDerivative(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
        {
            t = Mathf.Clamp01(t);

            float t_1 = 1 - t;

            return 3 * t_1 * t_1 * (c1 - a1) + 6 * t_1 * t * (c2 - c1) + 3 * t * t * (a2 - c2);
        }

        // Crude, but fast estimation of curve length.
        public static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float controlNetLength = (p0 - p1).magnitude + (p1 - p2).magnitude + (p2 - p3).magnitude;
            float estimatedCurveLength = (p0 - p3).magnitude + controlNetLength / 2f;
            return estimatedCurveLength;
        }
    }
}
