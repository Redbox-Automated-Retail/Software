using System;
using System.ComponentModel;
using System.IO;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Shortcut")]
[Element(XPath = "shortcut")]
public class ShortcutDefinition : Definition
{
	private string m_path;

	private string m_iconPath;

	private string m_arguments;

	private string m_targetPath;

	private string m_description;

	private string m_workingDirectory;

	[ElementProperty(Name = "path", IsRequired = true)]
	public string Path
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_path);
		}
		set
		{
			m_path = value;
		}
	}

	[ElementProperty(Name = "icon-index")]
	public int? IconIndex { get; set; }

	[ElementProperty(Name = "icon-path")]
	public string IconPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_iconPath);
		}
		set
		{
			m_iconPath = value;
		}
	}

	[ElementProperty(Name = "arguments")]
	public string Arguments
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_arguments);
		}
		set
		{
			m_arguments = value;
		}
	}

	[ElementProperty(Name = "target-path", IsRequired = true)]
	public string TargetPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_targetPath);
		}
		set
		{
			m_targetPath = value;
		}
	}

	[ElementProperty(UseNodeText = true)]
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

	[ElementProperty(Name = "working-dir")]
	public string WorkingDirectory
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_workingDirectory);
		}
		set
		{
			m_workingDirectory = value;
		}
	}

	[DefaultValue(11)]
	public override int InstallSortOrder { get; set; }

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} shortcut '{1}' launching '{2}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Path, TargetPath);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			using ShellShortcut shellShortcut = new ShellShortcut(Path);
			if (Description != null)
			{
				shellShortcut.Description = Description.TrimStart().TrimEnd();
			}
			if (Arguments != null)
			{
				shellShortcut.Arguments = Arguments;
			}
			shellShortcut.Path = TargetPath;
			if (IconPath != null)
			{
				shellShortcut.IconPath = IconPath;
			}
			if (IconIndex.HasValue)
			{
				shellShortcut.IconIndex = IconIndex.Value;
			}
			if (WorkingDirectory != null)
			{
				shellShortcut.WorkingDirectory = WorkingDirectory;
			}
			shellShortcut.Save();
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to create shortcut due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			File.Delete(Path);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to remove shortcut due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}
}
