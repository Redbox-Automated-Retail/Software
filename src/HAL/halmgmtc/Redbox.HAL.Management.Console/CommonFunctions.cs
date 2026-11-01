using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;
using Range = Redbox.HAL.Core.Range;

namespace Redbox.HAL.Management.Console;

internal static class CommonFunctions
{
	private static string m_servicePath;

	private static string ServicePath
	{
		get
		{
			if (string.IsNullOrEmpty(m_servicePath))
			{
				RegistryKey registryKey = null;
				try
				{
					registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\HalSvc$default");
					if (registryKey != null)
					{
						string text = (string)registryKey.GetValue("ImagePath");
						if (!string.IsNullOrEmpty(text))
						{
							if (text.StartsWith("\""))
							{
								text = text.Remove(0, 1);
							}
							if (text.EndsWith("\""))
							{
								text = text.Remove(text.Length - 1, 1);
							}
							m_servicePath = Path.GetDirectoryName(text);
						}
					}
				}
				catch (Exception ex)
				{
					OutputWindow.Instance.Append("Unable to locate HAL service path " + ex.Message);
					m_servicePath = null;
				}
			}
			return m_servicePath;
		}
	}

	internal static Location CurrentLocation()
	{
		TouchScreenAccess.WriteInstructionToOutput("LOC");
		HardwareJob hardwareJob = ExecuteInstruction("LOC");
		hardwareJob.WaitForCompletion();
		hardwareJob.GetStack(out var stack);
		if (stack == null || stack.Count < 2)
		{
			return null;
		}
		if (!int.TryParse(stack.Pop(), out var deck))
		{
			LogHelper.Instance.Log("Currently not at a deck, slot location", LogEntryType.Error);
			return null;
		}
		if (!int.TryParse(stack.Pop(), out var slot))
		{
			LogHelper.Instance.Log("Currently not at a deck, slot location", LogEntryType.Error);
			return null;
		}
		return new Location
		{
			Deck = deck,
			Slot = slot
		};
	}

	internal static void OnReadPickerSensors(object sender, EventArgs e)
	{
		using (TouchScreenAccess.Instance.Manager.MakeAspect(sender))
		{
			int firstSensor = 1;
			int lastSensor = 6;
			string readInstruction = $"SENSOR READ PICKER-SENSOR={firstSensor}..{lastSensor}";
			OutputWindow.Instance.Append(readInstruction);
			HardwareJob hardwareJob = ExecuteInstruction(readInstruction);
			if (hardwareJob == null || !hardwareJob.GetStack(out var stack).Success)
			{
				return;
			}
			SensorView.Instance.ResetSensors();
			if (stack.Count != 0)
			{
				stack.Pop();
				for (int sensor = lastSensor; sensor >= firstSensor; sensor--)
				{
					string reading = stack.Pop();
					SensorView.Instance.Sensors[sensor - 1] = reading.Contains("BLOCKED");
				}
				string offInstruction = "SENSOR PICKER-OFF";
				OutputWindow.Instance.Append(offInstruction);
				ExecuteInstruction(offInstruction);
			}
		}
	}

	internal static void OnLaunchCameraProperties(object sender, EventArgs e)
	{
		using (TouchScreenAccess.Instance.Manager.MakeAspect(sender))
		{
			if (string.IsNullOrEmpty(ServicePath))
			{
				OutputWindow.Instance.Append("Unable to locate service path; cannot launch amcap.");
			}
			else
			{
				CommonCameraFunctions.LaunchAndWaitForTuner(ProfileManager.Instance.Service);
			}
		}
	}

	internal static bool SyncSlot()
	{
		try
		{
			Location location = CurrentLocation();
			if (location == null)
			{
				return false;
			}
			if (ProfileManager.Instance.Service.HardSync(new SyncRange(location.Deck, location.Deck, new Range(location.Slot, location.Slot)), new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out var job).Success)
			{
				job.Resume();
				job.WaitForCompletion();
				ProfileManager.Instance.Service.GetJob(job.ID, out job);
				return job.Status == HardwareJobStatus.Completed || job.Status == HardwareJobStatus.Stopped;
			}
			LogHelper.Instance.Log("Sync slot failed.", LogEntryType.Error);
			return false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in SyncSlot.", e);
			return false;
		}
	}

	internal static HardwareJob ExecuteInstruction(string instruction)
	{
		ProcessCommandResult(ProfileManager.Instance.Service.ExecuteImmediate(instruction, Settings.Default.ImmediateInstructionTimeout, out var job));
		return job;
	}

	internal static void ScheduleJob(string jobName, string label, HardwareJobSchedule schedule, bool resumeNow)
	{
		HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.ScheduleJob(jobName, label, enableDebugging: false, schedule, out var job);
		ProcessCommandResult(hardwareCommandResult);
		if (hardwareCommandResult.Success & resumeNow)
		{
			job.Resume();
		}
	}

	internal static void ProcessCommandResult(HardwareCommandResult result)
	{
		LogHelper.Instance.Log(result.CommandText);
		OutputWindow.Instance.Append(">>> Attempt to execute: " + result.CommandText);
		if (!result.Success)
		{
			ProcessCommandResultError(result);
			OutputWindow.Instance.Append("Unable to schedule job");
			LogHelper.Instance.Log("ERROR: Failed to execute command", LogEntryType.Error);
			{
				foreach (string commandMessage in result.CommandMessages)
				{
					OutputWindow.Instance.Append(commandMessage);
					LogHelper.Instance.Log(commandMessage, LogEntryType.Error);
				}
				return;
			}
		}
		if (!ErrorListView.Instance.KeepOpenOnSuccessfulInstruction)
		{
			ErrorListView.Instance.Clear();
			ListViewTabControl.Instance.Remove(ListViewNames.Errors);
		}
	}

	internal static void ProcessCommandResultError(HardwareCommandResult result)
	{
		if (result.Errors.ContainsCode("J001"))
		{
			ProfileManager.Instance.Disconnect();
			MessageBox.Show($"Unable to connect to the designated HAL Service at {Settings.Default.DefaultConnectionURL}.\n\nEnsure the HAL Service is installed and running, bound to the desired network interface and port.", "Connect to Service", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		EnvironmentHelper.DisplayErrors(result.Errors, clearPrevious: false, keepOpenOnSuccessfulInstruction: false);
	}
}
