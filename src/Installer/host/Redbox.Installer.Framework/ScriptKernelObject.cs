using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.Compression;
using Redbox.Core;
using Redbox.IIS;
using Redbox.Installer.Client;
using Redbox.Macros;
using Redbox.ScriptingHost.Framework;

namespace Redbox.Installer.Framework;

public class ScriptKernelObject : KernelObject
{
	public ReadOnlyCollection<Definition> prohibitedProcesses => InstallerProject.Instance.Definitions.GetProhibitedProcessesForNotify().AsReadOnly();

	public string[] networkInterfaces
	{
		get
		{
			List<string> list = new List<string>();
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				IPAddress iPAddress = null;
				foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						iPAddress = unicastAddress.Address;
						break;
					}
				}
				if (iPAddress != null)
				{
					list.Add($"{networkInterface.Name} ({iPAddress})");
				}
			}
			return list.ToArray();
		}
	}

	public string[] timezones
	{
		get
		{
			List<string> list = new List<string>();
			foreach (TimeZoneInfo systemTimeZone in TimeZoneInfo.GetSystemTimeZones())
			{
				list.Add($"{systemTimeZone.DisplayName} ({systemTimeZone.BaseUtcOffset})");
			}
			return list.ToArray();
		}
	}

	public IList<IIsApplicationPool> appPools => IIsApplicationPool.GetAll();

	public IList<IIsWebsite> webSites => IIsWebsite.GetAll();

	public IPAddress[] hostAddresses => Dns.GetHostAddresses(Dns.GetHostName());

	public StringDictionary macros => MacroContainer.Instance.Values;

	public InstallerProject project => InstallerProject.Instance;

	public string productVersion => InstallerProject.Instance.ProductVersion;

	public string productName => InstallerProject.Instance.ProductName;

	public string setupName => $"{InstallerProject.Instance.ProductName} {InstallerProject.Instance.ProductVersion} Setup";

	public void log(string message)
	{
		InstallerProject.Instance.ActiveConsole.WriteLine(message);
	}

	public void alert(string message)
	{
		MessageBox.Show(message, setupName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	public void warning(string message)
	{
		MessageBox.Show(message, setupName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	public bool confirm(string message)
	{
		DialogResult dialogResult = MessageBox.Show(message, setupName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		return dialogResult == DialogResult.Yes;
	}

	public string expandConstantMacros(string value)
	{
		return MacroContainer.Instance.ExpandConstantMacros(value);
	}

	public string expandRuntimeMacros(string value)
	{
		return MacroContainer.Instance.ExpandRuntimeMacros(value);
	}

	public string extractCodeFromBrackets(string value, string prefix, string postfix)
	{
		return GlobalStringExtensions.ExtractCodeFromBrackets(value, prefix, postfix);
	}

	public void unzip(string sourceFile, string targetPath)
	{
		ZipHelper.ExtractAllFiles(sourceFile, targetPath);
	}

	public void SetLocalMachineRegistry(string path, string name, object value)
	{
		RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(path);
		registryKey.SetValue(name, value);
		registryKey.Close();
	}

	public void launchprocessblocking(string name, string arguments)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(name, arguments);
			using Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}
		catch (Exception value)
		{
			InstallerProject.Instance.ActiveConsole.WriteLine(value);
		}
	}

	public void launchprocess(string name, string arguments)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(name, arguments);
			using Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
		}
		catch (Exception value)
		{
			InstallerProject.Instance.ActiveConsole.WriteLine(value);
		}
	}

	public string encrypt(string input)
	{
		return PasswordHelper.Encrypt(input);
	}

	public bool validateUser(string username, string password)
	{
		string[] parts = username.Split('\\');
		ContextType contextType;
		string name;
		if (parts.Length == 1)
		{
			contextType = ContextType.Machine;
			name = Environment.MachineName;
		}
		else
		{
			if (parts.Length != 2)
			{
				return false;
			}
			contextType = ContextType.Domain;
			name = parts[0];
			username = parts[1];
		}
		using PrincipalContext principalContext = new PrincipalContext(contextType, name);
		return principalContext.ValidateCredentials(username, password);
	}
}
