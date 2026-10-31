using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

internal sealed class PluginLoader
{
	private string m_pluginsDirectory;

	private List<CameraPluginDescriptor> m_pluginDescriptors;

	internal static PluginLoader Instance => Singleton<PluginLoader>.Instance;

	internal List<CameraPluginDescriptor> Plugins
	{
		get
		{
			if (m_pluginDescriptors == null)
			{
				m_pluginDescriptors = new List<CameraPluginDescriptor>();
				string[] files = Directory.GetFiles(PluginsDirectory, "*.xml");
				string[] array = files;
				foreach (string xmlPath in array)
				{
					CameraPluginDescriptor cameraPluginDescriptor = CameraPluginDescriptor.FromXml(xmlPath);
					if (cameraPluginDescriptor != null)
					{
						m_pluginDescriptors.Add(cameraPluginDescriptor);
					}
				}
			}
			return m_pluginDescriptors;
		}
	}

	private string PluginsDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_pluginsDirectory))
			{
				m_pluginsDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "camera.plugins");
			}
			return m_pluginsDirectory;
		}
	}

	internal CameraPluginDescriptor LocatePlugin(string name, out ICameraPlugin plugin)
	{
		plugin = null;
		CameraPluginDescriptor cameraPluginDescriptor = Plugins.Find((CameraPluginDescriptor d) => name.Equals(d.PluginName, StringComparison.CurrentCultureIgnoreCase));
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
	}

	private bool LocatePluginInner(CameraPluginDescriptor descriptor, out ICameraPlugin plugin)
	{
		plugin = null;
		string assemblyFile = Path.Combine(PluginsDirectory, descriptor.AssemblyName);
		Assembly assembly = Assembly.LoadFrom(assemblyFile);
		Type[] exportedTypes = assembly.GetExportedTypes();
		Type[] array = exportedTypes;
		foreach (Type type in array)
		{
			if (type.FullName.Equals(descriptor.PluginNamespace))
			{
				MethodInfo method = type.GetMethod(descriptor.PluginServiceProviderMethodName, BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					object obj = method.Invoke(null, null);
					plugin = (ICameraPlugin)obj;
					return plugin != null;
				}
			}
		}
		return false;
	}
}
