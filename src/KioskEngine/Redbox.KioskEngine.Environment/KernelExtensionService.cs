using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class KernelExtensionService : IKernelExtensionService
{
	private readonly object m_syncObject = new object();

	private IDictionary<Guid, IKernelExtension> m_innerExtensions;

	public static IKernelExtensionService Instance => Singleton<KernelExtensionService>.Instance;

	public ReadOnlyCollection<IKernelExtension> Extensions => new List<IKernelExtension>(InnerExtensions.Values).AsReadOnly();

	public string[] SearchPath
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service != null)
			{
				return service.GetValue("Core", "KernelExtensionSearchPath", new string[1] { "..\\kext" });
			}
			return new string[1] { "..\\kext" };
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "KernelExtensionSearchPath", value);
		}
	}

	internal IDictionary<Guid, IKernelExtension> InnerExtensions
	{
		get
		{
			if (m_innerExtensions == null)
			{
				m_innerExtensions = new Dictionary<Guid, IKernelExtension>();
			}
			return m_innerExtensions;
		}
	}

	public IKernelExtension GetExtension(Guid id)
	{
		lock (m_syncObject)
		{
			return InnerExtensions.ContainsKey(id) ? InnerExtensions[id] : null;
		}
	}

	public ErrorList Initialize(List<Type> kernelExtensionHostTypes)
	{
		LogHelper.Instance.Log("Initialize kernel extension service:");
		ErrorList errorList = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(IKernelExtensionService), Instance);
		foreach (Type kernelExtensionHostType in kernelExtensionHostTypes)
		{
			errorList.AddRange(LoadExtension(kernelExtensionHostType));
			LogHelper.Instance.Log("...Loaded kernel extension '{0}'", kernelExtensionHostType);
		}
		return errorList;
	}

	public ErrorList LoadExtension(Type extensionHostType)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (extensionHostType != null)
			{
				string version = "0.0.0.0";
				Assembly assembly = extensionHostType?.Assembly;
				if (assembly != null)
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
					if (versionInfo != null)
					{
						version = versionInfo.FileVersion;
					}
				}
				if (Activator.CreateInstance(extensionHostType) is IKernelExtensionHost kernelExtensionHost)
				{
					Guid? guid = Guid.NewGuid();
					KernelExtension value = (KernelExtension)(kernelExtensionHost.Extension = new KernelExtension
					{
						Host = kernelExtensionHost,
						ID = guid.Value,
						IsUnloadSupported = false,
						Name = extensionHostType.ToString(),
						Version = version
					});
					errorList.AddRange(kernelExtensionHost.PreLoad());
					InnerExtensions[guid.Value] = value;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("P999", $"An unhandled exception was raised while loading kernel extension: {extensionHostType}.   See the details of this error for more information on the exception.", e));
		}
		return errorList;
	}

	public void NotifyOfHostCrash(Exception e)
	{
		lock (m_syncObject)
		{
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					value.Host.HandleHostCrash(e);
				}
			}
		}
	}

	public ErrorList LoadAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Load());
				}
			}
			return errorList;
		}
	}

	public ErrorList ResetAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Reset());
				}
			}
			return errorList;
		}
	}

	public ErrorList ActivateAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Activate());
				}
			}
			return errorList;
		}
	}

	public ErrorList DeactivateAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Deactivate());
				}
			}
			return errorList;
		}
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool result = true;
		lock (m_syncObject)
		{
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null && !value.Host.CanSwitch(out var errors2))
				{
					result = false;
					errors.AddRange(errors2);
				}
			}
			return result;
		}
	}

	private KernelExtensionService()
	{
	}
}
