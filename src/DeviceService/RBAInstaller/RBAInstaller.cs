using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RBAInstaller;

public class RBAInstaller : IHostedService
{
	private ILogger<RBAInstaller> _logger;

	private IIUC285Proxy _proxy;

	private IFileUpdater _fileUpdater;

	private IConfiguration _configuration;

	private const string UPDATE_FILE = "update.json";

	private const string DEPLOYMENT_FOLDER = "RBA Files";

	public RBAInstaller(ILogger<RBAInstaller> logger, IIUC285Proxy proxy, IFileUpdater fileUpdater, IConfiguration configuration)
	{
		_logger = logger;
		_proxy = proxy;
		_fileUpdater = fileUpdater;
		_configuration = configuration;
	}

	private void Log(string message)
	{
		_logger?.LogInformation(message);
	}

	private bool WaitForConnection()
	{
		if (_proxy.IsConnected)
		{
			Log("Already Connected - Continue.");
			return true;
		}
		Log("Wait For Connection To Device.");
		ManualResetEvent mre = new ManualResetEvent(initialState: false);
		_proxy.OnConnectedHandler += delegate
		{
			mre.Set();
		};
		return mre.WaitOne(180000);
	}

	private bool IsQA()
	{
		return _configuration.GetChildren().Any((IConfigurationSection x) => x.Key == "QA");
	}

	private static string GetFilePath(string fileName)
	{
		return Path.Combine(Path.Combine(Environment.CurrentDirectory, "RBA Files"), fileName);
	}

	private UpdateJson LoadUpdateFile()
	{
		string filePath = GetFilePath("update.json");
		if (!File.Exists(filePath))
		{
			Log("Unable to locate: " + filePath);
			return null;
		}
		UpdateJson result = null;
		try
		{
			string text = File.ReadAllText(filePath);
			Log(text);
			result = JsonConvert.DeserializeObject<UpdateJson>(text, new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unhandled exception in LoadUpdateFile", null);
		}
		return result;
	}

	private bool UpdateRBA(UpdateFile file)
	{
		if (_proxy?.UnitData == null)
		{
			Log("Update RBA Missing Unit Data");
			return false;
		}
		string text = null;
		if (!string.IsNullOrWhiteSpace(file.MinorVersion))
		{
			text = _proxy.GetVariable_29("254");
		}
		Log("Update RBA Version: " + file.CompareVersion + " - Minor Version: " + file.MinorVersion + " - Current Version: " + _proxy.UnitData.ApplicationVersion + " - Current Minor Version: " + text);
		if (_proxy.UnitData.ApplicationVersion == file.CompareVersion && (text == null || file.MinorVersion == text))
		{
			Log("Update RBA Currently Up To Date.");
			return true;
		}
		return WriteFile(file, reboot: true);
	}

	private bool UpdateRKI(UpdateFile file)
	{
		Log($"Begin UpdateRKI - {_proxy?.HasValidVasRKI ?? false}");
		if (_proxy == null)
		{
			Log("UpdateRKI failed due to missing Proxy injection.");
			return false;
		}
		if (_proxy.HasValidVasRKI)
		{
			Log("UpdateRKI already supports VAS");
			return true;
		}
		Log($"UpdateRKI - File IsIntermediateVasKey: {file.IsIntermediateVasKey} - CardReader HasInvalidVasRKI: {_proxy.HasInvalidVasRKI} - HasIntermediateVasKey: {_proxy.HasIntermediateVasRepairKey}");
		if ((file.IsIntermediateVasKey && !_proxy.HasInvalidVasRKI) || (!file.IsIntermediateVasKey && _proxy.HasInvalidVasRKI))
		{
			Log("Skiping File: " + file.Path);
			return true;
		}
		return WriteFile(file, reboot: true);
	}

	private bool WriteFile(UpdateFile file, bool reboot)
	{
		if (string.IsNullOrWhiteSpace(file?.Path))
		{
			Log("Write File Failed - No File Specified");
			return false;
		}
		string filePath = GetFilePath(file.Path);
		if (!File.Exists(filePath))
		{
			Log("Write File Failed - Unable to locate: " + filePath);
			return false;
		}
		bool flag = true;
		try
		{
			FileStream stream = File.OpenRead(filePath);
			return _fileUpdater.WriteStream(stream, file.Path, reboot, waitForReconnect: true).Result;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unhandled Exception During WriteFile.");
			return false;
		}
	}

	private bool BeginUpdates()
	{
		try
		{
			Log("Begin Updates");
			UpdateJson updateJson = LoadUpdateFile();
			if (updateJson?.Files == null || !updateJson.Files.Any())
			{
				Log("Unable to load update files");
				return false;
			}
			bool flag = true;
			foreach (UpdateFile file in updateJson.Files)
			{
				string text = Path.GetExtension(file.Path).ToUpper();
				Log("File Path: " + file.Path + " - Compare Version: " + file.CompareVersion);
				Log($"Connected: {WaitForConnection()}");
				switch (text)
				{
				case ".OGZ":
					flag = UpdateRBA(file);
					break;
				case ".RKI":
					flag = UpdateRKI(file);
					break;
				}
				if (!flag)
				{
					break;
				}
			}
			Log($"Updates Complete Success: {flag}");
			if (flag)
			{
				WaitForConnection();
				int fileUpdateRevisionNumber = _fileUpdater.GetFileUpdateRevisionNumber();
				Log($"Update Revision to {updateJson.Revision} from {fileUpdateRevisionNumber}");
				if (int.TryParse(updateJson.Revision, out var result) && result > fileUpdateRevisionNumber)
				{
					_fileUpdater.SetFileUpdateRevisionNumber(updateJson.Revision);
				}
				else if (result == fileUpdateRevisionNumber)
				{
					Log("Revision Already Up To Date");
				}
				else
				{
					_logger.LogError("Failed to update revision.");
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Unhandled Exception in BeginUpdates: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
			return false;
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		bool flag = WaitForConnection();
		Log($"Connected = {flag}");
		Log("Update Files");
		BeginUpdates();
		Log("Update Complete");
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
