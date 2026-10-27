using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Kernel;

public sealed class KernelService : IKernelService, IDisposable
{
	private const string CollectGarbageScript = "___CollectGarbage";

	private DateTime m_lastGarbageCollection;

	private string m_startupScriptResourceName;

	private readonly IDictionary<string, LuaFunction> m_compiledScripts = new Dictionary<string, LuaFunction>();

	public static KernelService Instance => Singleton<KernelService>.Instance;

	public bool IsApplicationRunning { get; private set; }

	public bool IsTestPlanExecuting { get; private set; }

	internal LuaDebugger LuaDebugger { get; set; }

	internal LuaDebugInfo DebugState { get; set; }

	internal LuaVirtualMachine LuaRuntime { get; set; }

	public event WaitingForDebuggerAction WaitingForDebuggerAction;

	public event DebuggerStopping DebuggerStopping;

	public void Reset()
	{
		IsApplicationRunning = false;
		m_startupScriptResourceName = null;
		EnvironmentFunctions.Reset();
		LogHelper.Instance.Log("Reset kernel service.");
		foreach (KeyValuePair<string, LuaFunction> compiledScript in m_compiledScripts)
		{
			LogHelper.Instance.Log("...Disposing compiled Lua script '{0}'.", compiledScript.Key);
			compiledScript.Value.Dispose();
		}
		m_compiledScripts.Clear();
		if (LuaRuntime != null)
		{
			try
			{
				LuaRuntime.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was rasied in KernelService.Reset.", e);
			}
			finally
			{
				LuaRuntime = null;
			}
		}
		LuaRuntime = new LuaVirtualMachine();
		LogHelper.Instance.Log("...Lua compiler version: {0}, architecture: {1}.", LuaRuntime.Compiler.Version, LuaRuntime.Compiler.Architecture);
		LuaDebugger = new LuaDebugger(LuaRuntime)
		{
			Enabled = false,
			FullTrace = false
		};
		LuaRuntime.HookException += delegate
		{
		};
		LuaDebugger.FullTraceData += delegate(object o, DebugHookEventArgs e2)
		{
			DebugState = e2.DebugInfo;
		};
		LuaDebugger.Stopping += delegate(object o, StoppingEventArgs e2)
		{
			if (DebuggerStopping != null)
			{
				DebuggerStopping(e2.FileName, e2.Line, e2.Action.ToString());
			}
		};
		LuaDebugger.WaitingForAction += delegate
		{
			if (WaitingForDebuggerAction == null)
			{
				LuaDebugger.Run();
			}
			else
			{
				WaitingForDebuggerAction();
			}
		};
		LoadScript("___CollectGarbage", "collectgarbage( 'collect' ); return collectgarbage( 'count' )", out var _);
		ServiceLocator.Instance.GetService<IDebugService>()?.SetDebuggerInstance(LuaDebugger);
		m_lastGarbageCollection = DateTime.Now;
	}

