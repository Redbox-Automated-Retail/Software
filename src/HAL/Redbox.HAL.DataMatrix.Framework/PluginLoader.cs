using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class PluginLoader
{
	private readonly string PluginsDirectory;

	private readonly List<CameraPluginDescriptor> PluginDescriptors;

	internal static PluginLoader Instance => Singleton<PluginLoader>.Instance;

	internal CameraPluginDescriptor LocatePlugin(string name, out ICameraPlugin plugin)
	{
		plugin = null;
		CameraPluginDescriptor cameraPluginDescriptor = PluginDescriptors.Find((CameraPluginDescriptor d) => name.Equals(d.PluginName, StringComparison.CurrentCultureIgnoreCase));
		if (cameraPluginDescriptor == null)
		{
			return null;
		}
		if (!LocatePluginInner(cameraPluginDescriptor, out plugin))
		{
			return null;
		}
		return cameraPluginDescriptor;
	}

	private PluginLoader()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		PluginsDirectory = service.RuntimePath("camera.plugins");
		string[] files = Directory.GetFiles(PluginsDirectory, "*.xml");
		PluginDescriptors = new List<CameraPluginDescriptor>();
		if (files.Length == 0)
		{
			LogHelper.Instance.Log("[Camera Plugin Manager] There are no plugin descriptor files.", LogEntryType.Error);
			return;
		}
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			CameraPluginDescriptor cameraPluginDescriptor = CameraPluginDescriptor.FromXml(array[i]);
			if (cameraPluginDescriptor != null)
			{
				PluginDescriptors.Add(cameraPluginDescriptor);
			}
		}
	}

	private bool LocatePluginInner(CameraPluginDescriptor descriptor, out ICameraPlugin plugin)
	{
		plugin = null;
		Type[] exportedTypes = Assembly.LoadFrom(Path.Combine(PluginsDirectory, descriptor.AssemblyName)).GetExportedTypes();
		foreach (Type type in exportedTypes)
		{
			if (type.FullName.Equals(descriptor.PluginNamespace))
			{
				MethodInfo method = type.GetMethod(descriptor.PluginServiceProviderMethodName, BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					plugin = method.Invoke(null, null) as ICameraPlugin;
					return plugin != null;
				}
			}
		}
		return false;
	}
}
