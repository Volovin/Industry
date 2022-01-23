using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.Utilities;

namespace Industry.World
{
    public class City
    {
        public City(Rect3 bounds)
        {
            Bounds = bounds;
            Level = 1;
        }

        public string Name
        {
            get; set;
        }

        public int Level
        {
            get; private set;
        }

        public Rect3 Bounds
        {
            get; private set;
        }


        public void Update()
        {

        }
    }
}
