using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.JScript.Vsa;
using Microsoft.Vsa;

namespace Redbox.ScriptingHost.Framework;

public class VsaScriptingHost : KernelObject, IVsaSite, IDisposable
{
	private IDictionary<string, object> m_evaluationResults;

	private IDictionary<string, IVsaKernelObject> m_instanceCache;

	private static readonly string[] m_defaultAssemblyReferences = new string[7] { "Mscorlib.dll", "System.dll", "System.Data.dll", "System.Drawing.dll", "System.Windows.Forms.dll", "System.Xml.dll", "Microsoft.Vsa.dll" };

	public bool IsRunning => Engine.IsRunning;

	internal VsaEngine Engine { get; set; }

	internal IDictionary<string, object> EvaluationResults
	{
		get
		{
			if (m_evaluationResults == null)
			{
				m_evaluationResults = new Dictionary<string, object>();
			}
			return m_evaluationResults;
		}
	}

	internal IDictionary<string, IVsaKernelObject> InstanceCache
	{
		get
		{
			if (m_instanceCache == null)
			{
				m_instanceCache = new Dictionary<string, IVsaKernelObject>();
			}
			return m_instanceCache;
		}
	}

	private int InstanceNumber { get; set; }

	private static int InstanceCount { get; set; }

	public event EventHandler CompileError;

	public VsaScriptingHost()
	{
		Initialize();
	}

	public void Stop()
	{
		if (Engine.IsRunning)
		{
			Engine.Reset();
		}
	}

	public void Dispose()
	{
		if (Engine != null)
		{
			Engine.Close();
			Engine = null;
		}
	}

	public void ExecuteScript(string sourceText)
	{
		if (Engine.IsRunning)
		{
			Engine.Reset();
		}
		IVsaCodeItem vsaCodeItem = CreateCodeItem(IdHelper.GeneratePseudoUniqueId(8, "_ABCDEFGHIJKLMNOPQRSTUV"));
		if (vsaCodeItem != null)
		{
			vsaCodeItem.SourceText = sourceText;
			if (Engine.Compile())
			{
				Engine.Run();
			}
		}
	}

	public object EvaluateScript(string sourceText)
	{
		string text = Guid.NewGuid().ToString();
		sourceText = sourceText.Replace("'", "\\'");
		ExecuteScript($"Host.SetEvaluationResult( '{text}', eval('{sourceText}', 'unsafe') );");
		object result = (EvaluationResults.ContainsKey(text) ? EvaluationResults[text] : null);
		EvaluationResults.Remove(text);
		return result;
	}

	public void SetKernelObject(string itemName, IVsaKernelObject instance)
	{
		SetKernelObject(itemName, instance, useTypeInformation: false);
	}

	public void SetKernelObject(string itemName, IVsaKernelObject instance, bool useTypeInformation)
	{
		if (InstanceCache.ContainsKey(itemName))
		{
			Engine.Items.Remove(itemName);
		}
		InstanceCache[itemName] = instance;
		IVsaGlobalItem vsaGlobalItem = (IVsaGlobalItem)Engine.Items.CreateItem(itemName, VsaItemType.AppGlobal, VsaItemFlag.None);
		if (useTypeInformation)
		{
			vsaGlobalItem.TypeString = instance.GetType().FullName;
		}
	}

	public bool RemoveKernelObject(string itemName)
	{
		bool result = InstanceCache.Remove(itemName);
		Engine.Items.Remove(itemName);
		return result;
	}

	public bool ContainsKernelObject(string itemName)
	{
		return InstanceCache.ContainsKey(itemName);
	}

	public IVsaKernelObject GetKernelObject(string itemName)
	{
		return InstanceCache.ContainsKey(itemName) ? InstanceCache[itemName] : null;
	}

	public IVsaItem GetEngineItem(string itemName)
	{
		foreach (IVsaItem item in Engine.Items)
		{
			if (string.Compare(item.Name, itemName, ignoreCase: true) == 0)
			{
				return item;
			}
		}
		return null;
	}

	public bool ContainsEngineItem(string itemName)
	{
		return GetEngineItem(itemName) != null;
	}

	public IVsaReferenceItem CreateReferenceItem(string itemName, string assemblyName)
	{
		IVsaReferenceItem vsaReferenceItem = (IVsaReferenceItem)Engine.Items.CreateItem(itemName, VsaItemType.Reference, VsaItemFlag.None);
		vsaReferenceItem.AssemblyName = assemblyName;
		return vsaReferenceItem;
	}

	public IVsaCodeItem CreateCodeItem(string itemName)
	{
		try
		{
			return (IVsaCodeItem)Engine.Items.CreateItem(itemName, VsaItemType.Code, VsaItemFlag.None);
		}
		catch
		{
			return null;
		}
	}

	public ReadOnlyCollection<IVsaReferenceItem> CreateAssemblyReferences(params string[] assemblyNames)
	{
		List<IVsaReferenceItem> list = new List<IVsaReferenceItem>();
		foreach (string text in assemblyNames)
		{
			list.Add(CreateReferenceItem(text, text));
		}
		return list.AsReadOnly();
	}

	public void GetCompiledState(out byte[] pe, out byte[] debugInfo)
	{
		pe = (debugInfo = null);
	}

	public bool OnCompilerError(IVsaError error)
	{
		if (CompileError != null)
		{
			CompileError(error.ToString(), EventArgs.Empty);
		}
		return true;
	}

	public object GetGlobalInstance(string name)
	{
		return InstanceCache.ContainsKey(name) ? InstanceCache[name] : null;
	}

	public object GetEventSourceInstance(string itemName, string eventSourceName)
	{
		return null;
	}

	public void Notify(string notify, object info)
	{
	}

	public void SetEvaluationResult(string key, object result)
	{
		EvaluationResults[key] = result;
	}

	internal void Initialize()
	{
		lock (this)
		{
			InstanceCount++;
			InstanceNumber = InstanceCount;
		}
		Engine = new VsaEngine
		{
			RootMoniker = $"redbox://RedboxVsaEngine/Instance{InstanceNumber}",
			Site = this
		};
		Engine.InitNew();
		Engine.RootNamespace = "redbox";
		Engine.SetOption("AlwaysGenerateIL", false);
		Engine.SetOption("CLSCompliant", false);
		CreateAssemblyReferences(m_defaultAssemblyReferences);
		CreateAssemblyReferences(Assembly.GetExecutingAssembly().Location);
		SetKernelObject("Host", this);
	}
}
