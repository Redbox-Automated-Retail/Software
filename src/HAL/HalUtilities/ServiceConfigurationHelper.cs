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
		char c = ':';
		int index = updateConfigurationString.IndexOf('|');
		if (-1 != index)
		{
			c = '|';
		}
		string[] parts = updateConfigurationString.Split(c);
		if (parts.Length < 3)
		{
			Console.WriteLine("Unable to parse option {0}", updateConfigurationString);
			return false;
		}
		string path = runtimeService.RuntimePath("ConfigurationUpdateError.log");
		string text = parts[0];
		string arg = parts[1];
		string text2 = parts[2];
		bool flag = parts.Length == 4;
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
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate($"SETCFG \"{arg}\" \"{text2}\" TYPE={text}", out var job);
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
		if (!flag)
		{
			return true;
		}
		hardwareCommandResult = Service.ExecuteImmediate($"GETCFG \"{arg}\" TYPE={text}", out job);
		string path2 = runtimeService.RuntimePath("ConfigurationUpdateValidateError.log");
		runtimeService.SafeDelete(path2);
		bool result = false;
		StreamWriter validateFile = new StreamWriter(path2);
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
				string text3 = stack.Pop();
				if (!text3.Equals(text2, StringComparison.CurrentCultureIgnoreCase))
				{
					validateFile.WriteLine("The new value {0} doesn't match {1}", text3, text2);
				}
				else
				{
					result = true;
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
		return result;
	}

	internal ServiceConfigurationHelper(HardwareService service)
	{
		Service = service;
	}
}
