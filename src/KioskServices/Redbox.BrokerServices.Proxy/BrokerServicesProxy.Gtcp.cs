#if !NETFRAMEWORK
using System;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy;

// .NET 10 transport. .NET Remoting and GenuineChannels do not exist on .NET (Core); a GTCP client for the broker's
// small interface takes their place (docs/modernization.md, section 7).
public partial class BrokerServicesProxy
{
	private void InitializeTransport()
	{
	}

	private IKioskRegister OpenBroker()
	{
		throw new NotSupportedException("The legacy reservation broker (gtcp://) is not available in this .NET 10 build yet.");
	}

	private void DisconnectFromBroker()
	{
	}

	private void RegisterChannel()
	{
	}

	private void UnregisterChannel()
	{
	}
}
#endif
