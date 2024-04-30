using System.Net;
using System.Net.NetworkInformation;

namespace bms.Leaf.Common
{
    public class Utils
    {
        public static string GetIp()
        {
            string ip;
            try
            {
                List<string> ipList = GetHostAddress(null);
                ip = (ipList.Count > 0) ? ipList[0] : "";
            }
            catch
            {
                throw;
            }
            return ip;
        }
        public static string GetIp(string interfaceName)
        {
            string ip;
            interfaceName = interfaceName.Trim();
            try
            {
                List<string> ipList = GetHostAddress(interfaceName);
                ip = (ipList.Count > 0) ? ipList[0] : "";
            }
            catch
            {
                throw;
            }
            return ip;
        }

        private static List<string> GetHostAddress(string interfaceName)
        {
            List<string> ipList = new List<string>(5);
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                foreach (UnicastIPAddressInformation addressInfo in ni.GetIPProperties().UnicastAddresses)
                {
                    IPAddress address = addressInfo.Address;
                    if (IPAddress.IsLoopback(address))
                        continue;
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        continue;

                    string hostAddress = address.ToString();
                    if (interfaceName == null)
                    {
                        ipList.Add(hostAddress);
                    }
                    else if (interfaceName.Equals(ni.Name))
                    {
                        ipList.Add(hostAddress);
                    }
                }
            }
            return ipList;
        }

    }
}
