using System;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model.Timers;

namespace HALUtilities;

internal sealed class HardwareSurvey
{
	private readonly SurveyModes Mode;

	private const string SurveyFile = "HardwareSurvey.txt";

	internal void Run(HardwareService service)
	{
		TextWriter textWriter = Console.Out;
		if (SurveyModes.File == Mode)
		{
			textWriter = new StreamWriter("HardwareSurvey.txt", append: true);
		}
		RunSurvey(service, textWriter);
		if (SurveyModes.File == Mode)
		{
			textWriter.Dispose();
		}
	}

	internal HardwareSurvey(SurveyModes mode)
	{
		Mode = mode;
	}

	private void RunSurvey(HardwareService service, TextWriter writer)
	{
		using HardwareSurveyExecutor hardwareSurveyExecutor = new HardwareSurveyExecutor(service);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		hardwareSurveyExecutor.Run();
		executionTimer.Stop();
		writer.WriteLine("## Hardware survey on {0} Kiosk = {1} ##", hardwareSurveyExecutor.Timestamp, hardwareSurveyExecutor.Kiosk);
		writer.WriteLine("\tPC Manufacturer         :  {0}", hardwareSurveyExecutor.PcManufacturer);
		writer.WriteLine("\tPC Model                :  {0}", hardwareSurveyExecutor.PcModel);
		writer.WriteLine("\tMemory                  :  {0} GB", hardwareSurveyExecutor.Memory >> 20);
		writer.WriteLine("\tDisk free space         :  {0} GB", hardwareSurveyExecutor.FreeDiskSpace >> 30);
		writer.WriteLine("\tQuick return            :  {0}", hardwareSurveyExecutor.QuickReturn.ToString());
		writer.WriteLine("\tAir exchanger           :  {0}", hardwareSurveyExecutor.AirExchanger.ToString());
		writer.WriteLine("\tAuxRelay board          :  {0}", hardwareSurveyExecutor.HasAuxRelayBoard ? "Yes" : "No");
		writer.WriteLine("\tFraud sensor            :  {0}", hardwareSurveyExecutor.FraudDevice.ToString());
		writer.WriteLine("\tABE device              :  {0}", hardwareSurveyExecutor.ABEDevice);
		writer.WriteLine("\tTouchscreen model       :  {0}", hardwareSurveyExecutor.Touchscreen);
		string arg = hardwareSurveyExecutor.TouchscreenFirmware;
		try
		{
			Version version = new Version(hardwareSurveyExecutor.TouchscreenFirmware);
			bool flag = version.Major == 4 && version.Minor == 30;
			arg = string.Format("{0} ( {1} CURRENT )", hardwareSurveyExecutor.TouchscreenFirmware, flag ? "MOST" : "NOT MOST");
		}
		catch (Exception ex)
		{
			writer.WriteLine("** ERROR** version format '{0}' caught an exception", hardwareSurveyExecutor.TouchscreenFirmware);
			writer.WriteLine(ex.Message);
		}
		writer.WriteLine("\tTouchscreen firmware    :  {0}", arg);
		writer.WriteLine("\tCamera                  :  {0}", hardwareSurveyExecutor.CameraVersion);
		writer.WriteLine("\tMonitor model           :  {0}", hardwareSurveyExecutor.Monitor);
		writer.WriteLine("\tUPS                     :  {0}", hardwareSurveyExecutor.UpsModel);
		writer.WriteLine("\tSerial controller f/w   :  {0}", hardwareSurveyExecutor.SerialControllerVersion);
		writer.WriteLine("## Execution time = {0} ##", executionTimer.Elapsed);
	}
}
