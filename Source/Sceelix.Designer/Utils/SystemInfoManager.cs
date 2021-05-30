using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using Sceelix.Extensions;

namespace Sceelix.Designer.Utils
{
    public class SystemInfoManager
    {

        internal static String Version
        {
            get
            {
                var programVersion = OSVersionInfo.ProgramBits == OSVersionInfo.SoftwareArchitecture.Bit32
                    ? "32 bit"
                    : "64 bit";
                
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();// + ", " + BuildPlatform.Enum + " " + programVersion + ", " + BuildDistribution.Enum
            }
        }



        internal static String MacAddress
        {
            get
            {
                var macAddress = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()).FirstOrDefault();

                var strings = macAddress.SplitSize(2);

                return String.Join(":", strings);
            }
        }



        internal static String OS
        {
            get
            {
                if (BuildPlatform.IsWindows)
                {
                    StringBuilder sb = new StringBuilder(String.Empty);
                    sb.Append(String.Format("Name = {0}", OSVersionInfo.Name));
                    sb.Append(String.Format(", Edition = {0}", OSVersionInfo.Edition));
                    if (OSVersionInfo.ServicePack != string.Empty)
                        sb.Append(String.Format(", Service Pack = {0}", OSVersionInfo.ServicePack));

                    sb.Append(String.Format(", ProcessorBits = {0}", OSVersionInfo.ProcessorBits));
                    sb.Append(String.Format(", OSBits = {0}", OSVersionInfo.OSBits));

                    return sb.ToString();
                }

                return Environment.OSVersion.ToString();
            }
        }



        internal static String HostName
        {
            get { return Dns.GetHostName(); }
        }
    }
}