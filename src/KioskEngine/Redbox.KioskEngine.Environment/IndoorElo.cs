using System;
using System.IO;
using System.Runtime.InteropServices;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class IndoorElo : GenericTouchScreen
{
	private static readonly string ControlApplication = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "EloTouchSystems\\elotouch.cpl");

	private static readonly string AlternateControlApplication = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "Elo TouchSystems\\Elo Universal Driver 470\\elotouch.cpl");

	public override string Driver
	{
		get
		{
			if (File.Exists(ControlApplication))
			{
				return ControlApplication;
			}
			if (!File.Exists(AlternateControlApplication))
			{
				return string.Empty;
			}
			return AlternateControlApplication;
		}
	}

	public static bool Connected()
	{
		try
		{
			return EloIF_EnumTouchScreenEx(new ulong[32]) > 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error checking for connected indoor ELO", e);
			return false;
		}
	}

	protected override void ToggleScreen()
	{
		EloIF_DisableTouchEx(base.State == TouchScreenState.Enabled, -1);
	}

	protected override bool Enabled()
	{
		bool bflag = false;
		EloIF_GetTouchState(ref bflag, 0u);
		return !bflag;
	}

	[DllImport("EloIntf.dll")]
	private static extern int EloIF_EnumTouchScreenEx(ulong[] array);

	[DllImport("EloIntf.dll")]
	private static extern void EloIF_GetTouchState(ref bool bflag, uint number);

	[DllImport("EloIntf.dll")]
	private static extern bool EloIF_DisableTouchEx(bool status, int monitor_id);
}
