using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.FileCache;

public static class XDeltaHelper
{
	private static readonly string _path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitOperatingSystem ? "etc\\xdelta3-64.exe" : "etc\\xdelta3.exe");

	private static string FormatApplyArguments(string source, string target, string patch)
	{
		return $"-f -d -s \"{source}\" \"{patch}\" \"{target}\"";
	}

	private static string FormatCreateArguments(string source, string target, string patch)
	{
		return $"-f -e -s \"{source}\" \"{target}\" \"{patch}\"";
	}

	public static List<Error> Apply(string source, string patch, string target)
	{
		if (!File.Exists(_path))
		{
			return new List<Error>
			{
				new Error
				{
					Message = "XDelta was not found at '" + _path + "'"
				}
			};
		}
		List<Error> list = new List<Error>();
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				ErrorDialog = false,
				FileName = _path,
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
				list.Add(new Error
				{
					Message = text
				});
			}
			process.StandardError.Close();
			process.StandardInput.Close();
			process.StandardOutput.Close();
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = e.GetFullMessage()
			});
		}
		return list;
	}

	public static List<Error> Apply(Stream source, Stream patch, Stream target)
	{
		List<Error> list = new List<Error>();
		string tempFileName = Path.GetTempFileName();
		string tempFileName2 = Path.GetTempFileName();
		string tempFileName3 = Path.GetTempFileName();
		try
		{
			BufferedWriteToFile(source, tempFileName);
			BufferedWriteToFile(patch, tempFileName3);
			list.AddRange(Apply(tempFileName, tempFileName3, tempFileName2));
			if (!list.Any())
			{
				BufferedReadFromFile(tempFileName2, target);
			}
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "X999 Error applying patch " + e.GetFullMessage()
			});
		}
		finally
		{
			File.Delete(tempFileName);
			File.Delete(tempFileName2);
			File.Delete(tempFileName3);
		}
		return list;
	}

	public static List<Error> Create(string source, string target, string patch)
	{
		if (!File.Exists(_path))
		{
			return new List<Error>
			{
				new Error
				{
					Message = "XDelta was not found at '" + _path + "'"
				}
			};
		}
		List<Error> list = new List<Error>();
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				ErrorDialog = false,
				FileName = _path,
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
				list.Add(new Error
				{
					Message = text
				});
			}
			process.StandardError.Close();
			process.StandardInput.Close();
			process.StandardOutput.Close();
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = e.GetFullMessage()
			});
		}
		return list;
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
}
