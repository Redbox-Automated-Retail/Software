using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public interface ISession : IMessageSink
{
	bool LogDetailedMessages { get; }

	ParameterDictionary Context { get; }

	event EventHandler Disconnect;

	void Start();
}
