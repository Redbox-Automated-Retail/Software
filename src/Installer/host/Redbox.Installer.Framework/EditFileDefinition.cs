using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "edit-file")]
[DisplayName("Edit File")]
public class EditFileDefinition : Definition
{
	internal class SearchReplaceInstruction
	{
		private string m_value;

		private string m_pattern;

		private string m_groupName;

		public string Value
		{
			get
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_value);
			}
			set
			{
				m_value = value;
			}
		}

		public string Pattern
		{
			get
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_pattern);
			}
			set
			{
				m_pattern = value;
			}
		}

		public string GroupName
		{
			get
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_groupName);
			}
			set
			{
				m_groupName = value;
			}
		}
	}

	private string m_fileName;

	private string m_backupExtension;

	private IList<SearchReplaceInstruction> m_instructions;

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

	internal IList<SearchReplaceInstruction> Instructions
	{
		get
		{
			if (m_instructions == null)
			{
				m_instructions = new List<SearchReplaceInstruction>();
			}
			return m_instructions;
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
		return $"Edit file '{FileName}'";
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (!File.Exists(FileName))
			{
				errors.Add(Error.NewError("I006", "Unable to edit file '" + FileName + "'; it does not exist.", "Correct the setup script to specify a valid file path."));
				return;
			}
			RegExFileEditor regExFileEditor = new RegExFileEditor(FileName, BackupBeforeEdit, BackupExtension);
			foreach (SearchReplaceInstruction instruction in Instructions)
			{
				regExFileEditor.Replace(instruction.Pattern, instruction.GroupName, instruction.Value);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to edit file {FileName}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name != "replace")
			{
				errors.Add(Error.NewError("C004", "The edit-node only supports replace nodes as children.", "Specify valid replace nodes under an edit-file node."));
				continue;
			}
			if (childNode.Attributes["group-name"] == null)
			{
				errors.Add(Error.NewError("C004", "The replace node requires a valid group-name attribute.", "Specify a valid group-name attribute."));
				continue;
			}
			XmlNode xmlNode2 = childNode.SelectSingleNode("pattern");
			if (xmlNode2 == null)
			{
				errors.Add(Error.NewError("C004", "The replace node requires a valid pattern child node.", "Specify a valid pattern child node."));
				continue;
			}
			XmlNode xmlNode3 = childNode.SelectSingleNode("value");
			if (xmlNode3 == null)
			{
				errors.Add(Error.NewError("C004", "The replace node requires a valid value child node.", "Specify a valid value child node."));
				continue;
			}
			Instructions.Add(new SearchReplaceInstruction
			{
				Pattern = xmlNode2.GetExpandedNodeValue("pattern").Trim(),
				GroupName = childNode.GetExpandedAttributeValue<string>("group-name"),
				Value = xmlNode3.GetExpandedNodeValue("value").Trim()
			});
		}
	}
}
