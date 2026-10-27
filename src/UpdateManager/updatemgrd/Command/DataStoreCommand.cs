using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("data")]
internal class DataStoreCommand
{
	[CommandForm(Name = "set")]
	[Usage("data set id: o:")]
	public void Set(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] string o)
	{
		ServiceLocator.Instance.GetService<IDataStoreService>().SetRaw(id, o);
	}

	[CommandForm(Name = "get")]
	[Usage("data get id:")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		string raw = ServiceLocator.Instance.GetService<IDataStoreService>().GetRaw(id);
		if (raw != null)
		{
			context.Messages.Add(raw.Replace(System.Environment.NewLine, ""));
		}
	}

	[CommandForm(Name = "delete")]
	[Usage("data delete id:")]
	public void Delete(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ServiceLocator.Instance.GetService<IDataStoreService>().Delete(id);
	}

	[CommandForm(Name = "reset")]
	[Usage("data reset")]
	public void Reset(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IDataStoreService>().Reset();
	}

	[CommandForm(Name = "cleanup")]
	[Usage("data cleanup [extension:]")]
	public void CleanUp(CommandContext context, [CommandKeyValue(IsRequired = false)] string extension)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (string.IsNullOrEmpty(extension))
		{
			service.CleanUp();
		}
		else
		{
			service.CleanUp(extension);
		}
	}
}
