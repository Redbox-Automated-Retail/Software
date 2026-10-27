using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Component.Model.Services;

public sealed class RuntimeService : IRuntimeService
{
	private struct SYSTEM_INFO
	{
		internal _PROCESSOR_INFO_UNION uProcessorInfo;

		public uint dwPageSize;

		public IntPtr lpMinimumApplicationAddress;

		public IntPtr lpMaximumApplicationAddress;

		public IntPtr dwActiveProcessorMask;

		public uint dwNumberOfProcessors;

		public uint dwProcessorType;

		public uint dwAllocationGranularity;

		public ushort dwProcessorLevel;

		public ushort dwProcessorRevision;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct _PROCESSOR_INFO_UNION
	{
		[FieldOffset(0)]
		internal uint dwOemId;

		[FieldOffset(0)]
		internal ushort wProcessorArchitecture;

		[FieldOffset(2)]
		internal ushort wReserved;
	}

	public const string RunningPath = "RunningPath";

	private Platform Platform;

	private string m_kioskId;

	private readonly WaitHandle m_waitObject = new ManualResetEvent(initialState: false);

	private readonly StringDictionary Values = new StringDictionary();

	public string KioskId
	{
		get
		{
			if (!string.IsNullOrEmpty(m_kioskId))
			{
				return m_kioskId;
			}
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
				if (registryKey != null)
				{
					object value = registryKey.GetValue("ID");
					if (value != null)
					{
						m_kioskId = ConversionHelper.ChangeType<string>(value);
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("[{0}] Unable to read Kiosk ID", GetType().Name);
				LogHelper.Instance.Log(ex.Message);
				m_kioskId = null;
			}
			finally
			{
				registryKey?.Close();
			}
			if (!string.IsNullOrEmpty(m_kioskId))
			{
				return m_kioskId;
			}
			return "UNKNOWN";
		}
	}

	public string DataPath { get; private set; }

	public string AssemblyDirectory { get; private set; }

	public string InstallRoot { get; private set; }

	public int PageSize { get; private set; }

	public string ScriptsPath { get; private set; }

	public string ExpandRuntimeMacros(string value)
	{
		return ExpandMacros(value, "${", "}");
	}

	public string ExpandConstantMacros(string value)
	{
		return ExpandMacros(value, "$(", ")");
	}

	public string RuntimePath(string fileName)
	{
		return Path.Combine(AssemblyDirectory, fileName);
	}

	public string InstallPath(string subfolder)
	{
		return Path.Combine(InstallRoot, subfolder);
	}

	public string CreateBackup(string path)
	{
		return CreateBackup(path, BackupAction.Delete);
	}

	public string CreateBackup(string path, BackupAction action)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		string text = CreateBackupName(path);
		if (action == BackupAction.Move)
		{
			File.Move(path, text);
		}
		else if (BackupAction.Copy == action)
		{
			File.Copy(path, text);
		}
		else if (BackupAction.Delete == action)
		{
			SafeDelete(path);
		}
		return text;
	}

	public string GenerateUniqueFile(string suffix)
	{
		if (!suffix.StartsWith("."))
		{
			suffix = $".{suffix}";
		}
		DateTime now = DateTime.Now;
		return $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}{suffix}";
	}

	public byte[] ReadToBuffer(string fileName)
	{
		if (fileName == null)
		{
			return null;
		}
		byte[] array = null;
		using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		array = new byte[fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		return array;
	}

	public bool ForceSafeCopy(string source, string dest)
	{
		return ForceSafeCopy(TextWriter.Null, source, dest);
	}

	public bool ForceSafeCopy(TextWriter w, string source, string dest)
	{
		return OnCopy(w, source, dest, force: true);
	}

	public bool SafeCopy(string source, string dest)
	{
		return SafeCopy(TextWriter.Null, source, dest);
	}

	public bool SafeCopy(TextWriter writer, string source, string dest)
	{
		return OnCopy(writer, source, dest, force: false);
	}

	public bool SafeMove(string source, string dest)
	{
		return SafeMove(TextWriter.Null, source, dest);
	}

	public bool SafeMove(TextWriter writer, string source, string dest)
	{
		string arg = $"move {source} -> {dest}";
		try
		{
			File.Move(source, dest);
			writer.WriteLine("[{0}] {1}: SUCCESS", GetType().Name, arg);
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("[{0}] {1}: FAILURE reason = {2}", GetType().Name, arg, ex.Message);
			return false;
		}
	}

	public bool SafeDelete(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return false;
		}
		bool flag = false;
		try
		{
			File.Delete(path);
			return true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("[{0}] Delete file failed:", GetType().Name);
			LogHelper.Instance.Log(ex.Message);
			return false;
		}
	}

	public Platform GetPlatform()
	{
		if (Platform != Platform.None)
		{
			return Platform;
		}
		Platform = Platform.Unknown;
		OperatingSystem oSVersion = Environment.OSVersion;
		if (oSVersion.Platform != PlatformID.Win32NT)
		{
			return Platform;
		}
		switch (oSVersion.Version.Major)
		{
		case 5:
			if (oSVersion.Version.Minor != 0)
			{
				Platform = Platform.WindowsXP;
			}
			break;
		case 6:
			if (oSVersion.Version.Minor != 0)
			{
				Platform = Platform.Windows7;
			}
			break;
		}
		return Platform;
	}

	public void SpinWait(int milliseconds)
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, milliseconds);
		SpinWait(timespan);
	}

	public void SpinWait(TimeSpan timespan)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		while ((double)executionTimer.ElapsedMilliseconds < timespan.TotalMilliseconds)
		{
		}
	}

	public void Wait(int ms)
	{
		if (ms < 1000)
		{
			SpinWait(ms);
		}
		else
		{
			m_waitObject.WaitOne(ms, exitContext: false);
		}
	}

	public RuntimeService()
	{
		AssemblyDirectory = Path.GetDirectoryName(typeof(RuntimeService).Assembly.Location);
		DataPath = Path.Combine(AssemblyDirectory, "data");
		try
		{
			DirectoryInfo parent = Directory.GetParent(AssemblyDirectory);
			InstallRoot = parent.FullName;
			ScriptsPath = Path.Combine(InstallRoot, "Scripts");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("[RuntimeService] Unable to find parent folder.", ex.Message);
			InstallRoot = AssemblyDirectory;
		}
		Platform = Platform.None;
		Values["RunningPath"] = AssemblyDirectory;
		SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
		GetSystemInfo(ref lpSystemInfo);
		PageSize = (int)lpSystemInfo.dwPageSize;
	}

	private bool OnCopy(TextWriter writer, string source, string dest, bool force)
	{
		string text = $"copy {source} -> {dest}";
		try
		{
			File.Copy(source, dest, force);
			writer.WriteLine("[{0}] {1}: SUCCESS", GetType().Name, text);
			return true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "COPY FAILURE {0}: reason = {1}", text, ex.Message);
			return false;
		}
	}

	private string CreateBackupName(string path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string directoryName = Path.GetDirectoryName(path);
		string extension = Path.GetExtension(path);
		string path2 = $"{fileNameWithoutExtension}-{GenerateUniqueFile(extension)}";
		return Path.Combine(directoryName, path2);
	}

	private string ExpandMacros(string value, string preamble, string postamble)
	{
		if (value == null)
		{
			return null;
		}
		while (true)
		{
			string text = StringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			string text2 = Values[text];
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			value = value.Replace($"{preamble}{text}{postamble}", text2);
		}
		return value;
	}

	[DllImport("kernel32.dll")]
	private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);
}
