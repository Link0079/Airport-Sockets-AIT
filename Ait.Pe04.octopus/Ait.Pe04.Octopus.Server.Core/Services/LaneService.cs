using Ait.Pe04.Octopus.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Services
{
    public class LaneService
    {
        public List<Lane> Lanes { get; private set; }
        public LaneService()
        {
            Lanes = new List<Lane> {
            new Lane("Lane A"),
            new Lane("Lane B"),
            new Lane("Lane C")
            };
        }
        public Lane FindAvailableLane()
        {
            foreach (Lane lane in Lanes)
                if (lane.IsAvailable)
                    return lane;
            return null;
        }

        public Lane AddPlaneToLane(Plane plane)
        {
            var lane = FindAvailableLane();
            lane.OccupyLane(plane);
            return lane;
        }
    }
}
