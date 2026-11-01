using System;
using System.Messaging;
using NAnt.Core;
using NAnt.Core.Attributes;
using Redbox.NAnt.Core;

namespace Redbox.NAnt.MessageQueuing.Tasks;

[TaskName("checkMessageQueueCount")]
public class MessageQueueCountTask : TaskBase
{
	private const string QUEUE_PATH = ".\\private$\\server.message.in";

	protected override void ExecuteTask()
	{
		if (!MessageQueue.Exists(".\\private$\\server.message.in"))
		{
			return;
		}
		MessageQueue messageQueue = new MessageQueue(".\\private$\\server.message.in");
		try
		{
			int messageCount = messageQueue.GetAllMessages().Length;
			Log(Level.Info, $"There are {messageCount} items in the queue");
			if (messageCount <= 0)
			{
				return;
			}
			Log(Level.Info, "Exiting upgrade since there are messages in the queue");
			foreach (IBuildListener buildListener in Project.BuildListeners)
			{
				if (buildListener is IBuildLogger buildLogger)
				{
					buildLogger.Flush();
				}
			}
			Environment.Exit(-99);
		}
		finally
		{
			IDisposable disposable = messageQueue as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
