using System;
using UnityEngine;
using Industry.AI.Routing;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.Utilities;
using static Industry.Utilities.Logger;


namespace Industry.World.Vehicles
{
    public abstract class Vehicle : GameEntity
    {
        private float m_cooldown;

        private int m_currentIdx;

        private bool m_shouldUpdateRoute;
        private bool m_routeFWD;

        protected bool m_canMove;
        protected bool m_aborted;

        private Route m_mainRoute;
        private Route m_tempRoute;

        private Vector3 m_destination;


        public ushort ID
        {
            get; private set;
        }
        public float MoveSpeed
        {
            get; protected set;
        }
        public float RotateSpeed
        {
            get; protected set;
        }
        public float StopTime
        {
            get; set;
        }


        private void FindWay()
        {
            var toGo = m_destination;
            var vPos = Position;

            if (WorldMap.IsCellFree(vPos))
                vPos = WorldMap.FindNearest<RoadNode>(vPos).Position;

            bool valid = false;

            if (m_tempRoute == null)
            {
                int p1 = m_mainRoute.Platform1;
                int p2 = m_mainRoute.Platform2;

                m_tempRoute = Route.Create(vPos, toGo, p1, p2, true);

                if (m_tempRoute != null)
                {
                    m_tempRoute.AddVehicle(this);
                    valid = true;
                }
            }
            else
            {
                valid = m_tempRoute.Set(vPos, toGo, true);
            }

            if (valid)
            {
                m_tempRoute.Color = Color.red;

                m_destination = m_tempRoute.endNodePosition;

                if (!m_tempRoute.Contains(vPos, out m_currentIdx))
                {
                    transform.position = m_tempRoute.GetClosestPointOnPath(Position, out m_currentIdx);
                }
            }
            else
            {
                Log("Unable to build temp Route", "red");

                DeleteTempRoute();

                transform.position = m_mainRoute.GetClosestPointOnPath(Position, out m_currentIdx);
            }
        }

        private void Move()
        {
            Route route = m_tempRoute ?? m_mainRoute;

            float value = MoveSpeed * Time.deltaTime;

            var posCurr = route.MovePoint(Position, value, out m_currentIdx, m_currentIdx);

            var dir = (posCurr - Position).normalized;
            var rot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(transform.rotation, rot, RotateSpeed * Time.deltaTime);
            transform.position += transform.forward * value;

            var p = m_routeFWD ? route.endPoint : route.startPoint;
            bool reachedDest = (posCurr - p).sqrMagnitude < 0.015f;

            if (m_tempRoute != null && (m_mainRoute.Contains(posCurr, out int newIdx) || reachedDest))
            {
                if ((m_routeFWD && newIdx < m_mainRoute.PointsCount / 2) || (!m_routeFWD && newIdx >= m_mainRoute.PointsCount / 2))
                {
                    route = m_mainRoute;

                    m_currentIdx = newIdx;

                    DeleteTempRoute();
                }
            }

            if (m_tempRoute == null && reachedDest)
            {
                if (m_destination == route.startNodePosition)
                {
                    m_routeFWD = true;
                    m_destination = route.endNodePosition;
                }
                else
                {
                    m_routeFWD = false;
                    m_destination = route.startNodePosition;
                }

                InitStop();
            }
        }


        private void InitStop()
        {
            Stop(StopTime);

            Vector3 rot = transform.rotation.eulerAngles;

            float min = ((int)rot.y / 45) * 45;
            rot.y = rot.y < min + 22.5f ? min : min + 90;

            transform.rotation = Quaternion.Euler(rot);

        }

        private void DeleteTempRoute()
        {
            if (m_tempRoute == null)
                return;

            RouteController.Instance.RemoveRoute(m_tempRoute);
            m_tempRoute = null;
        }

        public void Abort(string reason)
        {
            m_aborted = true;

            DeleteTempRoute();

            Debug.LogWarning("<color=red> Aborted:</color> Vehicle \"" + name + "\": " + reason + "!");
        }

        public void Restart(string message = null)
        {
            if (message == null)
                message = $"Vechile restarted";

            m_aborted = false;

            Log(message);
        }

        public void SetRoute(Route route)
        {
            if (route == null)
                throw new ArgumentNullException("route");

            if (m_mainRoute == route)
                return;

            m_mainRoute = route;

            DeleteTempRoute();
            UpdateRoute();

            m_canMove = true;
            m_aborted = false;
        }

        public void Stop(float seconds)
        {
            m_canMove = false;
            m_cooldown = seconds;
        }

        public void UpdateRoute()
        {
            m_shouldUpdateRoute = true;
        }


        protected override void _Awake()
        {
            MoveSpeed = 1f;
            RotateSpeed = 7.5f;
            StopTime = 2f;

            m_canMove = true;
        }

        protected override void _Start()
        {
            if (m_mainRoute == null)
            {
                Abort("No route is set");
                return;
            }

            m_routeFWD = true;

            m_destination = m_mainRoute.endNodePosition;

            UpdateRoute();

            //m_distance = m_mainRoute.GetClosestDistanceAlongPath(Position);
        }

        protected override void _Update()
        {
            if (m_aborted || m_mainRoute.IsDaemon)
            {
                return;
            }

            if (m_mainRoute == null)
            {
                Abort("Main route is null");
                return;
            }

            if (m_shouldUpdateRoute)
            {
                if (m_tempRoute != null)
                {
                    FindWay();
                }
                else
                {
                    m_destination = m_routeFWD ? m_mainRoute.endNodePosition : m_mainRoute.startNodePosition;

                    if (!m_mainRoute.Contains(Position, out m_currentIdx))
                    {
                        FindWay();
                    }
                }

                m_shouldUpdateRoute = false;
            }


            if (m_canMove)
            {
                Move();
            }
            else
            {
                m_cooldown -= Time.deltaTime;

                if (m_cooldown <= 0f)
                {
                    m_cooldown = 0f;
                    m_canMove = true;
                }

                // Add functionality
                // i.e. stop at factories
            }
        }


        public static ushort LastID
        {
            get;
            private set;
        }

        public static Vehicle Spawn(string vehicleName, Route mainRoute)
        {
            if (string.IsNullOrEmpty(vehicleName))
                throw new ArgumentNullException("vehicleName");

            if (mainRoute == null)
                throw new ArgumentNullException("mainRoute");

            var truck = ObjectManager.Instance.Spawn(vehicleName, mainRoute.startPoint) as Vehicle;

            LastID++;
            truck.ID = LastID;

            truck.m_mainRoute = mainRoute;

            return truck;
        }
    }
}
