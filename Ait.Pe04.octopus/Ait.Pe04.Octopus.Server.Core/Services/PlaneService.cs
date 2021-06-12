using Ait.Pe04.Octopus.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Services
{
    public class PlaneService
    {
        public List<Plane> Planes { get; private set; }
        public PlaneService()
        {
            Planes = new List<Plane>();
        }
    }
}
