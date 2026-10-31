using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Redbox.Core;

public class XmlFileEditor
{
	private List<XmlEdit> m_edits;

	private List<XmlNamespace> m_namespaces;

	public string FileName { get; internal set; }

	public bool RaiseErrors { get; set; }

	public List<XmlEdit> Edits
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

	public bool EditAllMatches { get; set; }

	public List<XmlNamespace> Namespaces
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

	public XmlFileEditor(string fileName)
	{
		FileName = fileName;
	}

	public ReadOnlyCollection<XmlEditorError> Apply()
	{
		List<XmlEditorError> list = new List<XmlEditorError>();
		try
		{
			if (!File.Exists(FileName))
			{
				list.Add(new XmlEditorError
				{
					Code = "I006",
					Description = "Unable to edit XML file '" + FileName + "'; it does not exist.",
					Details = "Correct the setup script to specify a valid file path."
				});
				return list.AsReadOnly();
			}
			NameTable nameTable = new NameTable();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
			foreach (XmlNamespace @namespace in Namespaces)
			{
				xmlNamespaceManager.AddNamespace(@namespace.Prefix, @namespace.Uri);
			}
			XmlDocument xmlDocument = new XmlDocument(nameTable);
			try
			{
				xmlDocument.Load(FileName);
			}
			catch (Exception ex)
			{
				list.Add(new XmlEditorError
				{
					Code = "I015",
					Description = "Unable to parse XML file '" + FileName + "'; see details for more information.",
					Details = ex.ToString()
				});
				return list.AsReadOnly();
			}
			if (xmlDocument.DocumentElement == null)
			{
				return list.AsReadOnly();
			}
			foreach (XmlEdit edit in Edits)
			{
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(edit.XPath);
				if (xmlNodeList != null && xmlNodeList.Count > 0)
				{
					List<XmlNode> list2 = new List<XmlNode>();
					if (EditAllMatches)
					{
						foreach (XmlNode item in xmlNodeList)
						{
							list2.Add(item);
						}
					}
					else
					{
						list2.Add(xmlNodeList[0]);
					}
					foreach (XmlNode item2 in list2)
					{
						if (item2 != null)
						{
							switch (item2.NodeType)
							{
							case XmlNodeType.Attribute:
								item2.Value = edit.Value;
								continue;
							case XmlNodeType.Element:
								item2.InnerText = edit.Value;
								continue;
							}
							list.Add(new XmlEditorError
							{
								Code = "I015",
								Description = $"XPath expression '{edit.XPath}' yielded an XML node type RIX cannot process.",
								Details = "Review your XPath expression, correct, and recompile your setup.  RIX supports editing XML attributes and element nodes."
							});
						}
					}
				}
				else if (RaiseErrors)
				{
					list.Add(new XmlEditorError
					{
						Code = "I015",
						Description = $"XPath expression '{edit.XPath}' did not yield a valid node(s).",
						Details = "Review your XPath expression, correct, and recompile your setup."
					});
				}
			}
			XmlTextWriter xmlTextWriter = new XmlTextWriter(FileName, null);
			xmlTextWriter.Formatting = Formatting.Indented;
			XmlTextWriter w = xmlTextWriter;
			xmlDocument.Save(w);
		}
		catch (Exception ex2)
		{
			list.Add(new XmlEditorError
			{
				Code = "I015",
				Description = $"Unable to edit XML file {FileName}; see details for exception.",
				Details = ex2.ToString()
			});
		}
		return list.AsReadOnly();
	}
}
