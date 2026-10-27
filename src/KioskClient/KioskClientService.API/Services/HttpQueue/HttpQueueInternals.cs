using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace KioskClientService.API.Services.HttpQueue;

internal static class HttpQueueInternals
{
	private static readonly string _queueFolder;

	private static readonly JsonSerializerSettings _serializerSettings;

	static HttpQueueInternals()
	{
		_serializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};
		_queueFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox", "KioskClient", "HttpQueue");
		Log.Information("HttpQueueInternals() - Queue Folder: " + _queueFolder);
	}

	internal static bool Enqueue(HttpQueueServiceRequest request)
	{
		bool result = false;
		try
		{
			if (request == null)
			{
				Log.Error("HttpQueueInternals.Enqueue - request is null!");
			}
			else
			{
				CheckAndCreateFolder();
				string fullFilename = GetFullFilename(request);
				FileInfo fileInfo = CreateOrGetFile(request, fullFilename);
				TouchFile(fileInfo);
				result = true;
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.Enqueue - exception occured!");
		}
		return result;
	}

	internal static bool Dequeue(out HttpQueueServiceRequest request)
	{
		bool result = false;
		FileInfo fileInfo = null;
		request = null;
		try
		{
			CheckAndCreateFolder();
			fileInfo = GetOldestFile();
			if (fileInfo != null)
			{
				request = ReadAndIncrementFile(fileInfo);
				if (request != null)
				{
					result = true;
				}
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.Dequeue - exception occured!");
		}
		finally
		{
			if (fileInfo != null)
			{
				TouchFile(fileInfo);
			}
		}
		return result;
	}

	internal static bool Finalize(HttpQueueServiceRequest request)
	{
		bool result = false;
		try
		{
			string fullFilename = GetFullFilename(request);
			if (File.Exists(fullFilename))
			{
				Log.Information("HttpQueueInternals.Finalize - queue file " + fullFilename + " exists; deleting");
				File.Delete(fullFilename);
			}
			result = true;
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.Finalize - exception occured!");
		}
		return result;
	}

	internal static IEnumerable<HttpQueueServiceRequest> List()
	{
		List<HttpQueueServiceRequest> list = new List<HttpQueueServiceRequest>();
		try
		{
			CheckAndCreateFolder();
			IOrderedEnumerable<FileInfo> orderedEnumerable = from f in new DirectoryInfo(_queueFolder).GetFiles()
				orderby f.CreationTimeUtc
				select f;
			foreach (FileInfo item in orderedEnumerable)
			{
				HttpQueueServiceRequest httpQueueServiceRequest = ReadFile(item);
				if (httpQueueServiceRequest == null)
				{
					Log.Error("HttpQueueInternals.List - unable to load request " + item.FullName + "; returning empty set!");
					list.Clear();
					break;
				}
				list.Add(httpQueueServiceRequest);
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.List - exception occured!");
		}
		return list;
	}

	internal static int Depth()
	{
		CheckAndCreateFolder();
		return new DirectoryInfo(_queueFolder).GetFiles().Count();
	}

	private static void CheckAndCreateFolder()
	{
		try
		{
			if (!Directory.Exists(_queueFolder))
			{
				Log.Information("HttpQueueInternals.CheckAndCreateFolder - creating queue folder " + _queueFolder);
				Directory.CreateDirectory(_queueFolder);
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.CheckAndCreateFolder - exception occured!");
		}
	}

	private static string GetFullFilename(HttpQueueServiceRequest request)
	{
		return Path.Combine(_queueFolder, request.Filename);
	}

	private static FileInfo CreateOrGetFile(HttpQueueServiceRequest request, string fullpath)
	{
		if (!File.Exists(fullpath))
		{
			WriteFile(request, fullpath);
			Log.Information("HttpQueueInternals.Enqueue - created new file " + request.Filename);
		}
		return new FileInfo(fullpath);
	}

	private static void TouchFile(FileInfo fileInfo)
	{
		DateTime dateTime = (fileInfo.LastAccessTimeUtc = DateTime.UtcNow);
		DateTime creationTimeUtc = (fileInfo.LastWriteTimeUtc = dateTime);
		fileInfo.CreationTimeUtc = creationTimeUtc;
		Log.Information($"HttpQueueInternals.Enqueue - updated file {fileInfo.FullName} timestamp to {fileInfo.CreationTimeUtc}");
	}

	private static FileInfo GetOldestFile()
	{
		return (from f in new DirectoryInfo(_queueFolder).GetFiles()
			orderby f.CreationTimeUtc
			select f).FirstOrDefault();
	}

	private static HttpQueueServiceRequest ReadAndIncrementFile(FileInfo source)
	{
		HttpQueueServiceRequest httpQueueServiceRequest = ReadFile(source);
		if (httpQueueServiceRequest != null)
		{
			try
			{
				httpQueueServiceRequest.Retries++;
				WriteFile(httpQueueServiceRequest, source.FullName);
			}
			catch (Exception exception)
			{
				Log.Error(exception, "HttpQueueInternals.ReadAndIncrementFile - exception occured!");
			}
		}
		return httpQueueServiceRequest;
	}

	private static HttpQueueServiceRequest ReadFile(FileInfo source)
	{
		HttpQueueServiceRequest result = null;
		try
		{
			Log.Information("HttpQueueInternals.ReadFile - processing file: " + source.Name);
			string value = null;
			using (StreamReader streamReader = new StreamReader(source.FullName))
			{
				value = streamReader.ReadToEnd();
			}
			if (!string.IsNullOrEmpty(value))
			{
				result = JsonConvert.DeserializeObject<HttpQueueServiceRequest>(value, _serializerSettings);
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.ReadFile - exception occured!");
		}
		return result;
	}

	private static void WriteFile(HttpQueueServiceRequest request, string fullPath)
	{
		try
		{
			using FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read);
			byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, Formatting.Indented, _serializerSettings));
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Flush();
		}
		catch (Exception exception)
		{
			Log.Error(exception, "HttpQueueInternals.WriteFile() - exception occured!");
		}
	}
}
