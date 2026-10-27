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
				LuaTable luaTable2 = (LuaTable)(luaTable["command_messages"] = new LuaTable(KernelService.Instance.LuaRuntime));
				int num = 1;
				foreach (string commandMessage in clientCommandResult.CommandMessages)
				{
					luaTable2[num++] = commandMessage;
				}
				num = 1;
				LuaTable luaTable4 = new LuaTable(KernelService.Instance.LuaRuntime);
				foreach (Redbox.IPC.Framework.Error error in clientCommandResult.Errors)
				{
					LuaTable luaTable5 = new LuaTable(KernelService.Instance.LuaRuntime);
					luaTable5["code"] = error.Code;
					luaTable5["description"] = error.Description;
					luaTable5["is_warning"] = error.IsWarning;
					luaTable5["details"] = error.Details;
					luaTable4[num++] = luaTable5;
				}
				luaTable["errors"] = luaTable4;
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
