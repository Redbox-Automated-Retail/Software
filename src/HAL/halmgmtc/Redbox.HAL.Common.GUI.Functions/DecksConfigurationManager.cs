using System;
using System.Xml;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class DecksConfigurationManager
{
	private XmlNode DecksNode;

	private readonly HardwareService Service;

	public XmlDocument Document { get; private set; }

	public DecksConfigurationManager(HardwareService service)
	{
		if (service == null)
		{
			throw new ArgumentException("HardwareService object cannot be null.");
		}
		Service = service;
		LoadConfiguration();
	}

	public XmlNodeList FindAllDeckNodes()
	{
		return DecksNode.ChildNodes;
	}

	public XmlNode FindDeckNode(int deckNumber)
	{
		foreach (XmlNode childNode in DecksNode.ChildNodes)
		{
			if (childNode.GetAttributeValue<int>("Number") == deckNumber)
			{
				return childNode;
			}
		}
		return null;
	}

	public bool FlushChanges(bool refresh)
	{
		HardwareCommandResult hardwareCommandResult = Service.SetConfiguration("controller", Document.OuterXml);
		if (hardwareCommandResult.Success & refresh)
		{
			LoadConfiguration();
		}
		return hardwareCommandResult.Success;
	}

	private void LoadConfiguration()
	{
		HardwareCommandResult configuration = Service.GetConfiguration("controller");
		if (!configuration.Success)
		{
			throw new Exception("Unable to get configuration from HAL.");
		}
		Document = new XmlDocument();
		Document.LoadXml(configuration.CommandMessages[0]);
		DecksNode = Document.DocumentElement.SelectSingleNode("property[@display-name='Decks']");
	}
}
