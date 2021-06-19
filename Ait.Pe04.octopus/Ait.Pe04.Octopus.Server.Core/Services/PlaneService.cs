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
            response.Append($"Plane {plane.Name};");
            plane.AddPassenger();
            response.Append($"ADDPASS={plane.TotalPassengers};");
            if(plane.TotalPassengers == plane.MaxPassengers) 
            {
                response.Append("FULL;");
            }
            return response.ToString(); //Plane {planeName};ADDPASS={TotalPassengers};FULL;
        }

        public string SubstractPassengerOfPlane(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"Plane {plane.Name};");
            plane.SubtractPassenger();
            response.Append($"SUBSPASS={plane.TotalPassengers};");
            if (plane.TotalPassengers == 0) //Plane {planeName};SUBSPASS={TotalPassengers};EMPTY
            {
                response.Append("EMPTY;");
            }
            return response.ToString();
        }

        public string PlaneWantsToLiftOff(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"Plane {plane.Name};");
            plane.LiftOffPlane();
            response.Append($"REQLIFT=FLYING;");
            return response.ToString();
        }

        //public string PlaneWantsToLand(long id)
        //{
        //    StringBuilder response = new StringBuilder();
        //    Plane plane = FindPlane(id);
        //    response.Append($"Plane {plane.Name}");
        //    plane.LandingPlane();
        //    response.Append($"REQLAND=LANDING;");
        //    return response.ToString();
        //}

        public string StartPlaneEngine(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"Plane {plane.Name};");
            plane.StartEngine(true);
            response.Append($"STARTENG=ENGINE STARTED");
            return response.ToString();
        }

        public string StopPlaneEngine(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"Plane {plane.Name};");
            plane.StartEngine(false);
            response.Append($"STOPENG=ENGINE STOPPED");
            return response.ToString();
        }

        public string SendSOS(long id)
        {
            StringBuilder response = new StringBuilder();
            Plane plane = FindPlane(id);
            response.Append($"Plane {plane.Name};");

            Random rng = new Random();
            if (rng.Next(0, 100) > 33)
                response.Append($"SOS=PLANE IS SAFE;");
            else
            {
                response.Append($"SOS=PLANE IS LOST");
                Planes.Remove(plane);
                //plane.IsInEmergency();
            } 
                
            return response.ToString();
        }
    }
}
