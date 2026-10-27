using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.MSHALTester.Properties;

namespace Redbox.HAL.MSHALTester;

internal static class TunerLaunchService
{
	private static readonly byte[] ShutdownCommand;

	private static readonly byte[] StartupCommand;

	internal static void LaunchTunerAndWait(HardwareService service, ButtonAspectsManager manager)
	{
		service.ExecuteImmediateProgram(ShutdownCommand, out var job);
		ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(Settings.Default.TunerStartPause);
		string tunerApplication = Settings.Default.TunerApplication;
		if (File.Exists(tunerApplication))
		{
			Process.Start(tunerApplication, string.Empty).WaitForExit();
		}
		else
		{
			using CameraPreviewForm cameraPreviewForm = new CameraPreviewForm(service, manager, Settings.Default.ImageDirectory);
			cameraPreviewForm.ShowDialog();
		}
		service.ExecuteImmediateProgram(StartupCommand, out job);
	}

	static TunerLaunchService()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
		stringBuilder.Append("RINGLIGHT ON" + Environment.NewLine);
		ShutdownCommand = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		stringBuilder = new StringBuilder();
		stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
		stringBuilder.Append("CAMERA START" + Environment.NewLine);
		StartupCommand = Encoding.ASCII.GetBytes(stringBuilder.ToString());
	}
}