	public void Dispose()
	{
		try
		{
			if (LuaRuntime != null)
			{
				LuaRuntime.Dispose();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KernelService.Dispose.", e);
		}
	}

	public string FormatLuaValue(object value)
	{
		if (value is LuaTable)
		{
			return LuaHelper.FormatLuaValue(value);
		}
		return value.ToString();
	}

	public ReadOnlyCollection<object> ExecuteChunk(string chunk)
	{
		return LuaRuntime.DoString(chunk);
	}

	public ReadOnlyCollection<object> ExecuteFunction(string functionName, params object[] parms)
	{
		if (LuaRuntime[functionName] is LuaFunction luaFunction)
		{
			return luaFunction.Call(parms);
		}
		return null;
	}

	public void LogLuaFunctions()
	{
		LuaFunction.LogLuaFunctions();
	}

	public object ToLuaTable<T>(IEnumerable<T> enumerator)
	{
		if (enumerator == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(LuaRuntime);
		int num = 1;
		foreach (T item in enumerator)
		{
			luaTable[num] = item;
			num++;
		}
		return luaTable;
	}

	public bool RaiseDebuggerOrError(Exception e, string resourceName)
	{
		if (!(e is LuaException e2))
		{
			return false;
		}
		e2.RaiseDebuggerOrError(resourceName);
		return true;
	}

	public void ExecuteApplication()
	{
		if (string.IsNullOrEmpty(m_startupScriptResourceName))
		{
			LogHelper.Instance.Log("No startup script resouce has been loaded into the runtime.", LogEntryType.Error);
			return;
		}
		ExecuteScript(m_startupScriptResourceName);
		IsApplicationRunning = true;
		IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
		if (service != null)
		{
			service.IsApplicationRunning = true;
		}
	}

	public void ExecuteTestPlan(string resourceName)
	{
		if (IsTestPlanExecuting)
		{
			return;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		XmlNode xml = service.GetXml(resourceName);
		if (xml == null)
		{
			LogHelper.Instance.LogTo("TestServices", "Test Control resource not found; Integration tests aborted.");
			return;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("testset");
		if (xmlNodeList == null)
		{
			LogHelper.Instance.LogTo("TestServices", "Test Control contains no valid test sets");
			return;
		}
		IsTestPlanExecuting = true;
		foreach (XmlNode item in xmlNodeList)
		{
			LogHelper.Instance.LogTo("TestServices", "TestSet " + item.GetAttributeValue<string>("name"));
			LogHelper.Instance.LogTo("TestServices", ">  Test: Setup");
			XmlNode testStageNode = item.SelectSingleNode("setup");
			RunTestScripts(testStageNode);
			LogHelper.Instance.LogTo("TestServices", ">  Test: Start Bundle Application");
			ExecuteApplication();
			LogHelper.Instance.LogTo("TestServices", ">  Test: Execution");
			XmlNode testStageNode2 = item.SelectSingleNode("execution");
			RunTestScripts(testStageNode2);
			LogHelper.Instance.LogTo("TestServices", ">  Test: Deactivate Bundle Application");
			service.SetSwitchToBundle(service.ActiveBundle);
			service.Switch(out var _);
			LogHelper.Instance.LogTo("TestServices", ">  Test: Teardown");
			XmlNode testStageNode3 = item.SelectSingleNode("teardown");
			RunTestScripts(testStageNode3);
		}
		IsTestPlanExecuting = false;
		LogHelper.Instance.LogTo("TestServices", "Execute Test Plan completed, exiting the application.");
		if (WaitForSafeShutDown())
		{
			ExecuteChunk("kernel.applyunsafeshutdown()");
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		ServiceLocator.Instance.AddService(typeof(IKernelService), Instance);
		Reset();
		return result;
	}

	public void Shutdown()
	{
		if (LuaRuntime != null)
		{
			LuaRuntime.Dispose();
		}
	}

	public void SetDebuggingFullTrace(bool enabled)
	{
		if (LuaDebugger != null)
		{
			LuaDebugger.FullTrace = enabled;
		}
	}

	public void SetDebuggingState(bool enabled)
	{
		if (LuaDebugger != null)
		{
			LuaDebugger.Enabled = enabled;
		}
	}

	public bool LoadStartupScript(string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			return false;
		}
		string script = ServiceLocator.Instance.GetService<IResourceBundleService>().GetScript(resourceName);
		if (script == null)
		{
			return false;
		}
		LuaFunction luaFunction = LoadScript(resourceName, script, out var _);
		if (luaFunction != null)
		{
			m_startupScriptResourceName = resourceName;
		}
		return luaFunction != null;
	}

	public ReadOnlyCollection<object> ExecuteScript(string resourceName)
	{
		try
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			IResource resource = service.GetResource(resourceName);
			if (resource == null)
			{
				return null;
			}
			if (!LoadDependentScripts(resource.Dependencies, service))
			{
				return null;
			}
			object content = resource.GetAspect("content").GetContent();
			string text = ((!(content is byte[])) ? ((string)content) : Encoding.ASCII.GetString((byte[])content));
			if (string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log("*** Script resource '{0}' content aspect has no content.", resource.Name);
				return null;
			}
			LuaFunction luaFunction = LoadScript(resourceName, text, out var _);
			if (luaFunction == null)
			{
				LogHelper.Instance.Log("Unable to compile script resource '{0}'", resourceName);
				return null;
			}
			ReadOnlyCollection<object> result = luaFunction.Call();
			if ((DateTime.Now - m_lastGarbageCollection).Minutes > 5 && m_compiledScripts.ContainsKey("___CollectGarbage"))
			{
				ReadOnlyCollection<object> readOnlyCollection = m_compiledScripts["___CollectGarbage"].Call();
				LogHelper.Instance.Log("Lua garbage collection cycle complete, current Lua heap size: {0}", readOnlyCollection[0]);
				m_lastGarbageCollection = DateTime.Now;
			}
			return result;
		}
		catch (Exception e)
		{
			RaiseDebuggerOrError(e, resourceName);
			return null;
		}
	}

	public void RegisterFunctions(ReadOnlyCollection<KernelFunctionInfo> functionInfos)
	{
		if (LuaRuntime == null)
		{
			return;
		}
		foreach (KernelFunctionInfo functionInfo in functionInfos)
		{
			foreach (KernelFunctionAttribute attribute in functionInfo.Attributes)
			{
				string fullPath = attribute.GetNamespace();
				if (LuaRuntime[fullPath] == null)
				{
					LuaRuntime[fullPath] = new LuaTable(LuaRuntime);
				}
				LuaRuntime.RegisterMethod(attribute.Name, functionInfo.Method, compileStaticCallSite: true);
			}
		}
	}

	public object NewTable()
	{
		return new LuaTable(LuaRuntime);
	}

	internal LuaFunction LoadScript(string scriptName, string scriptSource, out bool wasCached)
	{
		wasCached = true;
		if (!m_compiledScripts.ContainsKey(scriptName))
		{
			m_compiledScripts[scriptName] = LuaRuntime.LoadString(scriptSource, scriptName);
			wasCached = false;
		}
		return m_compiledScripts[scriptName];
	}

	private KernelService()
	{
	}

	private bool LoadDependentScripts(IEnumerable<string> resourceNames, IResourceBundleService bundleService)
	{
		if (!bundleService.HasActiveBundle())
		{
			return false;
		}
		foreach (string resourceName in resourceNames)
		{
			IResource resource = bundleService.GetResource(resourceName);
			if (resource == null)
			{
				continue;
			}
			if (resource.Dependencies.Count > 0)
			{
				LogHelper.Instance.Log(".....Dependent resources: {1}", resource.Name, resource.Dependencies.Join(","));
				if (!LoadDependentScripts(resource.Dependencies, bundleService))
				{
					return false;
				}
			}
			string text = (string)resource.GetAspect("content").GetContent();
			if (string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log("*** Script resource '{0}' content aspect is empty.", resource.Name);
				continue;
			}
			LuaFunction luaFunction = LoadScript(resource.Name, text, out var wasCached);
			if (!wasCached)
			{
				try
				{
					LogHelper.Instance.Log("...Execute script resource '{0}'.", resource.Name);
					luaFunction.Call();
				}
				catch (Exception e)
				{
					RaiseDebuggerOrError(e, resource.Name);
					return false;
				}
			}
		}
		return true;
	}

	private void RunTestScripts(XmlNode testStageNode)
	{
		if (testStageNode == null)
		{
			return;
		}
		XmlNodeList xmlNodeList = testStageNode.SelectNodes("script");
		if (xmlNodeList == null)
		{
			return;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		foreach (XmlNode item in xmlNodeList)
		{
			string attributeValue = item.GetAttributeValue<string>("value");
			string chunk = Encoding.UTF8.GetString(service.ActiveBundle.Storage.GetEntryContent(attributeValue));
			ExecuteChunk(chunk);
		}
	}

	private bool WaitForSafeShutDown()
	{
		try
		{
			DateTime now = DateTime.Now;
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			IKernelExtensionService service = ServiceLocator.Instance.GetService<IKernelExtensionService>();
			Redbox.KioskEngine.ComponentModel.ErrorList errors;
			while (!service.CanSwitch(out errors))
			{
				LogHelper.Instance.Log("Unsafe for shut down; wait for 15 seconds before next check.");
				manualResetEvent.WaitOne(15000);
				if ((DateTime.Now - now).TotalMinutes >= 5.0)
				{
					LogHelper.Instance.Log("Unsafe for shut down timed out unsuccessfully.");
					return false;
				}
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
