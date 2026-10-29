using System;
using System.Collections.Generic;

namespace Redbox.Installer.Framework;

public static class Constants
{
	public static class Pages
	{
		public const string Finish = "finish";

		public const string Progress = "install-progress";

		public const string Introduction = "introduction";
	}

	public static class Nodes
	{
		public const string Log = "log";

		public const string User = "user";

		public const string Group = "group";

		public const string Queue = "queue";

		public const string Files = "file";

		public const string Pages = "page";

		public const string Script = "script";

		public const string Member = "member";

		public const string XmlEdit = "edit";

		public const string Include = "include";

		public const string Website = "website";

		public const string Replace = "replace";

		public const string Shortcut = "shortcut";

		public const string Triggers = "triggers/*";

		public const string EventLog = "event-log";

		public const string EditFile = "edit-file";

		public const string Bindings = "bindings/*";

		public const string Property = "property";

		public const string Namespace = "namespace";

		public const string Permission = "permission";

		public const string IdleTrigger = "idle-trigger";

		public const string Directories = "directory";

		public const string EditXmlFile = "edit-xml-file";

		public const string DailyTrigger = "daily-trigger";

		public const string LogonTrigger = "logon-trigger";

		public const string ScheduleTask = "schedule-task";

		public const string ServerBinding = "server-binding";

		public const string WeeklyTrigger = "weekly-trigger";

		public const string PageTemplates = "page-template";

		public const string RunOnceTrigger = "run-once-trigger";

		public const string MonthlyTrigger = "monthly-trigger";

		public const string ScriptMappings = "script-maps/*";

		public const string AddRegistryKey = "add";

		public const string EditRegistryKey = "edit";

		public const string RegistryEntries = "registry-entry";

		public const string ApplicationPool = "app-pool";

		public const string RemoveRegistryKey = "remove";

		public const string ProcessesExecute = "execute-process";

		public const string EnvironmentVariable = "env-variable";

		public const string FirewallExemptions = "firewall-exemption";

		public const string SystemStartupTrigger = "system-startup-trigger";

		public const string MonthlyDayOfWeekTrigger = "monthly-dow-trigger";

		public const string ProhibitedProcessesProcess = "prohibit-process";
	}

	public static class Attributes
	{
		public const string ID = "id";

		public const string Uri = "uri";

		public const string Key = "key";

		public const string Name = "name";

		public const string Path = "path";

		public const string Text = "text";

		public const string Type = "type";

		public const string Icon = "icon";

		public const string Hour = "hour";

		public const string Kill = "kill";

		public const string Copy = "copy";

		public const string Mode = "mode";

		public const string Port = "port";

		public const string Order = "order";

		public const string Value = "value";

		public const string Title = "title";

		public const string Flags = "flags";

		public const string XPath = "xpath";

		public const string Verbs = "verbs";

		public const string Phase = "phase";

		public const string Action = "action";

		public const string Prefix = "prefix";

		public const string Rights = "rights";

		public const string Backup = "backup";

		public const string Reboot = "reboot";

		public const string Target = "target";

		public const string Editor = "editor";

		public const string Message = "message";

		public const string Surname = "surname";

		public const string Pattern = "pattern";

		public const string Creator = "creator";

		public const string Comment = "comment";

		public const string Minutes = "minutes";

		public const string EndDate = "end-date";

		public const string TcpPort = "tcp-port";

		public const string AppPool = "app-pool";

		public const string Options = "options";

		public const string External = "external";

		public const string Template = "template";

		public const string Category = "category";

		public const string Disabled = "disabled";

		public const string Priority = "priority";

		public const string FileName = "filename";

		public const string Subtitle = "subtitle";

		public const string ReadOnly = "read-only";

		public const string UseShell = "use-shell";

		public const string IconPath = "icon-path";

		public const string Interval = "interval";

		public const string AuthNTLM = "auth-ntlm";

		public const string UserName = "user-name";

		public const string Password = "password";

