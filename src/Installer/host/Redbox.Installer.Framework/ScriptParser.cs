using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

public class ScriptParser
{
	private readonly string m_fileName;

	private readonly IConsole m_console;

	public ScriptParser(string fileName, IConsole console)
	{
		m_console = console;
		m_fileName = fileName;
	}

	public ErrorList Compile(string xml, Assembly[] assemblies, bool checkFiles, out string outerXml)
	{
		outerXml = null;
		string text = null;
		ErrorList errorList = new ErrorList();
		if (xml == null)
		{
			text = Path.GetDirectoryName(Path.GetFullPath(m_fileName));
			if (!File.Exists(m_fileName))
			{
				errorList.Add(Error.NewError("C001", "The script file '" + m_fileName + "' does not exist.", "Check the path of the script file specified on the command line."));
				return errorList;
			}
		}
		m_console.WriteLine("Compiling: " + m_fileName);
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			if (xml == null)
			{
				xmlDocument.Load(m_fileName);
			}
			else
			{
				xmlDocument.LoadXml(xml);
			}
		}
		catch (Exception ex)
		{
			errorList.Add(Error.NewError("C002", "Unable to parse script '" + m_fileName + "'; see details for more information.", ex.ToString()));
			return errorList;
		}
		if (xmlDocument.DocumentElement == null)
		{
			return errorList;
		}
		if (xmlDocument.DocumentElement.Attributes["id"] == null)
		{
			errorList.Add(Error.NewError("C005", "The rix node requires an id attribute.", "Specify a valid id attribute with a GUID value."));
		}
		if (xmlDocument.DocumentElement.Attributes["name"] == null)
		{
			errorList.Add(Error.NewError("C005", "The rix node requires a name attribute.", "Specify a valid name attribute."));
		}
		if (errorList.Count > 0)
		{
			return errorList;
		}
		InstallerProject.Instance.ID = xmlDocument.DocumentElement.GetExpandedAttributeValue<string>("id");
		InstallerProject.Instance.Name = xmlDocument.DocumentElement.GetExpandedAttributeValue<string>("name");
		InstallerProject.Instance.Icon = xmlDocument.DocumentElement.GetExpandedAttributeValue<string>("icon");
		InstallerProject.Instance.AllowModify = xmlDocument.DocumentElement.GetExpandedAttributeValue<bool>("allow-modify");
		InstallerProject.Instance.IncludeUninstall = xmlDocument.DocumentElement.GetExpandedAttributeValue("uninstall", defaultValue: true);
		InstallerProject.Instance.RebootMode = xmlDocument.DocumentElement.GetExpandedAttributeValue("reboot", RebootMode.IfNecessary);
		InstallerProject.Instance.SecurityLevel = xmlDocument.DocumentElement.GetExpandedAttributeValue("security-level", SecurityLevel.User);
		InstallerProject.Instance.InstallArchitecture = xmlDocument.DocumentElement.GetExpandedAttributeValue("architecture", Architecture.x86);
		ParseProperties(xmlDocument, errorList);
		ParseIncludedScripts(xmlDocument, text, errorList);
		DefinitionList collection = Definition.Parse(xmlDocument, text, checkFiles, m_console, errorList);
		if (errorList.Count > 0)
		{
			return errorList;
		}
		InstallerProject.Instance.Definitions.AddRange(collection);
		AddDeterminedSystemValues(errorList);
		outerXml = xmlDocument.OuterXml;
		return errorList;
	}

	private void ParseProperties(XmlDocument xmlDocument, ErrorList errors)
	{
		if (xmlDocument == null || xmlDocument.DocumentElement == null)
		{
			return;
		}
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
		if (xmlNodeList == null || xmlNodeList.Count <= 0)
		{
			return;
		}
		m_console.WriteLine("\nParsing properties:");
		foreach (XmlNode item in xmlNodeList)
		{
			if (item.Attributes["name"] == null)
			{
				errors.Add(Error.NewError("C003", "The property node must have a valid Name attribute.", "Specify a valid Name attribute."));
				continue;
			}
			if (item.Attributes["value"] == null)
			{
				errors.Add(Error.NewError("C003", "The property node must have a valid Value attribute.", "Specify a valid Value attribute."));
				continue;
			}
			string expandedAttributeValue = item.GetExpandedAttributeValue<string>("name");
			if (InstallerProject.Instance.Values[expandedAttributeValue] == null)
			{
				expandedAttributeValue = MacroContainer.Instance.ExpandConstantMacros(expandedAttributeValue);
				string attributeValue = Redbox.Core.XmlNodeExtensions.GetAttributeValue<string>(item, "value");
				attributeValue = MacroContainer.Instance.ExpandConstantMacros(attributeValue);
				InstallerProject.Instance.Values[expandedAttributeValue] = attributeValue;
				m_console.WriteLine("\t{0}={1}", expandedAttributeValue, attributeValue);
			}
		}
	}

	private void ParseIncludedScripts(XmlDocument xmlDocument, string scriptPath, ErrorList errors)
	{
		if (xmlDocument == null || xmlDocument.DocumentElement == null || string.IsNullOrEmpty(scriptPath))
		{
			return;
		}
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("include");
		if (xmlNodeList == null)
		{
			return;
		}
		m_console.WriteLine("\nParsing included scripts:");
		foreach (XmlNode item in xmlNodeList)
		{
			string text = MacroContainer.Instance.ExpandRuntimeMacros(item.GetExpandedAttributeValue<string>("path"));
			if (string.IsNullOrEmpty(text))
			{
				errors.Add(Error.NewError("C001", "The path attribute is required and cannot be empty.", "Correct the path attribute of the include element and recompile."));
				continue;
			}
			if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(scriptPath, text);
			}
			if (!File.Exists(text))
			{
				errors.Add(Error.NewError("C001", "The included script file '" + text + "' does not exist.", "Check the path of the included script file specified on the command line."));
				continue;
			}
			m_console.WriteLine("\t{0}", text);
			XmlDocument xmlDocument2 = new XmlDocument();
			try
			{
				xmlDocument2.Load(text);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("C002", "Unable to parse included script '" + text + "'; see details for more information.", ex.ToString()));
				continue;
			}
			if (xmlDocument2.DocumentElement == null)
			{
				continue;
			}
			xmlDocument.DocumentElement.RemoveChild(item);
			foreach (XmlNode childNode in xmlDocument2.DocumentElement.ChildNodes)
			{
				XmlNode newChild = xmlDocument.ImportNode(childNode, deep: true);
				xmlDocument.DocumentElement.AppendChild(newChild);
			}
		}
	}

	private void AddDeterminedSystemValues(ErrorList errors)
	{
		AddProgramFilesBasedDirectory();
	}

	private static void AddProgramFilesBasedDirectory()
	{
		string value = ((InstallerProject.Instance.MachineArchitecture != Architecture.x64 || InstallerProject.Instance.InstallArchitecture != Architecture.x86) ? "c:\\Program Files" : "c:\\Program Files (x86)");
		InstallerProject.Instance.Values["ProgramFiles"] = value;
	}
}
