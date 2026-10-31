using System;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("User")]
[Element(XPath = "user")]
public class UserDefinition : Definition
{
	private string m_name;

	private string m_surname;

	private string m_password;

	private string m_homeDrive;

	private string m_givenName;

	private string m_employeeId;

	private string m_middleName;

	private string m_scriptPath;

	private string m_displayName;

	private string m_description;

	private string m_emailAddress;

	private string m_homeDirectory;

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

	[ElementProperty(Name = "surname")]
	public string Surname
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_surname);
		}
		set
		{
			m_surname = value;
		}
	}

	[ElementProperty(Name = "home-drive")]
	public string HomeDrive
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_homeDrive);
		}
		set
		{
			m_homeDrive = value;
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

	[ElementProperty(Name = "given-name")]
	public string GivenName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_givenName);
		}
		set
		{
			m_givenName = value;
		}
	}

	[ElementProperty(Name = "employee-id")]
	public string EmployeeId
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_employeeId);
		}
		set
		{
			m_employeeId = value;
		}
	}

	[ElementProperty(Name = "middle-name")]
	public string MiddleName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_middleName);
		}
		set
		{
			m_middleName = value;
		}
	}

	[ElementProperty(Name = "script-path")]
	public string ScriptPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_scriptPath);
		}
		set
		{
			m_scriptPath = value;
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

	[ElementProperty(Name = "display-name")]
	public string DisplayName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_displayName);
		}
		set
		{
			m_displayName = value;
		}
	}

	[ElementProperty(Name = "email-address")]
	public string EmailAddress
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_emailAddress);
		}
		set
		{
			m_emailAddress = value;
		}
	}

	[ElementProperty(Name = "home-directory")]
	public string HomeDirectory
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_homeDirectory);
		}
		set
		{
			m_homeDirectory = value;
		}
	}

	[ElementProperty(Name = "expiration-date")]
	public DateTime? ExpirationDate { get; set; }

	[ElementProperty(Name = "pwd-not-required")]
	public bool PasswordNotRequired { get; set; }

	[ElementProperty(Name = "prohibit-pwd-change")]
	public bool ProhibitPasswordChange { get; set; }

	[ElementProperty(Name = "pwd-never-expires")]
	public bool PasswordNeverExpires { get; set; }

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} user account '{1}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			UserPrincipal userPrincipal = new UserPrincipal(new PrincipalContext(ContextType.Machine));
			userPrincipal.Name = Name;
			userPrincipal.DisplayName = DisplayName;
			userPrincipal.Description = Description;
			UserPrincipal userPrincipal2 = userPrincipal;
			if (ExpirationDate.HasValue)
			{
				userPrincipal2.AccountExpirationDate = ExpirationDate.Value;
			}
			try
			{
				userPrincipal2.Surname = Surname;
				userPrincipal2.GivenName = GivenName;
				userPrincipal2.EmployeeId = EmployeeId;
				userPrincipal2.MiddleName = MiddleName;
				userPrincipal2.EmailAddress = EmailAddress;
			}
			catch (InvalidOperationException)
			{
			}
			if (ScriptPath != null)
			{
				userPrincipal2.ScriptPath = ScriptPath;
			}
			if (HomeDirectory != null)
			{
				userPrincipal2.HomeDirectory = HomeDirectory;
			}
			if (HomeDrive != null)
			{
				userPrincipal2.HomeDrive = HomeDrive;
			}
			userPrincipal2.PasswordNotRequired = PasswordNotRequired;
			userPrincipal2.PasswordNeverExpires = PasswordNeverExpires;
			userPrincipal2.UserCannotChangePassword = ProhibitPasswordChange;
			if (Password != null)
			{
				userPrincipal2.SetPassword(Password);
			}
			userPrincipal2.Save();
		}
		catch (Exception ex2)
		{
			errors.Add(Error.NewError("I015", $"Unable to create local user account {Name}; see details for exception.", ex2.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.Name, Name)?.Delete();
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewWarning("I015", $"Unable to remove local user account {Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}
}
