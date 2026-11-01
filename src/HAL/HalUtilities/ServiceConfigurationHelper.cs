using System;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class ServiceConfigurationHelper : IDisposable
{
	private readonly HardwareService Service;

	public void Dispose()
	{
	}

	internal void BackupConfiguration(string kioskID)
	{
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		using StreamWriter streamWriter = new StreamWriter(File.Open(runtimeService.RuntimePath("backup-log.txt"), FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = runtimeService.RuntimePath(kioskID);
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch
			{
				streamWriter.WriteLine("Unable to create backup directory.");
				return;
			}
		}
		try
		{
			File.Copy("c:\\gamp\\SystemData.dat", Path.Combine(text, "SystemData.dat"), overwrite: true);
			File.Copy("c:\\gamp\\SlotData.dat", Path.Combine(text, "SlotData.dat"), overwrite: true);
			File.Copy("c:\\Program Files\\Redbox\\HALService\\bin\\HAL.xml", Path.Combine(text, "HAL.xml"), overwrite: true);
		}
		catch
		{
			streamWriter.WriteLine("Error backing up file.");
		}
	}

	internal bool UpdateOption(string updateConfigurationString)
	{
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		char separator = ':';
		int index = updateConfigurationString.IndexOf('|');
		if (-1 != index)
		{
			separator = '|';
		}
		string[] parts = updateConfigurationString.Split(separator);
		if (parts.Length < 3)
		{
			Console.WriteLine("Unable to parse option {0}", updateConfigurationString);
			return false;
		}
		string path = runtimeService.RuntimePath("ConfigurationUpdateError.log");
		string configType = parts[0];
		string option = parts[1];
		string newValue = parts[2];
		bool validate = parts.Length == 4;
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
		}
		if (Service == null)
		{
			using (StreamWriter streamWriter = new StreamWriter(path))
			{
				streamWriter.WriteLine("Unable to talk to HAL.");
			}
			return false;
		}
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate($"SETCFG \"{option}\" \"{newValue}\" TYPE={configType}", out var job);
		if (!hardwareCommandResult.Success)
		{
			StreamWriter log = new StreamWriter(path);
			try
			{
				log.WriteLine("The configuration update failed.");
				hardwareCommandResult.Errors.ForEach(delegate(Error err)
				{
					log.WriteLine(err.ToString());
				});
			}
			finally
			{
				if (log != null)
				{
					((IDisposable)log).Dispose();
				}
			}
			return false;
		}
		if (!validate)
		{
			return true;
		}
		hardwareCommandResult = Service.ExecuteImmediate($"GETCFG \"{option}\" TYPE={configType}", out job);
		string validatePath = runtimeService.RuntimePath("ConfigurationUpdateValidateError.log");
		runtimeService.SafeDelete(validatePath);
		bool validated = false;
		StreamWriter validateFile = new StreamWriter(validatePath);
		try
		{
			if (!hardwareCommandResult.Success)
			{
				validateFile.WriteLine("The configuration update failed.");
				hardwareCommandResult.Errors.ForEach(delegate(Error err)
				{
					validateFile.WriteLine(err.ToString());
				});
				return false;
			}
			if (!job.GetStack(out var stack).Success)
			{
				validateFile.WriteLine("Couldn't validate: unable to get stack.");
			}
			else
			{
				string currentValue = stack.Pop();
				if (!currentValue.Equals(newValue, StringComparison.CurrentCultureIgnoreCase))
				{
					validateFile.WriteLine("The new value {0} doesn't match {1}", currentValue, newValue);
				}
				else
				{
					validated = true;
				}
			}
		}
		finally
		{
			if (validateFile != null)
			{
				((IDisposable)validateFile).Dispose();
			}
		}
		return validated;
	}

	internal ServiceConfigurationHelper(HardwareService service)
	{
		Service = service;
	}
}
