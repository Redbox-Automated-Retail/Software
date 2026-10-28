using System.Collections.Generic;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.KernelExtension;

internal abstract class BaseQueueMessageProcessorService : IQueueMessageProcessorService
{
	public abstract string Name { get; }

	public abstract bool IsDefault { get; }

	public abstract List<string> SupportedMessageTypes { get; }

	public abstract ProcessMessageDelegate ProcessMessage { get; }

	public virtual BeforeMessageProcessingDelegate BeforeMessageProcessing => OnBeforeMessageProcessing;

	public virtual MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully => OnMessageProcessedSuccessfully;

	public AfterMessageProcessedDelegate AfterMessageProcessed => null;

	private static void OnBeforeMessageProcessing(IMessage message, out object clientData)
	{
		IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
		object obj = null;
		service?.HandleBeforeReturnMessageProcessing(message, out obj);
		clientData = obj;
	}

	private static void OnMessageProcessedSuccessfully(IMessage message, IDictionary<string, object> result, object clientData)
	{
		if (message.Type == "Return")
		{
			ServiceLocator.Instance.GetService<IFraudService>()?.ProcessReturnResponse(result, clientData);
		}
		ServiceLocator.Instance.GetService<IReservationServices>()?.ProcessValidationResponse(message, result, clientData);
	}
}
