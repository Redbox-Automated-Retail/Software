using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("queue")]
internal class QueueCommand
{
	[CommandForm(Name = "enqueue")]
	[Usage("queue enqueue label: entry:")]
	public void Enqueue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label, [CommandKeyValue(IsRequired = true)] string entry)
	{
		ServiceLocator.Instance.GetService<IQueueService>().Enqueue(label, entry.ToObject<Dictionary<string, object>>());
	}

	[CommandForm(Name = "dequeue")]
	[Usage("queue dequeue label:")]
	public void Dequeue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
	{
		ServiceLocator.Instance.GetService<IQueueService>().Dequeue(label);
	}

	[CommandForm(Name = "peek")]
	[Usage("queue peek label:")]
	public void Peek(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
	{
		string text = ServiceLocator.Instance.GetService<IQueueService>().PeekRaw(label);
		if (text != null)
		{
			context.Messages.Add(text.Replace(System.Environment.NewLine, ""));
		}
	}
}
