using System;
using System.Collections.Generic;
using System.Text;

namespace Ait.Pe04.Octopus.Core.Helpers
{
    public class Destinations
    {
        public Dictionary<string, string> Airports { get; private set; }

        public Destinations()
        {
            Airports = new Dictionary<string, string>
            {
                { "LHR", "London Heathrow Airport" },
                { "AMS", "Amsterdam Airport"},
                { "MUC", "Munich International Airport"},
                { "VIE", "Vienna Airport" },
                { "DUB", "Dublin Airport" },
                { "CDG", "Paris Charles de Gaulle Airport" },
                { "FCO", "Rome Fiumicino Airport" },
                { "BCN", "Barcelona Airport" },
                { "BRU", "Brussels Airport" },
                { "GVA", "Geneva Airport" }
            };
        }
    }
}
