using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.AI.Routing;
using Industry.Managers;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.UI.Windows;

namespace Industry.Utilities
{
    public class TestingStuff : MonoBehaviour
    {
        private enum Actions
        {
            PlaceRoads,
            PlaceBuildings,
            BenchmarkPath
        }

        [SerializeField]
        private bool m_largeGrid;

        [SerializeField]
        private List<Actions> m_actionList;

        private void Start()
        {
            enabled = false;

            for (int i = 0; i < m_actionList.Count; i++)
            {
                switch (m_actionList[i])
                {
                    case Actions.PlaceRoads:
                        PlaceRoads();
                        break;

                    case Actions.PlaceBuildings:
                        if (!m_largeGrid && m_actionList.Contains(Actions.PlaceRoads))
                            PlaceBuildings();
                        break;
                    case Actions.BenchmarkPath:
                        if (m_largeGrid && m_actionList.Contains(Actions.PlaceRoads))
                            BenchmarkPath();
                        break;
                    default:
                        break;
                }
            }
        }

        private void Update()
        {

        }

        private void PlaceRoads()
        {
            var a = RoadSystem.Instance;
            var timer = new Timer().Start();

            if (m_largeGrid) // 26 ms - 18 ms
            {
                for (int i = 0; i < 40; i++)
                {
                    Vector3 s = i == 0 || i == 39 ? new Vector3(-20.5f, 0f, -19.5f + i) : new Vector3(-19.5f, 0f, -19.5f + i);
                    Vector3 e = i == 0 || i == 39 ? new Vector3(20.5f, 0f, -19.5f + i) : new Vector3(19.5f, 0f, -19.5f + i);

                    a.CreateRoad(s, e, true, false);
                }

                for (int i = 0; i < 40; i++)
                {
                    Vector3 s = i == 0 || i == 39 ? new Vector3(-19.5f + i, 0f, -20.5f) : new Vector3(-19.5f + i, 0f, -19.5f);
                    Vector3 e = i == 0 || i == 39 ? new Vector3(-19.5f + i, 0f, 20.5f) : new Vector3(-19.5f + i, 0f, 19.5f);

                    a.CreateRoad(s, e, false, false);
                }
            }
            else // 5500 ms - 1415 ms
            {
                a.CreateRoad(new Vector3(-4.5f, 0f, 4.5f), new Vector3(3.5f, 0f, 4.5f), true, false);
                a.CreateRoad(new Vector3(-4.5f, 0f, 0.5f), new Vector3(3.5f, 0f, 0.5f), true, false);
                a.CreateRoad(new Vector3(-4.5f, 0f, -4.5f), new Vector3(3.5f, 0f, -4.5f), true, false);
                a.CreateRoad(new Vector3(-3.5f, 0f, 5.5f), new Vector3(-3.5f, 0f, -5.5f), false, false);
                a.CreateRoad(new Vector3(-2.5f, 0f, 4.5f), new Vector3(-2.5f, 0f, -4.5f), false, false);
                a.CreateRoad(new Vector3(-1.5f, 0f, 4.5f), new Vector3(-1.5f, 0f, -4.5f), false, false);
                a.CreateRoad(new Vector3(-0.5f, 0f, 5.5f), new Vector3(-0.5f, 0f, -5.5f), false, false);
                a.CreateRoad(new Vector3(0.5f, 0f, 4.5f), new Vector3(0.5f, 0f, -4.5f), false, false);
                a.CreateRoad(new Vector3(1.5f, 0f, 4.5f), new Vector3(1.5f, 0f, -4.5f), false, false);
                a.CreateRoad(new Vector3(2.5f, 0f, 5.5f), new Vector3(2.5f, 0f, -5.5f), false, false);
            }

            Debug.Log($"{timer.ElapsedTime()} ms");
        }

        private void PlaceBuildings()
        {
            var a = BuildingManager.Instance;

            a.AutoCreate("Saw Mill", new Vector3(4f, 0f, -3.5f), 1);
            a.AutoCreate("Saw Mill", new Vector3(-5f, 0f, -0.5f), 3);
            a.AutoCreate("Warehouse 4x4", new Vector3(-6f, 0f, -5f), 3);
            a.AutoCreate("Warehouse 4x4", new Vector3(5f, 0f, 1f), 1);
            a.AutoCreate("Depot", new Vector3(0f, 0.05f, 6f), 0);


            var from1 = (WorldMap.GetEntity(new Vector3(4f, 0f, -3.5f)) as EntranceBuilding).Node;
            var goal1 = (WorldMap.GetEntity(new Vector3(-6f, 0f, -5f)) as EntranceBuilding).Node;

            var from2 = (WorldMap.GetEntity(new Vector3(-5f, 0f, -0.5f)) as EntranceBuilding).Node;
            var goal2 = (WorldMap.GetEntity(new Vector3(5f, 0f, 1f)) as EntranceBuilding).Node;


            var rm = WindowsController.Instance.Windows.RouteManager;

            rm.AddRouteLine();
            var r1 = Route.Create(from1, goal1, 0, 0);
            rm.SetRouteInfoToActiveLine(r1.ID, new int[2] { 1, 1 });

            rm.AddRouteLine();
            var r2 = Route.Create(from2, goal2, 0, 0);
            rm.SetRouteInfoToActiveLine(r2.ID, new int[2] { 1, 1 });
        }

        private void BenchmarkPath()
        {
            int times = 11;
            string result = " \n";
            double total = 0.0;

            var from = WorldMap.GetEntity(new Vector3(-19.5f, 0f, -20.5f)) as RoadNode;
            var goal = WorldMap.GetEntity(new Vector3(19.5f, 0f, 20.5f)) as RoadNode;

            //warmup
            Route.Create(from, goal, 0, 0);

            var timer = new Timer();

            for (int i = 0; i < times; i++)
            {
                timer.Start();

                Route.Create(from, goal, 0, 0);

                double d = timer.ElapsedTime();

                if (i == 4) continue;

                total += d;
                result += $"{i + 1}: {d} ms\n";
            }

            result += $"Avg: {total / (times - 1)}";

            Logger.Log(result);
        }

    }
}
