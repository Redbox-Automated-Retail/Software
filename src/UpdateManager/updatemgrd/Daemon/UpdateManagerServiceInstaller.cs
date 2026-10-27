using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Redbox.UpdateManager.Daemon;

[RunInstaller(true)]
public class UpdateManagerServiceInstaller : Installer
{
	private IContainer components;

	public UpdateManagerServiceInstaller()
	{
		InitializeComponent();
		ServiceInstaller value = new ServiceInstaller
		{
			StartType = ServiceStartMode.Automatic,
			DisplayName = "Redbox Store Manager Service",
			ServiceName = "updatemgrd$default",
			Description = "Manage updates and run scheduled tasks.",
			ServicesDependedOn = new string[1] { "Tcpip" }
		};
		ServiceProcessInstaller value2 = new ServiceProcessInstaller
		{
			Account = ServiceAccount.LocalSystem
		};
		base.Installers.Add(value);
		base.Installers.Add(value2);
	}

	protected override void OnAfterInstall(IDictionary savedState)
	{
		try
		{
			string keyName = string.Format("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\{0}", "updatemgrd$default");
			int num = (int)Registry.GetValue(keyName, "Type", 0);
			num |= 0x100;
			Registry.SetValue(keyName, "Type", num);
			if (Options.Instance.ForceStart)
			{
				using ServiceController serviceController = new ServiceController("updatemgrd$default");
				serviceController.Start();
			}
		}
		catch (Exception ex)
		{
			base.Context.LogMessage("An exception was raised in OnAfterInstall: " + ex);
		}
		base.OnAfterInstall(savedState);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		components = new Container();
	}
}
