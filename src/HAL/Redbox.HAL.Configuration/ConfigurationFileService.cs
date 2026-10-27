using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class ConfigurationFileService : IConfigurationFileService
{
	private readonly IRuntimeService RuntimeService;

	private readonly List<IConfigurationFile> Configs = new List<IConfigurationFile>();

	public IConfigurationFile Get(SystemConfigurations config)
	{
		if (config == SystemConfigurations.None)
		{
			throw new ArgumentException("Unspecified configuration");
		}
		return Configs.Find((IConfigurationFile each) => each.Type == config);
	}

	public void DoForEach(Predicate<IConfigurationFile> a)
	{
		foreach (IConfigurationFile config in Configs)
		{
			if (!a(config))
			{
				break;
			}
		}
	}

	public void BackupTo(IConfigurationFile f, string targetDir, ErrorList errors)
	{
		string dest = Path.Combine(targetDir, f.FileName);
		ForceCopy(f.FullSourcePath, dest, errors);
	}

	public void Restore(IConfigurationFile f, string fromDir, ErrorList errors)
	{
		string text = Path.Combine(fromDir, f.FileName);
		if (!File.Exists(text))
		{
			errors.Add(Error.NewError("F002", "File doesn't exist", $"Backup file {text} doesn't exist."));
		}
		else
		{
			ForceCopy(text, f.FullSourcePath, errors);
		}
	}

	public bool Backup(IConfigurationFile f, ErrorList errors)
	{
		if (File.Exists(f.FullSourcePath))
		{
			try
			{
				RuntimeService.CreateBackup(f.FullSourcePath, BackupAction.Move);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("F003", "Backup failure", ex.Message));
				return false;
			}
		}
		return !File.Exists(f.FullSourcePath);
	}

	public ConfigurationFileService(IRuntimeService rts)
	{
		RuntimeService = rts;
		Configs.Add(new RedboxConfigurationFile());
		Configs.Add(new LegacySlotData());
		Configs.Add(new LegacySystemData());
	}

	private void ForceCopy(string src, string dest, ErrorList errors)
	{
		try
		{
			File.Copy(src, dest, overwrite: true);
		}
		catch (Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Failed to copy {src} --> {dest}");
			stringBuilder.AppendLine(ex.Message);
			errors.Add(Error.NewError("F001", "Copy failure", stringBuilder.ToString()));
		}
	}
}
