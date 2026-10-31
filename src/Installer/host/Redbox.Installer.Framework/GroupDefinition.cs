using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Group")]
[Element(XPath = "group")]
public class GroupDefinition : Definition
{
	private string m_name;

	private string m_description;

	private List<string> m_members;

	[ElementProperty(Name = "name")]
	public string Name
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_name);
		}
		set
		{
			m_name = value;
		}
	}

	[ElementProperty(Name = "description")]
	public string Description
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_description);
		}
		set
		{
			m_description = value;
		}
	}

	public List<string> Members
	{
		get
		{
			if (m_members == null)
			{
				m_members = new List<string>();
			}
			return m_members;
		}
	}

	[DefaultValue(15)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(8)]
	public override int UninstallSortOrder { get; set; }

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} group '{1}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			GroupPrincipal groupPrincipal = new GroupPrincipal(new PrincipalContext(ContextType.Machine));
			groupPrincipal.Name = Name;
			GroupPrincipal groupPrincipal2 = groupPrincipal;
			foreach (string member in Members)
			{
				Principal principal = Principal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.Name, MacroContainer.Instance.ExpandRuntimeMacros(member));
				if (principal != null)
				{
					groupPrincipal2.Members.Add(principal);
				}
			}
			groupPrincipal2.Save();
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to create local group {Name}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			Principal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.Name, Name)?.Delete();
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to delete local group {Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name != "member")
			{
				errors.Add(Error.NewError("C004", "Only member is a valid under the group node.", "Specify a node named member."));
				continue;
			}
			string expandedAttributeValue = childNode.GetExpandedAttributeValue<string>("name");
			if (string.IsNullOrEmpty(expandedAttributeValue))
			{
				errors.Add(Error.NewError("C004", "The member node requires a valid name attribute.", "Specify a valid name attribute."));
			}
			if (!Members.Contains(expandedAttributeValue))
			{
				Members.Add(expandedAttributeValue);
			}
		}
	}
}
