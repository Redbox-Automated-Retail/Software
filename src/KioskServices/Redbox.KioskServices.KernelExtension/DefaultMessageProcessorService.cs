using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

internal class DefaultMessageProcessorService : BaseQueueMessageProcessorService
{
	public override string Name => "KioskServices";

	public override bool IsDefault => true;

	public override List<string> SupportedMessageTypes => new List<string>();

	public override ProcessMessageDelegate ProcessMessage => OnProcessMessage;

	private static bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		response = new Dictionary<string, object>();
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null || service.IsServerBusy || ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
		{
			return false;
		}
		try
		{
			string jsonRequest = Encoding.ASCII.GetString(message.Data);
			response = KioskServicesProxy.Instance.ExecuteMessage(jsonRequest);
			return response.ContainsKey("Success") && (bool)response["Success"];
		}
		catch
		{
			return false;
		}
	}
}