		public const string AutoStart = "autostart";

		public const string Uninstall = "uninstall";

		public const string Extension = "extension";

		public const string HomeDrive = "home-drive";

		public const string GivenName = "given-name";

		public const string IpAddress = "ip-address";

		public const string Arguments = "arguments";

		public const string BeginDate = "begin-date";

		public const string IconIndex = "icon-index";

		public const string GroupName = "group-name";

		public const string Architecture = "architecture";

		public const string IsRequired = "is-required";

		public const string LaunchOnly = "launch-only";

		public const string EmployeeId = "employee-id";

		public const string HostHeader = "host-header";

		public const string DaysOfYear = "days-of-year";

		public const string DaysOfWeek = "days-of-week";

		public const string MaxRuntime = "max-runtime";

		public const string SourcePath = "source-path";

		public const string TargetPath = "target-path";

		public const string TargetWeek = "target-week";

		public const string AccessRead = "access-read";

		public const string MiddleName = "middle-name";

		public const string ScriptPath = "script-path";

		public const string AllowCancel = "allow-cancel";

		public const string RaiseErrors = "raise-errors";

		public const string WaitForExit = "wait-for-exit";

		public const string DisplayName = "display-name";

		public const string Description = "description";

		public const string AllowModify = "allow-modify";

		public const string Compression = "compression";

		public const string ProceedText = "proceed-text";

		public const string AccessWrite = "access-write";

		public const string InstallOrder = "install-order";

		public const string EmailAddress = "email-address";

		public const string IdentityType = "identity-type";

		public const string MonthsOfYear = "months-of-year";

		public const string ResourceName = "resource-name";

		public const string DefaultValue = "default-value";

		public const string IfExpression = "if";

		public const string Transactional = "transactional";

		public const string AspNetVersion = "asp-net-version";

		public const string ParentWebsite = "parent-website";

		public const string SecurityLevel = "security-level";

		public const string AccessExecute = "access-execute";

		public const string AuthAnonymous = "auth-anonymous";

		public const string HomeDirectory = "home-directory";

		public const string AllowMoveNext = "allow-move-next";

		public const string UninstallOrder = "uninstall-order";

		public const string ExpirationDate = "expiration-date";

		public const string SuppressWindow = "suppress-window";

		public const string ExecutablePath = "executable-path";

		public const string ReportErrorOnExit = "report-error";

		public const string BackupExtension = "backup-extension";

		public const string ApplicationName = "application-name";

		public const string AppFriendlyName = "app-friendly-name";

		public const string DurationMinutes = "duration-minutes";

		public const string EditAllMatches = "edit-all-matches";

		public const string IntervalMinutes = "interval-minutes";

		public const string DestinationPath = "destination-path";

		public const string DefaultDocuments = "default-documents";

		public const string WorkingDirectory = "working-dir";

		public const string AllowMovePrevious = "allow-move-previous";

		public const string EnableDirBrowsing = "enable-dir-browsing";

		public const string KillAtDurationEnd = "kill-at-duration-end";

		public const string ExplodeTargetPath = "explode-target-path";

		public const string DeleteAfterExplode = "delete-after-explode";

		public const string ManagedPipelineMode = "managed-pipeline-mode";

		public const string PasswordNotRequired = "pwd-not-required";

		public const string PasswordNeverExpires = "pwd-never-expires";

		public const string ExplodeArchiveOnCopy = "explode-on-copy";

		public const string SelfRegisterComServer = "self-register";

		public const string ManagedRuntimeVersion = "managed-runtime-version";

		public const string ProhibitPasswordChange = "prohibit-pwd-change";

		public const string ExecuteAssemblyInstaller = "execute-assembly-installer";
	}

	public static class ResourceGroups
	{
		public const string Prerequisites = "PREREQ";

		public const string Files = "FILE";
	}

	public static class EmbeddedFiles
	{
		public const string Script = "SCRIPT";

