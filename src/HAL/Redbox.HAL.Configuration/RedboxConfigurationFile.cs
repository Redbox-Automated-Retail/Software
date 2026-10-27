using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Configuration;

internal sealed class RedboxConfigurationFile : IConfigurationFile
{
	public SystemConfigurations Type => SystemConfigurations.Redbox;

	public string Path => "c:\\program files\\Redbox\\Halservice\\bin";

	public string FileName => "hal.xml";

	public string FullSourcePath { get; private set; }

	public void ImportFrom(IConfigurationFile config, ErrorList errors)
	{
		throw new NotImplementedException();
	}

	public ConversionResult ConvertTo(KioskConfiguration newconfig, ErrorList errors)
	{
		if (!File.Exists(FullSourcePath))
		{
			return ConversionResult.InvalidFile;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(FullSourcePath);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("C001", "Invalid XML file", ex.Message));
			return ConversionResult.InvalidFile;
		}
		XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("Controller/Decks");
		XmlNode lastChild = xmlNode.LastChild;
		if (lastChild.GetAttributeValue<bool>("IsQlm"))
		{
			XmlNode xmlNode2 = null;
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				int attributeValue = childNode.GetAttributeValue<int>("Number");
				if (7 == attributeValue)
				{
					xmlNode2 = childNode;
					break;
				}
			}
			XmlNode xmlNode4 = xmlNode2.Clone();
			xmlNode4.SetAttributeValue("Number", 8);
			xmlNode4.SetAttributeValue("Offset", lastChild.GetAttributeValue<int>("Offset"));
			xmlNode.ReplaceChild(xmlNode4, lastChild);
			xmlDocument.Save(FullSourcePath);
		}
		return ConversionResult.Success;
	}

	internal RedboxConfigurationFile()
	{
		FullSourcePath = System.IO.Path.Combine(Path, FileName);
	}
}
