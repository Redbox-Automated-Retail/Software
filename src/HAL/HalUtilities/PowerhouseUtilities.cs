using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class PowerhouseUtilities : IDisposable
{
	private readonly string BackupDirectory = string.Empty;

	private readonly IRuntimeService RuntimeService;

	private readonly string GampFolder = "c:\\Gamp";

	private readonly string HALInstallFolder = "c:\\Program Files\\Redbox\\HALService\\bin";

	private readonly string HALConfig = "hal.xml";

	private readonly string[] GampFiles = new string[2] { "systemdata.dat", "slotdata.dat" };

	internal bool BackupOk { get; private set; }

	public void Dispose()
	{
	}

	internal static void Run(PowerhouseOperations op, IRuntimeService rts)
	{
		if (op == PowerhouseOperations.None)
		{
			return;
		}
		using PowerhouseUtilities powerhouseUtilities = new PowerhouseUtilities(rts);
		if (PowerhouseOperations.Restore == op)
		{
			if (powerhouseUtilities.BackupOk)
			{
				powerhouseUtilities.Restore();
			}
		}
		else if (PowerhouseOperations.Backup == op && powerhouseUtilities.BackupOk)
		{
			powerhouseUtilities.Backup();
		}
		Console.WriteLine("Press any key to exit.");
		Console.ReadLine();
	}

	internal void Backup()
	{
		ForFiles(GampFolder, BackupDirectory);
		string text = Path.Combine(HALInstallFolder, HALConfig);
		if (File.Exists(text))
		{
			DoCopy(text, Path.Combine(BackupDirectory, HALConfig));
		}
		string kioskId = RuntimeService.KioskId;
		if ("UNKNOWN" != kioskId)
		{
			Console.WriteLine("Found kiosk id {0}", kioskId);
			using StreamWriter streamWriter = new StreamWriter(Path.Combine(BackupDirectory, "KID.txt"));
			streamWriter.WriteLine("Kiosk id = {0}", kioskId);
		}
	}

	internal void Restore()
	{
		if (CreateDir(GampFolder))
		{
			ForFiles(BackupDirectory, GampFolder);
		}
		string text = Path.Combine(BackupDirectory, HALConfig);
		if (File.Exists(text) && CreateDir(HALInstallFolder))
		{
			DoCopy(text, Path.Combine(HALInstallFolder, HALConfig));
		}
	}

	internal PowerhouseUtilities(IRuntimeService rts)
	{
		RuntimeService = rts;
		Console.WriteLine("Enter identifier:");
		string text = Console.ReadLine();
		Console.WriteLine("Identifier entered: {0}", text);
		string text2 = Path.Combine(RuntimeService.AssemblyDirectory, text);
		if (CreateDir(text2))
		{
			BackupDirectory = text2;
			BackupOk = true;
		}
	}

	private bool ForFiles(string sourceDirectory, string backup)
	{
		int copies = 0;
		Array.ForEach(GampFiles, delegate(string cf)
		{
			string text = Path.Combine(sourceDirectory, cf);
			if (File.Exists(text))
			{
				string dest = Path.Combine(backup, cf);
				if (DoCopy(text, dest))
				{
					int num = copies + 1;
					copies = num;
				}
			}
			else
			{
				Console.WriteLine("The file '{0}' doesn't exist.");
			}
		});
		return copies == GampFiles.Length;
	}

	private bool CreateDir(string directory)
	{
		if (Directory.Exists(directory))
		{
			return true;
		}
		try
		{
			Directory.CreateDirectory(directory);
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to create directory '{0}'", directory);
			Console.WriteLine(ex.Message);
			return false;
		}
	}

	private bool DoCopy(string src, string dest)
	{
		try
		{
			File.Copy(src, dest, overwrite: true);
			Console.WriteLine("Copied {0} --> {1}", src, dest);
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to copy {0} --> {1}", src, dest);
			Console.WriteLine(ex.Message);
			return false;
		}
	}
}
