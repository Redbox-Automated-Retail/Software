using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Script.Framework;

internal sealed class PollerManager
{
	private readonly Dictionary<string, ScriptPoller> PollerMap = new Dictionary<string, ScriptPoller>();

	public ScriptPoller Locate(string name)
	{
		foreach (string key in PollerMap.Keys)
		{
			if (key == name)
			{
				return PollerMap[key];
			}
		}
		return null;
	}

	public void StartPollers()
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.Start();
			return true;
		});
	}

	public void StopPollers()
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.Shutdown(joinThread: false);
			return true;
		});
	}

	public void InitializePollers()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string assemblyPath = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
		Assembly assembly = null;
		try
		{
			assembly = Assembly.LoadFrom(assemblyPath);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[PollerManager] Unable to load assembly '{assemblyPath}' to scan for commands.", e);
			return;
		}
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			PollerAttribute[] attributes = (PollerAttribute[])type.GetCustomAttributes(typeof(PollerAttribute), inherit: false);
			if (attributes != null && attributes.Length != 0 && !string.IsNullOrEmpty(attributes[0].Name))
			{
				try
				{
					ScriptPoller scriptPoller = Activator.CreateInstance(type, nonPublic: true) as ScriptPoller;
					PollerMap[attributes[0].Name] = scriptPoller;
					scriptPoller.PollerName = attributes[0].Name;
					scriptPoller.ConfigNotifications = attributes[0].ConfigNotifications;
					LogHelper.Instance.Log("[PollerManager] Registered poller {0}", scriptPoller.PollerName);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log($"[PollerManager] Unable to create type {type.ToString()}", e2);
				}
			}
		}
		LogHelper.Instance.Log("[PollerManager] Time to scan for {0} pollers: {1}", PollerMap.Keys.Count, executionTimer.Elapsed);
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.Initialize();
			return true;
		});
	}

	internal PollerManager()
	{
	}

	private void DoForAll(Predicate<ScriptPoller> predicate)
	{
		foreach (string key in PollerMap.Keys)
		{
			ScriptPoller poller = PollerMap[key];
			if (!predicate(poller))
			{
				break;
			}
		}
	}
}
