using System;
using System.ComponentModel;
using System.IO;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "directory")]
[DisplayName("Directory Entry")]
public class DirectoryEntryDefinition : Definition
{
	private string m_targetPath;

	[ElementProperty(Name = "path", IsRequired = true)]
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

	[DefaultValue(12)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(1)]
	public override int UninstallSortOrder { get; set; }

	public override string ToExecutionMessage()
	{
		string targetPath = GetTargetPath();
		if (Path.IsPathRooted(TargetPath))
		{
			targetPath = TargetPath;
		}
		return $"{GetExecutionAction()} directory {targetPath}";
	}

	public override int GetCountValue()
	{
		return 1;
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			CreateDirectory(errors);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "An error occurred while creating directories; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			RemoveDirectory(errors);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "An error occurred while removing directories; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	internal void CreateDirectory(ErrorList errors)
	{
		try
		{
			string targetPath = GetTargetPath();
			if (Path.IsPathRooted(TargetPath))
			{
				targetPath = TargetPath;
			}
			Directory.CreateDirectory(targetPath);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I001", $"Unable to create directory {GetTargetPath()}.", ex.ToString()));
		}
	}

	internal void RemoveDirectory(ErrorList errors)
	{
		try
		{
			string targetPath = GetTargetPath();
			if (Path.IsPathRooted(TargetPath))
			{
				targetPath = TargetPath;
			}
			Directory.Delete(targetPath, recursive: true);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I001", $"Unable to delete directory {GetTargetPath()}.", ex.ToString()));
		}
	}

	internal static string GetExecutionAction()
	{
		if (InstallerProject.Instance.Mode != InstallMode.Install)
		{
			return "Remove";
		}
		return "Create";
	}

	internal string GetTargetPath()
	{
		return Path.Combine(InstallerProject.Instance.InstallPath, TargetPath);
	}
}
