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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		using StreamWriter streamWriter = new StreamWriter(File.Open(service.RuntimePath("backup-log.txt"), FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = service.RuntimePath(kioskID);
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		char c = ':';
		int num = updateConfigurationString.IndexOf('|');
		if (-1 != num)
		{
			c = '|';
		}
		string[] array = updateConfigurationString.Split(c);
		if (array.Length < 3)
		{
			Console.WriteLine("Unable to parse option {0}", updateConfigurationString);
			return false;
		}
		string path = service.RuntimePath("ConfigurationUpdateError.log");
		string text = array[0];
		string arg = array[1];
		string text2 = array[2];
		bool flag = array.Length == 4;
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
		string path2 = service.RuntimePath("ConfigurationUpdateValidateError.log");
		service.SafeDelete(path2);
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
