using System;
using System.Collections.Generic;
using UnityEngine;

namespace Industry.World.Vehicles
{
    public class Truck : Vehicle
    {
        public int capacity;



        /*
        public override void Stop()
        {
            //Speed = 0f;
        }
        */

        protected override void _Awake()
        {
            base._Awake();
        }

        protected override void _Start()
        {
            base._Start();
            //vehicleCount++;
            gameObject.name = "Truck "; // + vehicleCount;
            
        }

        protected override void _Update()
        {
            base._Update();
        }
    }
}
