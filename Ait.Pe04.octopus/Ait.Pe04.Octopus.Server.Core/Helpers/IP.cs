using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ait.Pe04.Octopus.Server.Core.Helpers
{
    public class IP
    {
        public static List<string> GetActiveIPs()
        {
            //Only IPv4 is considered for this application
            List<string> activeIPs = new List<string>();
            activeIPs.Add("127.0.0.1");
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    activeIPs.Add(ip.ToString());
                }
            }

            return activeIPs;
        }
    }
}
