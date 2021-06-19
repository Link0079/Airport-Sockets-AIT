using Ait.Pe04.Octopus.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Services
{
    public class LaneService
    {
        private int counter;
        public List<Lane> Lanes { get; private set; }
        public LaneService()
        {
            Lanes = new List<Lane> {
            new Lane("LANE A"),
            new Lane("LANE B"),
            new Lane("LANE C"),
            new Lane("LANE D")
            };
        }
        public Lane FindAvailableLane()
        {
            foreach (Lane lane in Lanes)
                if (lane.IsAvailable)
                    return lane;
            return null;
        }

        public string GetNumberOfAvailableLanes()
        {
            foreach (Lane lane in Lanes)
                if (lane.IsAvailable)
                    counter++;
            return $"{counter}";
        }

        public Lane FindLaneByPlane(Plane plane)
        {
            foreach (Lane lane in Lanes)
                if (lane.Plane == plane) return lane;
            return null;
        }

        public void MakeLaneAvailable(Plane plane)
        {
            var lane = FindLaneByPlane(plane);
            lane.LeaveLane();
        }

        public string AddPlaneToLane(Plane plane)
        {
            StringBuilder response = new StringBuilder();
            response.Append($"Plane {plane.Name};");
            var lane = FindAvailableLane();
            if (lane != null)
            {
                lane.OccupyLane(plane);
                response.Append($"REQLANE={lane.Name.ToUpper()} IS AVAILABLE;"); //Plane {planeName};REQLANE={laneName}ISAVAIlABLE;
            }
            else response.Append($"REQLANE=NONEAVAILABLE"); //Plane {planeName};REQLANE=NOLANEAVAILABLE
            return response.ToString();
        }

        public string GetRequestLaneFromPlane(Plane plane)
        {
            StringBuilder response = new StringBuilder();
            response.Append($"Plane {plane.Name};");
            var lane = FindLaneByPlane(plane);
            response.Append($"GOTOLANE={lane.Name}");
            return response.ToString();
        }
    }
}
