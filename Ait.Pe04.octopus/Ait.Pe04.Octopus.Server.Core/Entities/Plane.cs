using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Entities
{
    public class Plane
    {
        public string Name { get; private set; }
        public string Destination { get; private set; }
        public int TotalPassengers { get; private set; }
        public int MaxPassengers { get; private set; }
        public bool IsEngineActive { get; private set; }
        public bool InFlight { get; private set; }
        public bool IsOnLane { get; private set; }
        //public bool InEmergency { get; private set; }
        //public Lane Lane { get; private set; }
        public Plane(string name)
        {
            Name = name;
            MaxPassengers = 10;             // Consider setting through parameters..
            TotalPassengers = 0;
            Destination = "";
            InFlight = false;
            IsEngineActive = false;
            IsOnLane = false;
            //InEmergency = false;
            //Lane = null;                  // Not sure of the Lane part
        }
        public void AddPassenger()
        {
            TotalPassengers++;
            if (TotalPassengers >= MaxPassengers)
                TotalPassengers = MaxPassengers;
        }
        public void SubtractPassenger()
        {
            TotalPassengers--;
            if (TotalPassengers <= 0)
                TotalPassengers = 0;
        }
        public void LiftOffPlane()
        {
            InFlight = true;
        }
        public void LandingPlane()
        {
            InFlight = false;
        }
        public void SetDestination(string destination)
        {
            Destination = destination;
        }
        public void StartEngine(bool isActive)
        {
            IsEngineActive = isActive;
        }
        //public void GoToLane(Lane lane)           // Methode for the Lane part
        //{
        //    IsOnLane = true;
        //    lane.OccupyLane();
        //    Lane = lane;
        //}
        //public void LeaveLane(Lane lane)
        //{
        //    IsOnLane = false;
        //    lane.LeaveLane();
        //    Lane = null;
        //}
        //public void CallSOS()
        //{
        //    InEmergency = true;
        //}
        //public void CallOfSOS()
        //{
        //    InEmergency = false;
        //}
    }
}
