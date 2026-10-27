using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

public class OutdoorElo : GenericTouchScreen
{
	private static readonly string EloInstallDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "Elo TouchSystems\\PCapacitive");

	private static readonly string ControlFile = Path.Combine(EloInstallDirectory, "EloPCap.INI");

	private static readonly string ControlExe = Path.Combine(EloInstallDirectory, "EloPCap_Control.exe");

	private static readonly string DriverExe = Path.Combine(EloInstallDirectory, "EloPCap_Driver.exe");

	public override string Driver => ControlExe;

	public static Process[] GetDriverProcess()
	{
		return GetDriverProcess("EloPCap_Driver");
	}

	public static Process[] GetDriverProcess(string processName)
	{
		try
		{
			return Process.GetProcessesByName(processName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Error getting {processName} process", e);
			return new Process[0];
		}
	}

	protected override void ToggleScreen()
	{
		try
		{
			List<Process> list = new List<Process>();
			list.AddRange(GetDriverProcess());
			list.AddRange(GetDriverProcess("EloPCap_Control"));
			list.ForEach(delegate(Process process)
			{
				process.Kill();
			});
			bool flag = !Enabled();
			WindowsAPI.WritePrivateProfileString("Output", "EnabledBoo1", Convert.ToInt16(flag).ToString(), ControlFile);
			string lpString = "2";
			if (!flag)
			{
				lpString = "0";
			}
			WindowsAPI.WritePrivateProfileString("Output", "PressTypeInt1", lpString, ControlFile);
			Process.Start(DriverExe);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error toggling touchscreen", e);
		}
	}

	protected override bool Enabled()
	{
		try
		{
			if (File.Exists(ControlFile))
			{
				GetDriverProcess("EloPCap_Control").ForEach(delegate(Process process)
				{
					process.Kill();
				});
				StringBuilder stringBuilder = new StringBuilder();
				WindowsAPI.GetPrivateProfileString("Output", "EnabledBoo1", "1", stringBuilder, stringBuilder.Capacity, ControlFile);
				return string.Compare(stringBuilder.ToString(), "1") == 0;
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting elo driver process", e);
			return true;
		}
	}
}
