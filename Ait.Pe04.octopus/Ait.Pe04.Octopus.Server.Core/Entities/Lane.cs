using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Entities
{
    public class Lane
    {
        public string Name { get; private set; }
        public bool IsAvailable { get; private set; }
        public Lane(string name)
        {
            Name = name;
            IsAvailable = true;
        }
        public bool IsLaneAvailable()
        {
            return IsAvailable;
        }
        public void LeaveLane()
        {
            IsAvailable = true;
        }
        public void OccupyLane()
        {
            IsAvailable = false;
        }
    }
}
