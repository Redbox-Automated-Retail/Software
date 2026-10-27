using System;
using System.Diagnostics;
using System.IO;
using Redbox.Core;

namespace Redbox.DeltaCompression.Binary;

public static class XDeltaHelper
{
	private static readonly string Name = "xdelta3.exe";

	public static ErrorList Apply(string source, string patch, string target)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				ErrorDialog = false,
				FileName = Name,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = FormatApplyArguments(Path.GetFullPath(source), Path.GetFullPath(target), Path.GetFullPath(patch)),
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			using Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
			string text = process.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log("XDELTA ERRORS: {0}", text);
				errorList.Add(Error.NewError("X999", "Xdelta errors.", text));
			}
			process.StandardError.Close();
			process.StandardInput.Close();
			process.StandardOutput.Close();
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("X998", "Error calling xdelta.", e));
		}
		return errorList;
	}

	public static ErrorList Apply(Stream source, Stream patch, Stream target)
	{
		ErrorList errorList = new ErrorList();
		string tempFileName = Path.GetTempFileName();
		string tempFileName2 = Path.GetTempFileName();
		string tempFileName3 = Path.GetTempFileName();
		try
		{
			BufferedWriteToFile(source, tempFileName);
			BufferedWriteToFile(patch, tempFileName3);
			errorList.AddRange(Apply(tempFileName, tempFileName3, tempFileName2));
			if (!errorList.ContainsError())
			{
				BufferedReadFromFile(tempFileName2, target);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("X999", "Error applying patch", e));
		}
		finally
		{
			File.Delete(tempFileName);
			File.Delete(tempFileName2);
			File.Delete(tempFileName3);
		}
		return errorList;
	}

	public static ErrorList Create(string source, string target, string patch)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				ErrorDialog = false,
				FileName = Name,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = FormatCreateArguments(Path.GetFullPath(source), Path.GetFullPath(target), Path.GetFullPath(patch)),
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			using Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
			string text = process.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(text))
			{
				errorList.Add(Error.NewError("X999", "Xdelta errors.", text));
			}
			process.StandardError.Close();
			process.StandardInput.Close();
			process.StandardOutput.Close();
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("X998", "Error calling xdelta.", e));
		}
		return errorList;
	}

	public static ErrorList Create(Stream source, Stream target, Stream patch)
	{
		ErrorList errorList = new ErrorList();
		string tempFileName = Path.GetTempFileName();
		string tempFileName2 = Path.GetTempFileName();
		string tempFileName3 = Path.GetTempFileName();
		try
		{
			BufferedWriteToFile(source, tempFileName);
			BufferedWriteToFile(target, tempFileName2);
			errorList.AddRange(Create(tempFileName, tempFileName2, tempFileName3));
			if (!errorList.ContainsError())
			{
				BufferedReadFromFile(tempFileName3, patch);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("X999", "Error creating a patch.", e));
		}
		finally
		{
			File.Delete(tempFileName);
			File.Delete(tempFileName2);
			File.Delete(tempFileName3);
		}
		return errorList;
	}

	private static void BufferedWriteToFile(Stream s, string target)
	{
		byte[] array = new byte[65536];
		using (FileStream fileStream = File.Create(target))
		{
			for (int num = s.Read(array, 0, array.Length); num > 0; num = s.Read(array, 0, array.Length))
			{
				fileStream.Write(array, 0, num);
			}
		}
		if (s.CanSeek)
		{
			s.Seek(0L, SeekOrigin.Begin);
		}
	}

	private static void BufferedReadFromFile(string source, Stream target)
	{
		byte[] array = new byte[65536];
		using (FileStream fileStream = File.OpenRead(source))
		{
			for (int num = fileStream.Read(array, 0, array.Length); num > 0; num = fileStream.Read(array, 0, array.Length))
			{
				target.Write(array, 0, num);
			}
		}
		if (target.CanSeek)
		{
			target.Seek(0L, SeekOrigin.Begin);
		}
	}

	private static string FormatApplyArguments(string source, string target, string patch)
	{
		return $"-f -d -s \"{source}\" \"{patch}\" \"{target}\"";
	}

	private static string FormatCreateArguments(string source, string target, string patch)
	{
		return $"-f -e -s \"{source}\" \"{target}\" \"{patch}\"";
	}
}
