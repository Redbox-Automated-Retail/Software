using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "edit-xml-file")]
[DisplayName("Edit XML File")]
public class XmlEditFileDefinition : Definition
{
	private class XmlNamespace
	{
		public string Prefix { get; set; }

		public string Uri { get; set; }

		public XmlNamespace(string prefix, string uri)
		{
			Prefix = prefix;
			Uri = uri;
		}
	}

	private class XmlEdit
	{
		public string XPath { get; set; }

		public string Value { get; set; }

		public XmlEdit(string xpath, string value)
		{
			XPath = xpath;
			Value = value;
		}
	}

	private string m_fileName;

	private List<XmlEdit> m_edits;

	private string m_backupExtension;

	private List<XmlNamespace> m_namespaces;

	[ElementProperty(Name = "filename", IsRequired = true)]
	public string FileName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_fileName);
		}
		set
		{
			m_fileName = value;
		}
	}

	[ElementProperty(Name = "edit-all-matches")]
	public bool EditAllMatches { get; set; }

	[ElementProperty(Name = "raise-errors")]
	public bool RaiseErrors { get; set; }

	[ElementProperty(Name = "backup")]
	public bool BackupBeforeEdit { get; set; }

	[ElementProperty(Name = "backup-extension")]
	public string BackupExtension
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_backupExtension);
		}
		set
		{
			m_backupExtension = value;
		}
	}

	private List<XmlNamespace> Namespaces
	{
		get
		{
			if (m_namespaces == null)
			{
				m_namespaces = new List<XmlNamespace>();
			}
			return m_namespaces;
		}
	}

	private List<XmlEdit> Edits
	{
		get
		{
			if (m_edits == null)
			{
				m_edits = new List<XmlEdit>();
			}
			return m_edits;
		}
	}

	public override int GetTaskCount()
	{
		if (InstallerProject.Instance.Mode != InstallMode.Install)
		{
			return 0;
		}
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return $"Edit XML file '{FileName}'";
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (!File.Exists(FileName))
			{
				errors.Add(Error.NewError("I006", "Unable to edit XML file '" + FileName + "'; it does not exist.", "Correct the setup script to specify a valid file path."));
				return;
			}
			if (BackupBeforeEdit)
			{
				FileHelper.CreateBackup(FileName, BackupExtension);
			}
			NameTable nameTable = new NameTable();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
			foreach (XmlNamespace @namespace in Namespaces)
			{
				xmlNamespaceManager.AddNamespace(MacroContainer.Instance.ExpandRuntimeMacros(@namespace.Prefix), MacroContainer.Instance.ExpandRuntimeMacros(@namespace.Uri));
			}
			XmlDocument xmlDocument = new XmlDocument(nameTable);
			try
			{
				xmlDocument.Load(FileName);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("I015", "Unable to parse XML file '" + FileName + "'; see details for more information.", ex.ToString()));
				return;
			}
			if (xmlDocument.DocumentElement == null)
			{
				return;
			}
			foreach (XmlEdit edit in Edits)
			{
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(edit.XPath);
				if (xmlNodeList != null && xmlNodeList.Count > 0)
				{
					List<XmlNode> list = new List<XmlNode>();
					if (EditAllMatches)
					{
						foreach (XmlNode item in xmlNodeList)
						{
							list.Add(item);
						}
					}
					else
					{
						list.Add(xmlNodeList[0]);
					}
					foreach (XmlNode item2 in list)
					{
						if (item2 != null)
						{
							switch (item2.NodeType)
							{
							case XmlNodeType.Attribute:
								item2.Value = MacroContainer.Instance.ExpandRuntimeMacros(edit.Value);
								break;
							case XmlNodeType.Element:
								item2.InnerText = MacroContainer.Instance.ExpandRuntimeMacros(edit.Value);
								break;
							default:
								errors.Add(Error.NewError("I015", $"XPath expression '{edit.XPath}' yielded an XML node type RIX cannot process.", "Review your XPath expression, correct, and recompile your setup.  RIX supports editing XML attributes and element nodes."));
								break;
							}
						}
					}
				}
				else if (RaiseErrors)
				{
					errors.Add(Error.NewError("I015", $"XPath expression '{edit.XPath}' did not yield a valid node(s).", "Review your XPath expression, correct, and recompile your setup."));
				}
			}
			using XmlTextWriter xmlTextWriter = new XmlTextWriter(FileName, null);
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlDocument.Save(xmlTextWriter);
			xmlTextWriter.Flush();
			xmlTextWriter.Close();
		}
		catch (Exception ex2)
		{
			errors.Add(Error.NewError("I015", $"Unable to edit XML file {FileName}; see details for exception.", ex2.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		XmlNodeList xmlNodeList = node.SelectNodes("namespace");
		if (xmlNodeList != null)
		{
			foreach (XmlNode item in xmlNodeList)
			{
				string expandedAttributeValue = item.GetExpandedAttributeValue<string>("prefix");
				if (string.IsNullOrEmpty(expandedAttributeValue))
				{
					errors.Add(Error.NewError("C004", "The namespace node requires a vald prefix attribute.", "Specify a valid prefix attribute."));
					continue;
				}
				string expandedAttributeValue2 = item.GetExpandedAttributeValue<string>("uri");
				if (string.IsNullOrEmpty(expandedAttributeValue2))
				{
					errors.Add(Error.NewError("C004", "The namespace node requires a vald uri attribute.", "Specify a valid uri attribute."));
				}
				else
				{
					Namespaces.Add(new XmlNamespace(expandedAttributeValue, expandedAttributeValue2));
				}
			}
		}
		XmlNodeList xmlNodeList2 = node.SelectNodes("edit");
		if (xmlNodeList2 == null)
		{
			return;
		}
		foreach (XmlNode item2 in xmlNodeList2)
		{
			string expandedAttributeValue3 = item2.GetExpandedAttributeValue<string>("xpath");
			if (string.IsNullOrEmpty(expandedAttributeValue3))
			{
				errors.Add(Error.NewError("C004", "The edit node requires a vald xpath attribute.", "Specify a valid xpath attribute."));
				continue;
			}
			string expandedAttributeValue4 = item2.GetExpandedAttributeValue<string>("value");
			if (string.IsNullOrEmpty(expandedAttributeValue4))
			{
				errors.Add(Error.NewError("C004", "The edit node requires a vald value attribute.", "Specify a valid value attribute."));
			}
			else
			{
				Edits.Add(new XmlEdit(expandedAttributeValue3, expandedAttributeValue4));
			}
		}
	}
}
