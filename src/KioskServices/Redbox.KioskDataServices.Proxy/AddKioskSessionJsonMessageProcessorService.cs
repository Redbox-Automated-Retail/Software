using System.Collections.Generic;

namespace Redbox.KioskDataServices.Proxy;

internal class AddKioskSessionJsonMessageProcessorService : BaseMessageProcessorService
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.AddKioskSessionJson;

	public override List<string> SupportedMessageTypes => new List<string> { "AddKioskSessionJson" };
}
