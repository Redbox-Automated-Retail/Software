using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EXmlFileEditor
{
	private List<Redbox_002EMacros1309_002EXmlEdit> m_edits;

	private List<Redbox_002EMacros1309_002EXmlNamespace> m_namespaces;

	public string FileName { get; internal set; }

	public bool RaiseErrors { get; set; }

	public List<Redbox_002EMacros1309_002EXmlEdit> Edits
	{
		get
		{
			if (m_edits == null)
			{
				m_edits = new List<Redbox_002EMacros1309_002EXmlEdit>();
			}
			return m_edits;
		}
	}

	public bool EditAllMatches { get; set; }

	public List<Redbox_002EMacros1309_002EXmlNamespace> Namespaces
	{
		get
		{
			if (m_namespaces == null)
			{
				m_namespaces = new List<Redbox_002EMacros1309_002EXmlNamespace>();
			}
			return m_namespaces;
		}
	}

	public Redbox_002EMacros1309_002EXmlFileEditor(string fileName)
	{
		FileName = fileName;
	}

	public ReadOnlyCollection<Redbox_002EMacros1309_002EXmlEditorError> Apply()
	{
		List<Redbox_002EMacros1309_002EXmlEditorError> redbox_002EMacros1309_002EXmlEditorErrors = new List<Redbox_002EMacros1309_002EXmlEditorError>();
		try
		{
			if (!File.Exists(FileName))
			{
				redbox_002EMacros1309_002EXmlEditorErrors.Add(new Redbox_002EMacros1309_002EXmlEditorError
				{
					Code = "I006",
					Description = "Unable to edit XML file '" + FileName + "'; it does not exist.",
					Details = "Correct the setup script to specify a valid file path."
				});
				return redbox_002EMacros1309_002EXmlEditorErrors.AsReadOnly();
			}
			NameTable nameTable = new NameTable();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
			foreach (Redbox_002EMacros1309_002EXmlNamespace @namespace in Namespaces)
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
				redbox_002EMacros1309_002EXmlEditorErrors.Add(new Redbox_002EMacros1309_002EXmlEditorError
				{
					Code = "I015",
					Description = "Unable to parse XML file '" + FileName + "'; see details for more information.",
					Details = ex.ToString()
				});
				return redbox_002EMacros1309_002EXmlEditorErrors.AsReadOnly();
			}
			if (xmlDocument.DocumentElement == null)
			{
				return redbox_002EMacros1309_002EXmlEditorErrors.AsReadOnly();
			}
			foreach (Redbox_002EMacros1309_002EXmlEdit edit in Edits)
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
							redbox_002EMacros1309_002EXmlEditorErrors.Add(new Redbox_002EMacros1309_002EXmlEditorError
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
					redbox_002EMacros1309_002EXmlEditorErrors.Add(new Redbox_002EMacros1309_002EXmlEditorError
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
			redbox_002EMacros1309_002EXmlEditorErrors.Add(new Redbox_002EMacros1309_002EXmlEditorError
			{
				Code = "I015",
				Description = $"Unable to edit XML file {FileName}; see details for exception.",
				Details = ex2.ToString()
			});
		}
		return redbox_002EMacros1309_002EXmlEditorErrors.AsReadOnly();
	}
}
