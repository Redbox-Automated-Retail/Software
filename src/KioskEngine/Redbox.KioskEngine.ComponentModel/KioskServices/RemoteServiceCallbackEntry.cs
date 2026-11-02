using System;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public sealed class RemoteServiceCallbackEntry : ICallbackEntry
{
	public RemoteServiceCallback? Callback { get; set; }

	// Set with Callback by the proxy that queues the entry.
	public IRemoteServiceResult Result { get; set; } = null!;

	public string? Message { get; set; }

	public string Name => "RemoteServiceCallbackEntry";

	public void Invoke()
	{
		if (Callback == null)
		{
			return;
		}
		try
		{
			if (!string.IsNullOrEmpty(Message))
			{
				LogHelper.Instance.Log(Message);
			}
			Callback(Result);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in SimpleCallback handler.", e);
		}
	}
}
