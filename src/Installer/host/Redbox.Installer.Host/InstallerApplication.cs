using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Redbox.Compression;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Installer.Framework;

namespace Redbox.Installer.Host;

public class InstallerApplication : ApplicationContext
{
	private NamedLock m_instanceLock;

	private CommandLine m_commandLine;

	public Options Options => Singleton<Options>.Instance;

	public CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Options);
			}
			return m_commandLine;
		}
	}

	public static InstallerApplication Instance => Singleton<InstallerApplication>.Instance;

	public bool UserCanceled { get; set; }

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		if (CommandLine.Errors.Count > 0)
		{
			CommandLine.WriteUsage(includeHelp: false);
			while (CommandLine.Errors.Count > 0)
			{
				errorList.Add(Redbox.Installer.Framework.Error.NewError("I005", CommandLine.Errors.Pop(), string.Empty));
			}
			return errorList;
		}
		if (!Options.Silent && !Options.NoSplash)
		{
			Splash.Show();
		}
		string name = Guid.NewGuid().ToString();
		string location = Assembly.GetExecutingAssembly().Location;
		if (!string.IsNullOrEmpty(location))
		{
			name = CreateApplicationId(location);
		}
		m_instanceLock = new NamedLock(name, LockScope.Local);
		if (!m_instanceLock.Exists())
		{
			errorList.Add(Redbox.Installer.Framework.Error.NewError("I002", "Only one instance of setup may be run at a time.", "Another instance of setup is already running.  Finish this setup or cancel to start another setup process."));
			return errorList;
		}
		errorList.AddRange(ParseScript());
		IList<Version> oSRequired = InstallerProject.Instance.OSRequired;
		Version version = new Version(Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);
		if (!oSRequired.Contains(version))
		{
			string arg = Constants.OperatingSystems[oSRequired[0]];
			string arg2 = (Constants.OperatingSystems.Keys.Contains(version) ? Constants.OperatingSystems[version] : Environment.OSVersion.VersionString);
			errorList.Add(Redbox.Installer.Framework.Error.NewError("I020", string.Format("This installer requires {0}{1}.", arg, (oSRequired.Count > 1) ? " or above." : string.Empty), $"This installer is incompatible with {arg2}. Locate the correct installer or run this installer on a different operating system."));
		}
		if (errorList.Count > 0)
		{
			return errorList;
		}
		InstallerProject.Instance.Mode = (Options.Remove ? Redbox.Installer.Framework.InstallMode.Uninstall : Redbox.Installer.Framework.InstallMode.Install);
		InstallerProject.Instance.DoesCurrentUserMeetSecurityRequirements(errorList);
		InstallerProject.Instance.ShouldAllowModify(errorList);
		return errorList;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CommandLine.Dispose();
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	internal void ProcessPendingReboot()
	{
		if (!UserCanceled && (InstallerProject.Instance.RebootMode == RebootMode.Always || (InstallerProject.Instance.RebootMode == RebootMode.IfNecessary && InstallerProject.Instance.RebootRequired)))
		{
			DialogResult dialogResult = DialogResult.OK;
			if (!Options.Silent)
			{
				dialogResult = MessageBox.Show("Setup must restart Windows to complete the installation process.\n\nClick OK to restart now, or Cancel to exit setup and restart Windows later.", $"{InstallerProject.Instance.ProductName} {InstallerProject.Instance.ProductVersion} Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
			}
			if (dialogResult == DialogResult.OK)
			{
				ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.Force, ShutdownReason.MajorApplication | ShutdownReason.FlagPlanned);
			}
		}
	}

	private InstallerApplication()
	{
		Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.ToString());
		};
	}

	private ErrorList ParseScript()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			using (LibraryModule module = new LibraryModule(Assembly.GetExecutingAssembly().Location))
			{
				Options.LoadPropertiesFromResource(module);
				foreach (string key in Options.Properties.Keys)
				{
					InstallerProject.Instance.Values[key] = Options.Properties[key];
				}
				byte[] resource = ResourceTool.GetResource(module, "PREREQ", "SCRIPT");
				if (resource == null)
				{
					throw new FileNotFoundException("Unable to load the installer script from setup.exe; setup terminated.");
				}
				byte[] bytes = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Decompress(resource);
				string xml = Encoding.ASCII.GetString(bytes);
				errorList.AddRange(InstallerProject.ParseXml(xml, checkFiles: false, new Assembly[0], out var _, new NullConsole()));
			}
			if (errorList.Count == 0)
			{
				InstallerProject.Instance.Definitions.LoadFromResources(Assembly.GetExecutingAssembly().Location);
			}
		}
		catch (Exception ex)
		{
			errorList.Add(Redbox.Installer.Framework.Error.NewError("I001", "Unable to initialize setup; the script was not found or unparsable.", ex.ToString()));
		}
		return errorList;
	}

	private static string CreateApplicationId(string path)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		List<byte> list = new List<byte>();
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] array = new byte[1048576];
		int num = ReadChunk(fileStream, array);
		list.AddRange(array);
		if (num == array.Length)
		{
			byte[] array2 = new byte[1048576];
			fileStream.Seek(fileStream.Length - array2.Length, SeekOrigin.Begin);
			ReadChunk(fileStream, array2);
			list.AddRange(array2);
		}
		return Convert.ToBase64String(sHA256Managed.ComputeHash(list.ToArray()));
	}

	private static int ReadChunk(Stream file, byte[] buffer)
	{
		int i;
		int num;
		for (i = 0; i < buffer.Length; i += num)
		{
			num = file.Read(buffer, i, buffer.Length - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}
}
