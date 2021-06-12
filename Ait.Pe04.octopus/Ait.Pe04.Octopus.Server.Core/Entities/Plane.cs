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
        //public Lane lane { get; private set; }
    }
}
