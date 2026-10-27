using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Redbox.Core;

public static class IPAddressHelper
{
	private const string MicrosoftVideoDevice = "Microsoft TV/Video Connection";

	public static IPAddress GetAddressForHostName(string hostName)
	{
		if (!NetworkInterface.GetIsNetworkAvailable())
		{
			return null;
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
		if (hostAddresses.Length == 0)
		{
			return null;
		}
		if (string.Compare(hostName, "localhost", ignoreCase: true) == 0)
		{
			return hostAddresses[0];
		}
		IPAddress result = null;
		IPAddress bindableAddress = GetBindableAddress();
		IPAddress[] array = hostAddresses;
		foreach (IPAddress iPAddress in array)
		{
			if (!(iPAddress.ToString() != bindableAddress.ToString()))
			{
				result = iPAddress;
				break;
			}
		}
		return result;
	}

	private static IPAddress GetBindableAddress()
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if ((networkInterface.Description == "Microsoft TV/Video Connection" && networkInterface.GetPhysicalAddress().GetAddressBytes() == new byte[6]) || networkInterface.OperationalStatus != OperationalStatus.Up)
			{
				continue;
			}
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			if (iPProperties == null)
			{
				continue;
			}
			foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
			{
				if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					return unicastAddress.Address;
				}
			}
		}
		return null;
	}
}
