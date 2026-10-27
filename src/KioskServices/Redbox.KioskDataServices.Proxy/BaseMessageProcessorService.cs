using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskDataServices.Proxy;

internal abstract class BaseMessageProcessorService : IQueueMessageProcessorService
{
	public delegate IDictionary<string, object> ProxyDelegate(byte[] payload);

	public abstract ProxyDelegate ProxyMethod { get; }

	public abstract List<string> SupportedMessageTypes { get; }

	public string Name => GetType().Name;

	public bool IsDefault => false;

	public virtual BeforeMessageProcessingDelegate BeforeMessageProcessing => null;

	public virtual ProcessMessageDelegate ProcessMessage => OnProcessMessage;

	public virtual MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully => null;

	public virtual AfterMessageProcessedDelegate AfterMessageProcessed => null;

	private bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		response = new Dictionary<string, object>();
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if (service == null || service.IsStandAlone)
		{
			LogHelper.Instance.Log("Unable to process message type " + message.Type + " because application is in stand alone mode");
		}
		else
		{
			try
			{
				response = ProxyMethod(message.Data);
				return response != null && response.ContainsKey("success") && (bool)response["success"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Error in BaseMessageProcessorService.OnProcessMessage.", e);
			}
		}
		return false;
	}
}
