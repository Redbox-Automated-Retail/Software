using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class QueueFunctions
{
	[KernelFunction(Name = "kernel.queuecounts")]
	internal static LuaTable GetQueueCounts()
	{
		ErrorList errors;
		return InnerQueueCounts(out errors);
	}

	[KernelFunction(Name = "kernel.queuecounts2")]
	internal static LuaTable GetQueueCounts2()
	{
		LuaTable countsTable = InnerQueueCounts(out var errors);
		LuaTable result = BuildResultTable(errors);
		result["counts"] = countsTable;
		return result;
	}

	[KernelFunction(Name = "kernel.queuestopworker")]
	internal static void StopQueueWorker()
	{
		ServiceLocator.Instance.GetService<IQueueService>()?.StopQueueWorker();
	}

	[KernelFunction(Name = "kernel.queuestartworker")]
	internal static void StartQueueWorker()
	{
		ServiceLocator.Instance.GetService<IQueueService>()?.StartQueueWorker();
	}

	[KernelFunction(Name = "kernel.queueclear")]
	internal static LuaTable? ClearQueue()
	{
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService != null)
		{
			return BuildResultTable(queueService.Clear());
		}
		return null;
	}

	[KernelFunction(Name = "kernel.queuegetallmessages")]
	internal static LuaTable GetAllMessages()
	{
		LuaTable table = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService == null)
		{
			return table;
		}
		int index = 1;
		BuildResultTable(queueService.ForEach(delegate(IMessage m)
		{
			table[index++] = CreateMessageTable(m);
		}))["messages"] = table;
		return table;
	}

	[KernelFunction(Name = "kernel.queuegetmessages")]
	internal static LuaTable GetMessagesByType(string type)
	{
		LuaTable table = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService == null)
		{
			return table;
		}
		int index = 1;
		BuildResultTable(queueService.ForEach(delegate(IMessage m)
		{
			table[index++] = CreateMessageTable(m);
		}, type))["messages"] = table;
		return table;
	}

	[KernelFunction(Name = "kernel.queuedequeue")]
	internal static LuaTable? Dequeue(LuaFunction predicate)
	{
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService == null)
		{
			return null;
		}
		return BuildResultTable(queueService.Dequeue(delegate(IMessage m)
		{
			if (predicate == null)
			{
				return false;
			}
			ReadOnlyCollection<object?> returnValues = predicate.Call(CreateMessageTable(m));
			return returnValues != null && returnValues.Count > 0 && (bool)returnValues[0]!;
		}));
	}

	[KernelFunction(Name = "kernel.queueisworkerstarted")]
	internal static bool IsWorkerStarted()
	{
		return ServiceLocator.Instance.GetService<IQueueService>()?.IsWorkerStarted() ?? false;
	}

	[KernelFunction(Name = "kernel.queuedeletemessage")]
	internal static LuaTable? DeleteMessage(int id)
	{
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService != null)
		{
			return BuildResultTable(queueService.DeleteMessage(id));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.queuedeletemessagesbytype")]
	internal static LuaTable? DeleteMessageByType(string type)
	{
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService != null)
		{
			return BuildResultTable(queueService.DeleteMessageByType(type));
		}
		return null;
	}

	private static LuaTable InnerQueueCounts(out ErrorList errors)
	{
		errors = new ErrorList();
		LuaTable countsTable = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService == null)
		{
			return countsTable;
		}
		Dictionary<string, int> counts = new Dictionary<string, int>();
		errors = queueService.ForEach(delegate(IMessage m)
		{
			counts.TryGetValue(m.Type, out var countSoFar);
			counts[m.Type] = countSoFar + 1;
		});
		foreach (KeyValuePair<string, int> count in counts)
		{
			countsTable[count.Key] = count.Value;
		}
		return countsTable;
	}

	private static LuaTable CreateMessageTable(IMessage message)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["id"] = message.ID,
			["data"] = message.Data,
			["type"] = message.Type,
			["priority"] = message.Priority,
			["created_on"] = message.CreatedOn.ToString("yyyyMMddHHmmss")
		};
	}

	private static LuaTable BuildResultTable(ErrorList errors)
	{
		LuaTable result = new LuaTable(KernelService.Instance.LuaRuntime);
		result["success"] = !errors.ContainsError();
		LuaTable errorsTable = (LuaTable)(result["errors"] = new LuaTable(KernelService.Instance.LuaRuntime));
		if (!errors.ContainsError())
		{
			return result;
		}
		int index = 1;
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LuaTable errorTable = new LuaTable(KernelService.Instance.LuaRuntime);
			errorTable["code"] = error.Code;
			errorTable["description"] = error.Description;
			errorTable["details"] = error.Details;
			errorsTable[index++] = errorTable;
		}
		return result;
	}

	private static LuaTable BuildCountsTable(Dictionary<string, int> counts)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["movie_rental_count"] = counts["movie_rental_count"],
			["movie_purchase_count"] = counts["movie_purchase_count"],
			["game_rental_count"] = counts["game_rental_count"],
			["game_purchase_count"] = counts["game_purchase_count"],
			["total_count"] = counts["movie_rental_count"] + counts["movie_purchase_count"] + counts["game_rental_count"] + counts["game_purchase_count"]
		};
	}
}
