using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.Lua;

namespace Redbox.UpdateManager.Kernel;

public static class IpcFunctions
{
	[KernelFunction(Name = "kernel.executeservicecommand")]
	internal static LuaTable ExecuteServiceCommand(string url, string command, int timeout)
	{
		try
		{
			LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
			using (ClientSession clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(url), timeout))
			{
				clientSession.Connect();
				SystemFunctions.Wait(5000);
				ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(clientSession, disposeWhenDone: true, command);
				luaTable["success"] = clientCommandResult.Success;
				luaTable["execution_time"] = clientCommandResult.ExecutionTime.ToString();
				luaTable["command_text"] = clientCommandResult.CommandText;
				LuaTable messagesTable = (LuaTable)(luaTable["command_messages"] = new LuaTable(KernelService.Instance.LuaRuntime));
				int index = 1;
				foreach (string commandMessage in clientCommandResult.CommandMessages)
				{
					messagesTable[index++] = commandMessage;
				}
				index = 1;
				LuaTable errorsTable = new LuaTable(KernelService.Instance.LuaRuntime);
				foreach (Redbox.IPC.Framework.Error error in clientCommandResult.Errors)
				{
					LuaTable errorEntry = new LuaTable(KernelService.Instance.LuaRuntime);
					errorEntry["code"] = error.Code;
					errorEntry["description"] = error.Description;
					errorEntry["is_warning"] = error.IsWarning;
					errorEntry["details"] = error.Details;
					errorsTable[index++] = errorEntry;
				}
				luaTable["errors"] = errorsTable;
			}
			return luaTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in kernel.executeservicecommand:", e);
			throw;
		}
	}
}
