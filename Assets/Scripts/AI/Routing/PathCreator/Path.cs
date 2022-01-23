using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.Utilities;

namespace Industry.AI.Routing.PathCreator
{
    /// <summary>
    /// Represents the sequence of points of a <see cref="Route"/>.
    /// Contains methods for access to the points the path consists of.
    /// </summary>
    internal struct Path
    {
        #region Constructors

        /// <summary>
        /// Constructs a path with the provided points sequence.
        /// </summary>
        /// <param name="points">The sequence of points to construct the path with.</param>
        /// <param name="isClosed">Determines the path is a closed loop or not.</param>
        /// <param name="space">Determines the space (2D or 3D) to construct the path in.</param>
        public Path(List<Vector3> points, bool isClosed = false)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            if (points.Count < 2)
                throw new ArgumentException("points.Count < 2");

            m_points = points;
            m_isClosed = isClosed;
        }

        #endregion

        #region Fields

        private readonly List<Vector3> m_points;
        private readonly bool m_isClosed;

        #endregion

        #region Properties

        /// <summary>
        /// The number of points the path consists of.
        /// </summary>
        internal int PointsCount
        {
            get { return m_points.Count; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the route with the GizmosDraw call.
        /// </summary>
        /// <param name="color">The color to draw the path with.</param>
        internal void Display(Color color)
        {
            Gizmos.color = color;

            for (int i = 1; i < m_points.Count; i++)
                Gizmos.DrawLine(m_points[i - 1], m_points[i]);

            if (m_isClosed)
                Gizmos.DrawLine(m_points[m_points.Count - 1], m_points[0]);
        }

        /// <summary>
        /// Checks if the specified point lays within the path.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="index">The index of the leading point in the path for the <paramref name="point"/>.</param>
        internal bool Contains(Vector3 point, out int index)
        {
            return GetClosestPointOnPath(point, out index).EqualsApprox(point);
        }

        /// <summary>
        /// Gets the index of the leading point in the path for the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point that lays within the path.</param>
        /// <returns>The index of the leading point if the <paramref name="point"/> lays along the path. -1 if not.</returns>
        internal int GetIndexAtPoint(Vector3 point)
        {
            return Contains(point, out int i) ? i : -1;
        }

        /// <summary>
        /// Gets the closest to the specified point location within the path. 
        /// </summary>
        /// <param name="point">The world point to search for the closest point to.</param>
        /// <param name="index">The index of the leading point in the path for the <paramref name="point"/>.</param>
        internal Vector3 GetClosestPointOnPath(Vector3 point, out int index)
        {
            return GetClosestPointOnPath(point, 0, out index);
        }

        /// <summary>
        /// Gets the closest to the specified point location within the path, starting the search from the <paramref name="index"/>. 
        /// </summary>
        /// <param name="point">The world point to search for the closest point to.</param>
        /// <param name="startIdx">The index in the path to start searching from.</param>
        /// <param name="index">The index of the leading point in the path for the <paramref name="point"/>.</param>
        private Vector3 GetClosestPointOnPath(Vector3 point, int startIdx, out int index)
        {
            if (startIdx < 0)
                startIdx = 0;

            float minSqrDst = float.MaxValue;
            int indexOfClosestPoint = -1;
            Vector3 closestPoint = Vector3.zero;

            for (int i = startIdx; i < m_points.Count; i++)
            {
                int nextI = i + 1;

                if (nextI >= m_points.Count)
                {
                    if (m_isClosed)
                        nextI %= m_points.Count;
                    else
                        break;
                }

                Vector3 closestPointOnSegment = point.ClosestPointOnLineSegment(m_points[i], m_points[nextI]);

                float sqrDst = (point - closestPointOnSegment).sqrMagnitude;

                if (sqrDst < minSqrDst)
                {
                    minSqrDst = sqrDst;
                    indexOfClosestPoint = nextI;
                    closestPoint = closestPointOnSegment;
                }

            }

            index = indexOfClosestPoint;

            return closestPoint;
        }

        /// <summary>
        /// Gets a rotation smoothly aligned with the following path segments.
        /// </summary>
        /// <param name="point">The point to get a rotaion at.</param>
        /// <param name="knownIdx">[CAUTION IN USE] The index of the leading point in the path. Used to skip the search of it.</param>
        internal Quaternion GetRotationAt(Vector3 point, int knownIdx = -1)
        {
            if (knownIdx < -1 || knownIdx > m_points.Count)
                throw new ArgumentOutOfRangeException("knownIdx");

            if (knownIdx == -1 && !Contains(point, out knownIdx))
                throw new InvalidOperationException("The path does not contain the 'point'");

            int idx = knownIdx - 1;

            Vector3 prev = m_points[idx >= 0 ? idx : m_points.Count - 1];

            return Quaternion.LookRotation((point - prev).normalized);
        }

        /// <summary>
        /// Gets a point that is the <paramref name="from"/> point moved towards its leading point by <paramref name="distance"/>.
        /// </summary>
        /// <param name="from">The point to move.</param>
        /// <param name="distance">The value to move the <paramref name="from"/> point by.</param>
        /// <param name="newIdx">The index of the moved point's leading point. Changes if the leading point was passed by after the movement.</param>
        /// <param name="knownIdx">[CAUTION IN USE] The index of the leading point in the path. Used to skip the search of it.</param>
        internal Vector3 Move(Vector3 from, float distance, out int newIdx, int knownIdx = -1)
        {
            if (knownIdx < -1 || knownIdx > m_points.Count)
                throw new ArgumentOutOfRangeException("knownIdx");

            if (knownIdx == -1 && !Contains(from, out knownIdx))
                throw new InvalidOperationException("The path does not contain the 'from' point");

            int nextIdx = (knownIdx + 1) % m_points.Count;
            newIdx = knownIdx;

            if (distance == 0f)
                return from;

            Vector3 target = m_points[knownIdx];
            Vector3 diff = target - from;

            float eps = 0.04f;
            float sqrdistance = distance * distance;
            float diffsqrdist = diff.sqrMagnitude;

            if (sqrdistance < diffsqrdist - eps || distance < 0)
            {
                return from + (target - from).normalized * distance;
            }
            else
            {
                return Move(m_points[knownIdx], distance - diff.magnitude, out newIdx, nextIdx);
            }
        }

        #endregion
    }
}


//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using Industry.Utilities;

//namespace Industry.AI.Routing.PathCreator
//{
//    public enum EndOfPathInstruction { Loop, Reverse, Stop };

//    /// <summary>
//    /// Represents bezier and vertex path of a <see cref="Route"/>.
//    /// Containts methods for access to the points the path consists of.
//    /// </summary>
//    public class Path
//    {
//        #region Constructors

//        /// <summary>
//        /// Constructs a path with the provided points sequence.
//        /// </summary>
//        /// <param name="points">The sequence of points to construct the path with.</param>
//        /// <param name="isClosed">Determines the path is a closed loop or not.</param>
//        /// <param name="space">Determines the space (2D or 3D) to construct the path in.</param>
//        public Path(List<Vector3> points, bool isClosed = false)
//        {
//            if (points == null)
//                throw new ArgumentNullException("points");

//            if (points.Count < 2)
//                throw new ArgumentException("points.Count < 2");

//            Points = points;

//            var root = RouteController.Instance.transform;

//            m_bezierPath = new BezierPath(points, isClosed);

//            m_vertexPath = new VertexPath(m_bezierPath, root, 0.3f, 0.01f);
//        }

//        #endregion

//        #region Fields

//        private readonly BezierPath m_bezierPath;
//        private readonly VertexPath m_vertexPath;

//        #endregion

//        #region Properties

//        /// <summary>
//        /// The total length of all segments combined.
//        /// </summary>
//        internal float Length
//        {
//            get
//            {
//                return m_vertexPath.length;
//            }
//        }

//        /// <summary>
//        /// The sequence of points the path is constructed of.
//        /// </summary>
//        internal List<Vector3> Points
//        {
//            get;
//        }

//        #endregion

//        #region Methods

//        /// <summary>
//        /// Gets the closest distance value the specified point is located along the path.
//        /// </summary>
//        public float GetClosestDistanceAlongPath(Vector3 point)
//        {
//            return m_vertexPath.GetClosestDistanceAlongPath(point);
//        }

//        /// <summary>
//        /// Gets the closest to the specified point location along the path. 
//        /// </summary>
//        public Vector3 GetClosestPointOnPath(Vector3 point)
//        {
//            return m_vertexPath.GetClosestPointOnPath(point);
//        }

//        /// <summary>
//        /// Gets the closest to the specified distance value location along the path
//        /// considering the specified EndOfPath instruction.
//        /// </summary>
//        public Vector3 GetPointAtDistance(float distance, EndOfPathInstruction instruction)
//        {
//            return m_vertexPath.GetPointAtDistance(distance, instruction);
//        }

//        #endregion

//        #region Nested classes

//        /// A bezier path is a path made by stitching together any number of (cubic) bezier curves.
//        /// A single cubic bezier curve is defined by 4 points: anchor1, control1, control2, anchor2
//        /// The curve moves between the 2 anchors, and the shape of the curve is affected by the positions of the 2 control points
//        /// When two curves are stitched together, they share an anchor point (end anchor of curve 1 = start anchor of curve 2).
//        /// So while one curve alone consists of 4 points, two curves are defined by 7 unique points.
//        /// Apart from storing the points, this class also provides methods for working with the path.
//        /// For example, adding, inserting, and deleting points.
//        [System.Serializable]
//        public class BezierPath
//        {
//            public event System.Action OnModified;

//            #region Fields

//            private List<Vector3> points;

//            private bool isClosed;

//            private float autoControlLength = .3f;

//            // Normals settings
//            private List<float> perAnchorNormalsAngle;
//            private float globalNormalsAngle;
//            private bool flipNormals;

//            #endregion

//            #region Constructors

//            /// <summary> Creates a path from the supplied 3D points </summary>
//            ///<param name="points"> List or array of points to create the path from. </param>
//            ///<param name="isClosed"> Should the end point connect back to the start point? </param>
//            ///<param name="space"> Determines if the path is in 3d space, or clamped to the xy/xz plane </param>
//            public BezierPath(List<Vector3> points, bool isClosed = false)
//            {
//                if (points.Count < 2)
//                {
//                    Debug.LogError("Path requires at least 2 anchor points.");
//                }
//                else
//                {
//                    this.points = new List<Vector3> { points[0], Vector3.zero, Vector3.zero, points[1] };
//                    perAnchorNormalsAngle = new List<float>(new float[] { 0, 0 });

//                    for (int i = 2; i < points.Count; i++)
//                    {
//                        AddSegmentToEnd(points[i]);
//                        perAnchorNormalsAngle.Add(0);
//                    }
//                }

//                if (isClosed)
//                {
//                    this.isClosed = isClosed;
//                    UpdateClosedState();
//                }
//            }

//            #endregion

//            #region Public methods and accessors

//            /// Get world space position of point
//            public Vector3 this[int i]
//            {
//                get
//                {
//                    return points[i];
//                }
//            }

//            /// Number of anchor points making up the path
//            public int NumAnchorPoints
//            {
//                get
//                {
//                    return (isClosed) ? points.Count / 3 : (points.Count + 2) / 3;
//                }
//            }

//            /// Number of bezier curves making up this path
//            public int NumSegments
//            {
//                get
//                {
//                    return points.Count / 3;
//                }
//            }

//            /// If closed, path will loop back from end point to start point
//            public bool IsClosed
//            {
//                get
//                {
//                    return isClosed;
//                }
//            }

//            /// When using automatic control point placement, this value scales how far apart controls are placed
//            public float AutoControlLength
//            {
//                get
//                {
//                    return autoControlLength;
//                }
//                set
//                {
//                    value = Mathf.Max(value, .01f);
//                    if (autoControlLength != value)
//                    {
//                        autoControlLength = value;
//                        AutoSetAllControlPoints();
//                        NotifyPathModified();
//                    }
//                }
//            }

//            /// Add new anchor point to end of the path
//            public void AddSegmentToEnd(Vector3 anchorPos)
//            {
//                if (isClosed)
//                {
//                    return;
//                }

//                int lastAnchorIndex = points.Count - 1;
//                // Set position for new control to be mirror of its counterpart
//                Vector3 secondControlForOldLastAnchorOffset = points[lastAnchorIndex] - points[lastAnchorIndex - 1];
//                Vector3 secondControlForOldLastAnchor = points[lastAnchorIndex] + secondControlForOldLastAnchorOffset;
//                Vector3 controlForNewAnchor = (anchorPos + secondControlForOldLastAnchor) * .5f;

//                points.Add(secondControlForOldLastAnchor);
//                points.Add(controlForNewAnchor);
//                points.Add(anchorPos);
//                perAnchorNormalsAngle.Add(perAnchorNormalsAngle[perAnchorNormalsAngle.Count - 1]);

//                AutoSetAllAffectedControlPoints(points.Count - 1);
//                NotifyPathModified();
//            }

//            /// Returns an array of the 4 points making up the segment (anchor1, control1, control2, anchor2)
//            public Vector3[] GetPointsInSegment(int segmentIndex)
//            {
//                segmentIndex = Mathf.Clamp(segmentIndex, 0, NumSegments - 1);
//                int si3 = segmentIndex * 3;

//                return new Vector3[] { this[si3], this[si3 + 1], this[si3 + 2], this[LoopIndex(si3 + 3)] };
//            }

//            /// Flip the normal vectors 180 degrees
//            public bool FlipNormals
//            {
//                get
//                {
//                    return flipNormals;
//                }
//            }

//            /// Global angle that all normal vectors are rotated by(only relevant for paths in 3D space)
//            public float GlobalNormalsAngle
//            {
//                get
//                {
//                    return globalNormalsAngle;
//                }
//            }

//            /// Get the desired angle of the normal vector at a particular anchor(only relevant for paths in 3D space)
//            public float GetAnchorNormalAngle(int anchorIndex)
//            {
//                return perAnchorNormalsAngle[anchorIndex] % 360;
//            }

//            #endregion

//            #region Internal methods and accessors

//            /// Determines good positions (for a smooth path) for the control points affected by a moved/inserted anchor point
//            void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
//            {
//                for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
//                {
//                    if (i >= 0 && i < points.Count || isClosed)
//                    {
//                        AutoSetAnchorControlPoints(LoopIndex(i));
//                    }
//                }

//                AutoSetStartAndEndControls();
//            }

//            /// Determines good positions (for a smooth path) for all control points
//            void AutoSetAllControlPoints()
//            {
//                if (NumAnchorPoints > 2)
//                {
//                    for (int i = 0; i < points.Count; i += 3)
//                    {
//                        AutoSetAnchorControlPoints(i);
//                    }
//                }

//                AutoSetStartAndEndControls();
//            }

//            /// Calculates good positions (to result in smooth path) for the controls around specified anchor
//            void AutoSetAnchorControlPoints(int anchorIndex)
//            {
//                // Calculate a vector that is perpendicular to the vector bisecting the angle between this anchor and its two immediate neighbours
//                // The control points will be placed along that vector
//                Vector3 anchorPos = points[anchorIndex];
//                Vector3 dir = Vector3.zero;
//                float[] neighbourDistances = new float[2];

//                if (anchorIndex - 3 >= 0 || isClosed)
//                {
//                    Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
//                    dir += offset.normalized;
//                    neighbourDistances[0] = offset.magnitude;
//                }
//                if (anchorIndex + 3 >= 0 || isClosed)
//                {
//                    Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
//                    dir -= offset.normalized;
//                    neighbourDistances[1] = -offset.magnitude;
//                }

//                dir.Normalize();

//                // Set the control points along the calculated direction, with a distance proportional to the distance to the neighbouring control point
//                for (int i = 0; i < 2; i++)
//                {
//                    int controlIndex = anchorIndex + i * 2 - 1;
//                    if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
//                    {
//                        points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * autoControlLength;
//                    }
//                }
//            }

//            /// Determines good positions (for a smooth path) for the control points at the start and end of a path
//            void AutoSetStartAndEndControls()
//            {
//                if (isClosed)
//                {
//                    // Handle case with only 2 anchor points separately, as will otherwise result in straight line ()
//                    if (NumAnchorPoints == 2)
//                    {
//                        Vector3 dirAnchorAToB = (points[3] - points[0]).normalized;
//                        float dstBetweenAnchors = (points[0] - points[3]).magnitude;

//                        Vector3 perp = Vector3.Cross(dirAnchorAToB, Vector3.up) * dstBetweenAnchors / 2f;

//                        points[1] = points[0] + perp;
//                        points[5] = points[0] - perp;
//                        points[2] = points[3] + perp;
//                        points[4] = points[3] - perp;

//                    }
//                    else
//                    {
//                        AutoSetAnchorControlPoints(0);
//                        AutoSetAnchorControlPoints(points.Count - 3);
//                    }
//                }
//                else
//                {
//                    // Handle case with 2 anchor points separately, as otherwise minor adjustments cause path to constantly flip
//                    if (NumAnchorPoints == 2)
//                    {
//                        points[1] = points[0] + (points[3] - points[0]) * .25f;
//                        points[2] = points[3] + (points[0] - points[3]) * .25f;
//                    }
//                    else
//                    {
//                        points[1] = (points[0] + points[2]) * .5f;
//                        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
//                    }
//                }
//            }

//            /// Loop index around to start/end of points array if out of bounds (useful when working with closed paths)
//            int LoopIndex(int i)
//            {
//                return (i + points.Count) % points.Count;
//            }

//            /// Add/remove the extra 2 controls required for a closed path
//            void UpdateClosedState()
//            {
//                if (isClosed)
//                {
//                    // Set positions for new controls to mirror their counterparts
//                    Vector3 lastAnchorSecondControl = points[points.Count - 1] * 2 - points[points.Count - 2];
//                    Vector3 firstAnchorSecondControl = points[0] * 2 - points[1];

//                    points.Add(lastAnchorSecondControl);
//                    points.Add(firstAnchorSecondControl);
//                }
//                else
//                {
//                    points.RemoveRange(points.Count - 2, 2);

//                }

//                AutoSetStartAndEndControls();


//                if (OnModified != null)
//                {
//                    OnModified();
//                }
//            }

//            // Called when the path is modified
//            public void NotifyPathModified()
//            {
//                //boundsUpToDate = false;
//                if (OnModified != null)
//                {
//                    OnModified();
//                }
//            }

//            #endregion
//        }


//        /// A vertex path is a collection of points (vertices) that lie along a bezier path.
//        /// This allows one to do things like move at a constant speed along the path,
//        /// which is not possible with a bezier path directly due to how they're constructed mathematically.
//        /// This class also provides methods for getting the position along the path at a certain distance or time
//        /// (where time = 0 is the start of the path, and time = 1 is the end of the path).
//        /// Other info about the path (tangents, normals, rotation) can also be retrieved in this manner.
//        public class VertexPath
//        {
//            #region Fields

//            private readonly Transform transform;

//            public readonly bool isClosedLoop;

//            public readonly Vector3[] localPoints;
//            public readonly Vector3[] localTangents;
//            public readonly Vector3[] localNormals;

//            /// Percentage along the path at each vertex (0 being start of path, and 1 being the end)
//            public readonly float[] times;
//            /// Total distance between the vertices of the polyline
//            public readonly float length;
//            /// Total distance from the first vertex up to each vertex in the polyline
//            public readonly float[] cumulativeLengthAtEachVertex;

//            #endregion

//            #region Constructors

//            /// <summary> Splits bezier path into array of vertices along the path.</summary>
//            ///<param name="maxAngleError">How much can the angle of the path change before a vertex is added. This allows fewer vertices to be generated in straighter sections.</param>
//            ///<param name="minVertexDst">Vertices won't be added closer together than this distance, regardless of angle error.</param>
//            internal VertexPath(BezierPath bezierPath, Transform transform, float maxAngleError = 0.3f, float minVertexDst = 0) :
//                this(bezierPath, VertexPathUtility.SplitBezierPathByAngleError(bezierPath, maxAngleError, minVertexDst, 10), transform)
//            { }

//            /// Internal contructor
//            private VertexPath(BezierPath bezierPath, VertexPathUtility.PathSplitData pathSplitData, Transform transform)
//            {
//                this.transform = transform;
//                isClosedLoop = bezierPath.IsClosed;
//                int numVerts = pathSplitData.vertices.Count;
//                length = pathSplitData.cumulativeLength[numVerts - 1];

//                localPoints = new Vector3[numVerts];
//                localNormals = new Vector3[numVerts];
//                localTangents = new Vector3[numVerts];
//                cumulativeLengthAtEachVertex = new float[numVerts];
//                times = new float[numVerts];

//                var bounds = new Bounds(
//                    (pathSplitData.minMax.Min + pathSplitData.minMax.Max) / 2,
//                    pathSplitData.minMax.Max - pathSplitData.minMax.Min);

//                // Figure out up direction for path
//                var up = bounds.size.z > bounds.size.y ? Vector3.up : -Vector3.forward;
//                Vector3 lastRotationAxis = up;

//                // Loop through the data and assign to arrays.
//                for (int i = 0; i < localPoints.Length; i++)
//                {
//                    localPoints[i] = pathSplitData.vertices[i];
//                    localTangents[i] = pathSplitData.tangents[i];
//                    cumulativeLengthAtEachVertex[i] = pathSplitData.cumulativeLength[i];
//                    times[i] = cumulativeLengthAtEachVertex[i] / length;

//                    // Calculate normals
//                    if (i == 0)
//                    {
//                        localNormals[0] = Vector3.Cross(lastRotationAxis, pathSplitData.tangents[0]).normalized;
//                    }
//                    else
//                    {
//                        // First reflection
//                        Vector3 offset = localPoints[i] - localPoints[i - 1];
//                        float sqrDst = offset.sqrMagnitude;
//                        Vector3 r = lastRotationAxis - offset * 2 / sqrDst * Vector3.Dot(offset, lastRotationAxis);
//                        Vector3 t = localTangents[i - 1] - offset * 2 / sqrDst * Vector3.Dot(offset, localTangents[i - 1]);

//                        // Second reflection
//                        Vector3 v2 = localTangents[i] - t;
//                        float c2 = Vector3.Dot(v2, v2);

//                        Vector3 finalRot = r - v2 * 2 / c2 * Vector3.Dot(v2, r);
//                        Vector3 n = Vector3.Cross(finalRot, localTangents[i]).normalized;
//                        localNormals[i] = n;
//                        lastRotationAxis = finalRot;
//                    }
//                }

//                // Apply correction for 3d normals along a closed path
//                if (isClosedLoop)
//                {
//                    // Get angle between first and last normal (if zero, they're already lined up, otherwise we need to correct)
//                    float normalsAngleErrorAcrossJoin = Vector3.SignedAngle(localNormals[localNormals.Length - 1], localNormals[0], localTangents[0]);
//                    // Gradually rotate the normals along the path to ensure start and end normals line up correctly
//                    if (Mathf.Abs(normalsAngleErrorAcrossJoin) > 0.1f) // don't bother correcting if very nearly correct
//                    {
//                        for (int i = 1; i < localNormals.Length; i++)
//                        {
//                            float t = i / (localNormals.Length - 1f);
//                            float angle = normalsAngleErrorAcrossJoin * t;
//                            Quaternion rot = Quaternion.AngleAxis(angle, localTangents[i]);
//                            localNormals[i] = rot * localNormals[i] * (bezierPath.FlipNormals ? -1 : 1);
//                        }
//                    }
//                }

//                // Rotate normals to match up with user-defined anchor angles
//                for (int anchorIndex = 0; anchorIndex < pathSplitData.anchorVertexMap.Count - 1; anchorIndex++)
//                {
//                    int nextAnchorIndex = isClosedLoop ? (anchorIndex + 1) % bezierPath.NumSegments : anchorIndex + 1;

//                    float startAngle = bezierPath.GetAnchorNormalAngle(anchorIndex) + bezierPath.GlobalNormalsAngle;
//                    float endAngle = bezierPath.GetAnchorNormalAngle(nextAnchorIndex) + bezierPath.GlobalNormalsAngle;
//                    float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);

//                    int startVertIndex = pathSplitData.anchorVertexMap[anchorIndex];
//                    int endVertIndex = pathSplitData.anchorVertexMap[anchorIndex + 1];

//                    int num = endVertIndex - startVertIndex;
//                    if (anchorIndex == pathSplitData.anchorVertexMap.Count - 2)
//                    {
//                        num += 1;
//                    }
//                    for (int i = 0; i < num; i++)
//                    {
//                        int vertIndex = startVertIndex + i;
//                        float t = i / (num - 1f);
//                        float angle = startAngle + deltaAngle * t;
//                        Quaternion rot = Quaternion.AngleAxis(angle, localTangents[vertIndex]);
//                        localNormals[vertIndex] = (rot * localNormals[vertIndex]) * (bezierPath.FlipNormals ? -1 : 1);
//                    }
//                }
//            }

//            #endregion

//            #region Public methods and accessors

//            public int NumPoints
//            {
//                get
//                {
//                    return localPoints.Length;
//                }
//            }

//            public Vector3 GetPoint(int index)
//            {
//                return MathUtility.TransformPoint(localPoints[index], transform);
//            }

//            /// Gets point on path based on distance travelled.
//            public Vector3 GetPointAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
//            {
//                float t = dst / length;
//                return GetPointAtTime(t, endOfPathInstruction);
//            }

//            /// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
//            public Quaternion GetRotationAtDistance(float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
//            {
//                float t = dst / length;
//                return GetRotation(t, endOfPathInstruction);
//            }

//            /// Gets point on path based on 'time' (where 0 is start, and 1 is end of path).
//            public Vector3 GetPointAtTime(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
//            {
//                var data = CalculatePercentOnPathData(t, endOfPathInstruction);
//                return Vector3.Lerp(GetPoint(data.previousIndex), GetPoint(data.nextIndex), data.percentBetweenIndices);
//            }

//            /// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
//            public Quaternion GetRotation(float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
//            {
//                var data = CalculatePercentOnPathData(t, endOfPathInstruction);
//                Vector3 direction = Vector3.Lerp(localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
//                Vector3 normal = Vector3.Lerp(localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);

//                return Quaternion.LookRotation(MathUtility.TransformDirection(direction, transform), MathUtility.TransformDirection(normal, transform));
//            }

//            /// Finds the closest point on the path from any point in the world
//            public Vector3 GetClosestPointOnPath(Vector3 worldPoint)
//            {
//                TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
//                return Vector3.Lerp(GetPoint(data.previousIndex), GetPoint(data.nextIndex), data.percentBetweenIndices);
//            }

//            /// Finds the distance along the path that is closest to the given point
//            public float GetClosestDistanceAlongPath(Vector3 worldPoint)
//            {
//                TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
//                return Mathf.Lerp(cumulativeLengthAtEachVertex[data.previousIndex], cumulativeLengthAtEachVertex[data.nextIndex], data.percentBetweenIndices);
//            }

//            #endregion

//            #region Internal methods

//            /// For a given value 't' between 0 and 1, calculate the indices of the two vertices before and after t. 
//            /// Also calculate how far t is between those two vertices as a percentage between 0 and 1.
//            private TimeOnPathData CalculatePercentOnPathData(float t, EndOfPathInstruction endOfPathInstruction)
//            {
//                // Constrain t based on the end of path instruction
//                switch (endOfPathInstruction)
//                {
//                    case EndOfPathInstruction.Loop:
//                        // If t is negative, make it the equivalent value between 0 and 1
//                        if (t < 0)
//                        {
//                            t += Mathf.CeilToInt(Mathf.Abs(t));
//                        }
//                        t %= 1;
//                        break;
//                    case EndOfPathInstruction.Reverse:
//                        t = Mathf.PingPong(t, 1);
//                        break;
//                    case EndOfPathInstruction.Stop:
//                        t = Mathf.Clamp01(t);
//                        break;
//                }

//                int prevIndex = 0;
//                int nextIndex = NumPoints - 1;
//                int i = Mathf.RoundToInt(t * (NumPoints - 1)); // starting guess

//                // Starts by looking at middle vertex and determines if t lies to the left or to the right of that vertex.
//                // Continues dividing in half until closest surrounding vertices have been found.
//                while (true)
//                {
//                    // t lies to left
//                    if (t <= times[i])
//                    {
//                        nextIndex = i;
//                    }
//                    // t lies to right
//                    else
//                    {
//                        prevIndex = i;
//                    }
//                    i = (nextIndex + prevIndex) / 2;

//                    if (nextIndex - prevIndex <= 1)
//                    {
//                        break;
//                    }
//                }

//                float abPercent = Mathf.InverseLerp(times[prevIndex], times[nextIndex], t);
//                return new TimeOnPathData(prevIndex, nextIndex, abPercent);
//            }

//            /// Calculate time data for closest point on the path from given world point
//            private TimeOnPathData CalculateClosestPointOnPathData(Vector3 worldPoint)
//            {
//                float minSqrDst = float.MaxValue;
//                Vector3 closestPoint = Vector3.zero;
//                int closestSegmentIndexA = 0;
//                int closestSegmentIndexB = 0;

//                for (int i = 0; i < localPoints.Length; i++)
//                {
//                    int nextI = i + 1;
//                    if (nextI >= localPoints.Length)
//                    {
//                        if (isClosedLoop)
//                        {
//                            nextI %= localPoints.Length;
//                        }
//                        else
//                        {
//                            break;
//                        }
//                    }

//                    Vector3 closestPointOnSegment = MathUtility.ClosestPointOnLineSegment(worldPoint, GetPoint(i), GetPoint(nextI));
//                    float sqrDst = (worldPoint - closestPointOnSegment).sqrMagnitude;
//                    if (sqrDst < minSqrDst)
//                    {
//                        minSqrDst = sqrDst;
//                        closestPoint = closestPointOnSegment;
//                        closestSegmentIndexA = i;
//                        closestSegmentIndexB = nextI;
//                    }

//                }
//                float closestSegmentLength = (GetPoint(closestSegmentIndexA) - GetPoint(closestSegmentIndexB)).magnitude;
//                float t = (closestPoint - GetPoint(closestSegmentIndexA)).magnitude / closestSegmentLength;
//                return new TimeOnPathData(closestSegmentIndexA, closestSegmentIndexB, t);
//            }

//            public struct TimeOnPathData
//            {
//                public readonly int previousIndex;
//                public readonly int nextIndex;
//                public readonly float percentBetweenIndices;

//                public TimeOnPathData(int prev, int next, float percentBetweenIndices)
//                {
//                    this.previousIndex = prev;
//                    this.nextIndex = next;
//                    this.percentBetweenIndices = percentBetweenIndices;
//                }
//            }

//            #endregion
//        }

//        #endregion
//    }
//}