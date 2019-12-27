using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Extention
{
    public class PCExtention
    {
        public static string GetMacId()
        {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();



            string mac = "";
            int dem = 1;
            for (int i = 0; i < macAddr.Length; i++)
            {
                if (dem == 2 && i != macAddr.Length - 1)
                {
                    mac += macAddr[i] + ":";
                    dem = 0;
                }
                else
                {
                    mac += macAddr[i];
                }
                dem++;
            }
            mac = mac.ToLower();
            return mac;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var _ip in host.AddressList)
            {
                if (_ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return _ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
