using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Entities
{
    public class Plane
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Destination { get; private set; }
        public int TotalPassengers { get; private set; }
        public int MaxPassengers { get; private set; }
        public bool IsEngineActive { get; private set; }
        public bool InFlight { get; private set; }
        public bool IsOnLane { get; private set; }
        public bool InEmergency { get; private set; }
        //I put a Plane property in Lane, so a Plane is assigned to the Lane
        public Plane(long id, string name)
        {
            Id = id;
            Name = name;
            MaxPassengers = 10;             
            TotalPassengers = 0;
            Destination = "";
            InFlight = false;
            IsEngineActive = false;
            IsOnLane = false;
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
            IsOnLane = false;
        }
        public void LandingPlane()
        {
            InFlight = false;
            IsOnLane = true;
        }
        public void SetDestination(string destination)
        {
            Destination = destination;
        }
        public void StartEngine(bool isActive)
        {
            IsEngineActive = isActive;
        }

        public void IsInEmergency()
        {
            InEmergency = true;
        }
    }
}
