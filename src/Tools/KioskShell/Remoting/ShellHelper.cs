using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using Redbox.Core;

namespace Redbox.Shell.Remoting;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public class ShellHelper
{
	public static int StartProcessAsAdministrator(string processName, string args)
	{
		try
		{
			string? directoryName = Path.GetDirectoryName(processName);
			string fileName = Path.GetFileName(processName);
			string? text = (string.IsNullOrEmpty(args) ? null : $"{processName} {args}");
			LogHelper.Instance.Log("starting from path: '{0}' with file: '{1}' in directory: '{2}' with command line: '{3}'", processName, fileName, directoryName, text);
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = processName,
					Arguments = args,
					UseShellExecute = true,
					WindowStyle = ProcessWindowStyle.Maximized,
					Verb = "runas"
				}
			};
			if (process.Start())
			{
				return process.Id;
			}
			LogHelper.Instance.Log("Failed to start Kiosk Engine!");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Function StartProcessAsAdministrator.", e);
		}
		return -1;
	}
}
