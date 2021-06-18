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
        public void AddPlane(Plane plane)
        {
            Planes.Add(plane);
        }
        public void RemovePlane(Plane plane)
        {
            Planes.Remove(plane);
        }
        public Plane FindPlane(string name)
        {
            foreach (Plane plane in Planes)
                if (plane.Name.ToUpper() == name.ToUpper())
                    return plane;
            return null;
        }

        public Plane FindPlane(long id)
        {
            foreach (Plane plane in Planes)
            {
                if (plane.Id == id) return plane;
            }
            return null;
        }

        public string AddPassengerToPlane(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"ID={id};");
            plane.AddPassenger();
            response.Append("ADDPASS;");
            if(plane.TotalPassengers == plane.MaxPassengers)
            {
                response.Append("STOPADDPASS;");
            }
            return response.ToString();
        }
    }
}
