using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.IIS;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "app-pool")]
[DisplayName("Application Pool")]
public class ApplicationPoolDefinition : Definition
{
	private string m_name;

	private string m_userName;

	private string m_password;

	private string m_identityType;

	private string m_managedPipelineMode;

	private string m_managedRuntimeVersion;

	private string m_autoStart;

	[ElementProperty(Name = "name", IsRequired = true)]
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

	[ElementProperty(Name = "user-name")]
	public string UserName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_userName);
		}
		set
		{
			m_userName = value;
		}
	}

	[ElementProperty(Name = "password")]
	public string Password
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_password);
		}
		set
		{
			m_password = value;
		}
	}

	[DefaultValue("NetworkService")]
	[ElementProperty(Name = "identity-type")]
	public string IdentityType
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_identityType);
		}
		set
		{
			m_identityType = value;
		}
	}

	[ElementProperty(Name = "managed-pipeline-mode")]
	[DefaultValue("Integrated")]
	public string ManagedPipelineMode
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_managedPipelineMode);
		}
		set
		{
			m_managedPipelineMode = value;
		}
	}

	[DefaultValue("AspNet20")]
	[ElementProperty(Name = "managed-runtime-version")]
	public string ManagedRuntimeVersion
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_managedRuntimeVersion);
		}
		set
		{
			m_managedRuntimeVersion = value;
		}
	}

	[DefaultValue("False")]
	[ElementProperty(Name = "autostart")]
	public string AutoStart
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_autoStart);
		}
		set
		{
			m_autoStart = value;
		}
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} IIS Application Pool '{1}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			List<string> iisObjects = new List<string>();
			string[] existingObjects = (string[])Registry.GetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", new string[0]);
			if (existingObjects != null)
			{
				iisObjects.AddRange(existingObjects);
			}
			IIsApplicationPool isApplicationPool = IIsApplicationPool.GetByName(Name) ?? IIsApplicationPool.CreateAppPool(Name);
			if (isApplicationPool != null)
			{
				isApplicationPool.IdentityType = Enum<AppPoolIdentityType>.ParseIgnoringCase(IdentityType, AppPoolIdentityType.NetworkService);
				if (isApplicationPool.IdentityType == AppPoolIdentityType.SpecificUser)
				{
					isApplicationPool.WAMUserName = UserName;
					isApplicationPool.WAMUserPass = Password;
				}
				isApplicationPool.ManagedPipelineMode = Enum<AppPoolManagedPipelineMode>.ParseIgnoringCase(ManagedPipelineMode, AppPoolManagedPipelineMode.Integrated);
				isApplicationPool.ManagedRuntimeVersion = Enum<AspNetVersion>.ParseIgnoringCase(ManagedRuntimeVersion, AspNetVersion.AspNet20);
				isApplicationPool.AutoStart = Convert.ToBoolean(AutoStart);
				isApplicationPool.Save();
				iisObjects.Add($"pool|{Name}");
			}
			Registry.SetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", iisObjects.ToArray(), RegistryValueKind.MultiString);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to create IIS Application Pool {Name}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			foreach (string text in (string[])Registry.GetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", new string[0]))
			{
				string[] parts = text.Split('|');
				if (!(parts[0] != "pool"))
				{
					IIsApplicationPool.GetByName(parts[1])?.Delete();
				}
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to remove IIS Application Pool {Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}
}
