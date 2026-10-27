using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.Component.Model.Services;

public sealed class ZipFileService : IZipFileService
{
	private readonly IRuntimeService RuntimeService;

	public ZipResult Zip(IEnumerable<string> files, string zipPath)
	{
		ZipResult zipResult = new ZipResult
		{
			ZipFile = zipPath
		};
		try
		{
			using ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(zipPath));
			zipOutputStream.SetLevel(9);
			byte[] array = new byte[4096];
			int num = 0;
			foreach (string file in files)
			{
				FileInfo fileInfo = new FileInfo(file);
				zipOutputStream.PutNextEntry(new ZipEntry(fileInfo.Name)
				{
					DateTime = fileInfo.CreationTime
				});
				using (FileStream fileStream = File.OpenRead(file))
				{
					int num2;
					do
					{
						num2 = fileStream.Read(array, 0, array.Length);
						zipOutputStream.Write(array, 0, num2);
					}
					while (num2 > 0);
				}
				num++;
			}
			zipOutputStream.Finish();
			zipOutputStream.Close();
			zipResult.EntryCount = num;
			zipResult.Success = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("!!OnZipImages: exception!!", e);
			zipResult.Success = false;
		}
		return zipResult;
	}

	public UnzipResult Unzip(string zipFile)
	{
		FileInfo fileInfo = new FileInfo(zipFile);
		string unzipPath = RuntimeService.RuntimePath(Path.GetFileNameWithoutExtension(fileInfo.Name));
		return Unzip(zipFile, unzipPath);
	}

	public UnzipResult Unzip(string zipFile, string unzipPath)
	{
		UnzipResult unzipResult = new UnzipResult
		{
			SourceZip = zipFile,
			OutputPath = unzipPath,
			Success = false
		};
		if (!SafeCreateDirectory(unzipPath))
		{
			return unzipResult;
		}
		try
		{
			using (ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(zipFile)))
			{
				ZipEntry nextEntry;
				while ((nextEntry = zipInputStream.GetNextEntry()) != null)
				{
					if (!(Path.GetFileName(nextEntry.Name) != string.Empty))
					{
						continue;
					}
					string text = Path.Combine(unzipPath, nextEntry.Name);
					using (FileStream fileStream = File.Create(text))
					{
						int num = 2048;
						byte[] array = new byte[2048];
						while (true)
						{
							num = zipInputStream.Read(array, 0, array.Length);
							if (num > 0)
							{
								fileStream.Write(array, 0, num);
								continue;
							}
							break;
						}
					}
					new FileInfo(text).CreationTime = nextEntry.DateTime;
				}
			}
			unzipResult.Success = true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.WithContext("ZipService UnZip failed with an exception", ex);
			unzipResult.Success = false;
		}
		return unzipResult;
	}

	public ZipFileService(IRuntimeService rts)
	{
		RuntimeService = rts;
	}

	private UnzipResult OnUnzip(string zipFile, string outputPath)
	{
		UnzipResult unzipResult = new UnzipResult
		{
			OutputPath = outputPath,
			SourceZip = zipFile,
			Success = false
		};
		if (!SafeCreateDirectory(outputPath))
		{
			return unzipResult;
		}
		int num = 0;
		using ZipFile zipFile2 = new ZipFile(zipFile);
		foreach (ZipEntry item in zipFile2)
		{
			num++;
			if (item.IsDirectory)
			{
				if (!SafeCreateDirectory(item.Name))
				{
					return unzipResult;
				}
				continue;
			}
			byte[] buffer = new byte[4096];
			Stream inputStream = zipFile2.GetInputStream(item);
			string path = Path.Combine(outputPath, item.Name);
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName.Length > 0 && !SafeCreateDirectory(directoryName))
			{
				return unzipResult;
			}
			using FileStream destination = File.Create(path);
			StreamUtils.Copy(inputStream, destination, buffer);
		}
		unzipResult.EntryCount = num;
		unzipResult.Success = num == zipFile2.Count;
		return unzipResult;
	}

	private bool SafeCreateDirectory(string directory)
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
		catch
		{
			return false;
		}
	}
}
