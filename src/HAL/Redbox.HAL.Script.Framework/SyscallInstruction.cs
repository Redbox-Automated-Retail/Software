using System;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SyscallInstruction : Instruction
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct TokPriv1Luid
	{
		public int Count;

		public long Luid;

		public int Attr;
	}

	[Flags]
	public enum ShutdownFlags : uint
	{
		LogOff = 0u,
		ShutDown = 1u,
		Reboot = 2u,
		Force = 4u,
		PowerOff = 8u,
		ForceIfHung = 0x10u
	}

	[Flags]
	public enum ShutdownReason : uint
	{
		MajorApplication = 0x40000u,
		MajorHardware = 0x10000u,
		MajorLegacyApi = 0x70000u,
		MajorOperatingSystem = 0x20000u,
		MajorOther = 0u,
		MajorPower = 0x60000u,
		MajorSoftware = 0x30000u,
		MajorSystem = 0x50000u,
		MinorBlueScreen = 0xFu,
		MinorCordUnplugged = 0xBu,
		MinorDisk = 7u,
		MinorEnvironment = 0xCu,
		MinorHardwareDriver = 0xDu,
		MinorHotfix = 0x11u,
		MinorHung = 5u,
		MinorInstallation = 2u,
		MinorMaintenance = 1u,
		MinorMMC = 0x19u,
		MinorNetworkConnectivity = 0x14u,
		MinorNetworkCard = 9u,
		MinorOther = 0u,
		MinorOtherDriver = 0xEu,
		MinorPowerSupply = 0xAu,
		MinorProcessor = 8u,
		MinorReconfig = 4u,
		MinorSecurity = MinorHotfix | MinorInstallation,
		MinorSecurityFix = 0x12u,
		MinorSecurityFixUninstall = 0x18u,
		MinorServicePack = 0x10u,
		MinorServicePackUninstall = MinorNetworkConnectivity | MinorInstallation,
		MinorTermSrv = 0x20u,
		MinorUnstable = 6u,
		MinorUpgrade = 3u,
		MinorWMI = MinorHotfix | MinorReconfig,
		FlagUserDefined = 0x40000000u,
		FlagPlanned = 0x80000000u
	}

	private const int SE_PRIVILEGE_ENABLED = 2;

	private const int TOKEN_QUERY = 8;

	private const int TOKEN_ADJUST_PRIVILEGES = 32;

	private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

	public override string Mnemonic => "SYSCALL";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1 || operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected a system call to execute");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		string value = base.Operands[0].Value;
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		if ("GC".Equals(value, StringComparison.CurrentCultureIgnoreCase))
		{
			GC.Collect();
		}
		else
		{
			if (!"REBOOT".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				return;
			}
			if (context.IsImmediate)
			{
				context.PushTop("ERROR");
				return;
			}
			if (!ControllerConfiguration.Instance.RebootKioskDuringQlm)
			{
				LogHelper.Instance.Log("HAL is not configured to reboot the kiosk on Motioncontrol failure.", LogEntryType.Error);
				AddError("E005", "Kiosk reboot is not configured.", result.Errors);
				return;
			}
			if (map.GetValue("CheckRebootTime", def: true))
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, 0);
				TimeSpan timeSpan2 = new TimeSpan(5, 0, 0);
				TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
				if (!(timeOfDay >= timeSpan) || !(timeOfDay < timeSpan2))
				{
					string text = $"Cannot reboot - the current time {DateTime.Now.ToShortTimeString()} is not within 12 am - 5 am.";
					LogHelper.Instance.Log(text, LogEntryType.Info);
					context.AppLog.Write(text);
					AddError("E005", "Kiosk reboot now allowed because of time constraint.", result.Errors);
					return;
				}
			}
			context.DeferredStatusChangeRequested = ExecutionContextStatus.Suspended;
			ExecutionEngine.Instance.PerformContextSwitch(runtimeOnly: false);
			if (!Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.FlagPlanned))
			{
				context.AppLog.Write("Unable to execute a reboot.");
				LogHelper.Instance.Log("Call to reboot kiosk failed.", LogEntryType.Error);
				AddError("E005", "Request to reboot failed.", result.Errors);
			}
		}
	}

	[DllImport("kernel32.dll", ExactSpelling = true)]
	internal static extern IntPtr GetCurrentProcess();

	[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
	internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

	[DllImport("advapi32.dll", SetLastError = true)]
	internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

	[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
	internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	internal static extern bool ExitWindowsEx(ShutdownFlags uFlags, ShutdownReason dwReason);

	public static bool Shutdown(ShutdownFlags flags, ShutdownReason reason)
	{
		if (ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap().GetValue("SimulateRebootFailure", def: false))
		{
			LogHelper.Instance.Log("Simulating reboot failure to service ...", LogEntryType.Error);
			return false;
		}
		IntPtr currentProcess = GetCurrentProcess();
		IntPtr phtok = IntPtr.Zero;
		if (!OpenProcessToken(currentProcess, 40, ref phtok))
		{
			LogHelper.Instance.Log("Unable to adjust token privileges for shutdown.", LogEntryType.Error);
			return false;
		}
		TokPriv1Luid newst = default(TokPriv1Luid);
		newst.Count = 1;
		newst.Luid = 0L;
		newst.Attr = 2;
		bool flag = LookupPrivilegeValue(null, "SeShutdownPrivilege", ref newst.Luid);
		bool flag2 = AdjustTokenPrivileges(phtok, disall: false, ref newst, 0, IntPtr.Zero, IntPtr.Zero);
		if (!flag || !flag2)
		{
			LogHelper.Instance.Log($"Unable to lookup or adjust privileges: lookup = {flag}, adjust = {flag2}", LogEntryType.Error);
			return false;
		}
		return ExitWindowsEx(flags, reason);
	}
}
