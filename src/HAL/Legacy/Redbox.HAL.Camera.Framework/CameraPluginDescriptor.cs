using System;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

internal sealed class CameraPluginDescriptor
{
	internal string PluginName { get; set; }

	internal string AssemblyName { get; set; }

	internal string PluginNamespace { get; set; }

	internal string PluginServiceProviderMethodName { get; set; }

	internal string Version { get; set; }

	internal string DescriptorPath { get; private set; }

	internal static CameraPluginDescriptor FromXml(string xmlPath)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlPath);
			if (xmlDocument.DocumentElement == null)
			{
				return null;
			}
			XmlNode xmlNode = xmlDocument.SelectSingleNode("Plugin");
			if (xmlNode == null)
			{
				return null;
			}
			CameraPluginDescriptor cameraPluginDescriptor = new CameraPluginDescriptor();
			cameraPluginDescriptor.DescriptorPath = xmlPath;
			XmlNodeList childNodes = xmlNode.ChildNodes;
			foreach (XmlNode item in childNodes)
			{
				if (item.Name.Equals("PluginType"))
				{
					cameraPluginDescriptor.PluginName = item.Attributes.GetNamedItem("Name").Value;
					cameraPluginDescriptor.Version = item.Attributes.GetNamedItem("Version").Value;
				}
				else if (item.Name.Equals("Assembly"))
				{
					cameraPluginDescriptor.AssemblyName = item.Attributes.GetNamedItem("Name").Value;
				}
				else if (item.Name.Equals("PluginProvider"))
				{
					cameraPluginDescriptor.PluginServiceProviderMethodName = item.Attributes.GetNamedItem("Method").Value;
					cameraPluginDescriptor.PluginNamespace = item.Attributes.GetNamedItem("Namespace").Value;
				}
			}
			return cameraPluginDescriptor;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unable to read plugin descriptor", ex);
			return null;
		}
	}
}