		public const string PropertyOverrides = "PROPS";
	}

	public static class RegistryHives
	{
		public const string HKEY_LOCAL_MACHINE = "hkey_local_machine";

		public const string HKEY_CLASSES_ROOT = "hkey_classes_root";

		public const string HKEY_CURRENT_USER = "hkey_current_user";

		public const string HKEY_CURRENT_CONFIG = "hkey_current_config";

		public const string HKEY_USERS = "hkey_users";
	}

	public static class Properties
	{
		public static class Authentication
		{
			public const string UserName = "authentication:user-name";

			public const string Password = "authentication:password";
		}

		public static class AuthenticatedConnection
		{
			public const string ServerName = "authenticatedconnection:server-name";

			public const string UserName = "authenticatedconnection:user-name";

			public const string Password = "authenticatedconnection:password";
		}

		public static class EnvironmentVariables
		{
			public const string WinDir = "windir";

			public const string System32 = "system32";

			public const string ComSpec = "comspec";

			public const string Path = "path";

			public const string TempDir = "temp";
		}

		public static class SelectFolder
		{
			public const string Path = "select-folder:path";
		}

		public static class WebConfiguration
		{
			public const string SiteType = "webconfig:site-type";

			public const string WebsiteName = "webconfig:website-name";

			public const string NameToCreate = "webconfig:name-to-create";

			public const string ParentWebsite = "webconfig:parent-website";

			public const string WebsiteAddress = "webconfig:website-address";

			public const string WebsiteTcpPort = "webconfig:website-tcp-port";

			public const string WebsiteHostHeader = "webconfig:website-host-header";

			public const string WebsiteApplicationPool = "webconfig:app-pool";

			public const string WebsiteVirtualDirectory = "webconfig:vdir";
		}

		public static class BindInterface
		{
			public const string InterfaceName = "bindinterface:interface-name";

			public const string IpAddress = "bindinterface:ip-address";

			public const string TcpPort = "bindinterface:tcp-port";
		}

		public const string ID = "id";

		public const string Name = "name";

		public const string Icon = "icon";

		public const string Width = "width";

		public const string Height = "height";

		public const string Uninstall = "uninstall";

		public const string RebootMode = "rebootmode";

		public const string InstallMode = "installmode";

		public const string InstallPath = "installpath";

		public const string MarginImage = "marginimage";

		public const string BannerImage = "bannerimage";

		public const string ProductName = "productname";

		public const string AllowModify = "allowmodify";

		public const string ProgramFiles = "ProgramFiles";

		public const string LaunchOption = "LaunchOption";

		public const string SecurityLevel = "securitylevel";

		public const string ProductVersion = "productversion";

		public const string LaunchCheckState = "LaunchCheckState";

		public const string AgreementAccepted = "agreementaccepted";

		public const string RebootRequiredFlag = "rebootrequiredflag";

		public const string InstallArchitecture = "InstallArchitecture";

		public const string MachineArchitecture = "MachineArchitecture";

		public const string OSRequired = "OSRequired";

		public const string DotNetFramework10Path = "dotnetfx10-path";

		public const string DotNetFramework11Path = "dotnetfx11-path";

		public const string DotNetFramework20Path = "dotnetfx20-path";

		public const string DotNetFramework30Path = "dotnetfx30-path";

		public const string DotNetFramework35Path = "dotnetfx35-path";

		public const string DotNetFramework40Path = "dotnetfx40-path";
	}

	public static readonly IDictionary<Version, string> OperatingSystems = new Dictionary<Version, string>
	{
		{
			new Version(5, 1),
			"Windows XP"
		},
		{
			new Version(5, 2),
			"Windows Server 2003"
		},
		{
			new Version(6, 0),
			"Windows Server 2008"
		},
		{
			new Version(6, 1),
			"Windows 7"
		},
		{
			new Version(6, 2),
			"Windows 8"
		},
		{
			new Version(6, 3),
			"Windows 8.1"
		}
	};
}
