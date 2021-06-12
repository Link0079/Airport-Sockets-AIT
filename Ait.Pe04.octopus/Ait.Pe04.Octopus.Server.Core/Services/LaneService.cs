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
        public Lane FindPlane(string name)
        {
            foreach (Lane lane in Lanes)
                if (lane.Name.ToUpper() == name.ToUpper())
                    return lane;
            return null;
        }
    }
}
