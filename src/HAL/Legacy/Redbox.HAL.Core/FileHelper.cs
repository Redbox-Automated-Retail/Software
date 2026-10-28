using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.HAL.Core;

public static class FileHelper
{
	public static string CreateBackup(string path, BackupAction action)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		string text = CreateBackupPathInner(path);
		switch (action)
		{
		case BackupAction.None:
			return text;
		case BackupAction.Move:
			File.Move(path, text);
			break;
		default:
			File.Copy(path, text);
			break;
		}
		return text;
	}

	public static bool SafeDeleteFile(string path)
	{
		bool flag = false;
		try
		{
			File.Delete(path);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Delete file failed.", e);
			return false;
		}
	}

	public static string CreateBackupName(string path)
	{
		return CreateBackupPathInner(path);
	}

	public static string GetShortName(string path)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		GetShortPathName(path, stringBuilder, stringBuilder.Capacity);
		return stringBuilder.ToString();
	}

	public static bool SafeMove(TextWriter writer, string source, string dest)
	{
		string arg = $"move {source} -> {dest}";
		try
		{
			File.Move(source, dest);
			writer.WriteLine("{0}: SUCCESS", arg);
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("{0}: FAILURE reason = {1}", arg, ex.Message);
			return false;
		}
	}

	public static bool SafeCopy(TextWriter writer, string source, string dest)
	{
		string arg = $"copy {source} -> {dest}";
		try
		{
			File.Copy(source, dest);
			writer.WriteLine("{0}: SUCCESS", arg);
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("{0}: FAILURE reason = {1}", arg, ex.Message);
			return false;
		}
	}

	public static byte[] ReadToBuffer(string fileName)
	{
		if (fileName == null)
		{
			return null;
		}
		byte[] array = null;
		using FileStream fileStream = StreamBase.OpenFile(fileName);
		array = new byte[fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		return array;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath, int cchBuffer);

	private static string CreateBackupPathInner(string path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string directoryName = Path.GetDirectoryName(path);
		string extension = Path.GetExtension(path);
		string path2 = string.Format("{0}.{1}{2}", fileNameWithoutExtension, DateTime.Now.ToString("yyyyMMddHHmmss"), extension ?? ".bak");
		return Path.Combine(directoryName, path2);
	}
}
