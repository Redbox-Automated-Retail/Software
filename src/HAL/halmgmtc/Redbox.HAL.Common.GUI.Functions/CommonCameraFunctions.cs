using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CommonCameraFunctions
{
	public static bool IRCameraConfigured(HardwareService service)
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(service, "CAMERA IRCAMERA");
		if (hardwareJob == null)
		{
			return false;
		}
		return bool.Parse(hardwareJob.GetTopOfStack());
	}

	public static void LaunchAndWaitForTuner(HardwareService service)
	{
		if (service != null)
		{
			bool flag = IRCameraConfigured(service);
			string text = "c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe";
			if (File.Exists(text))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
				stringBuilder.Append("RINGLIGHT ON" + Environment.NewLine);
				service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var job);
				PerfFunctions.SpinWait(5000);
				Process.Start(text, flag ? "-secure" : string.Empty).WaitForExit();
				stringBuilder = new StringBuilder();
				stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
				stringBuilder.Append("CAMERA START" + Environment.NewLine);
				service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out job);
			}
		}
	}
}
