using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetTransition : IFileSetTransition
{
	[Flags]
	private enum MoveFileFlags
	{
		MOVEFILE_REPLACE_EXISTING = 1,
		MOVEFILE_COPY_ALLOWED = 2,
		MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		MOVEFILE_WRITE_THROUGH = 8
	}

	private static bool _rebootRequired;

	private const string FileSetChangeSetPath = "staging";

	private const string FileSetJsonFileName = "fileset.json";

	private IFileCacheService _fileCacheService;

	private IHttpService _httpService;

	private IWindowsServiceFunctions _winServiceFunctions;

	private readonly ICommandLineService _commandLineService;

	private AppSettings _appSettings;

	private ILogger<FileSetTransition> _logger;

	public bool RebootRequired => _rebootRequired;

	public FileSetTransition(IFileCacheService fileCacheService, IHttpService httpService, IWindowsServiceFunctions winServiceFunctions, ICommandLineService cmd, IOptions<AppSettings> appSettings, ILogger<FileSetTransition> logger)
	{
		_fileCacheService = fileCacheService;
		_httpService = httpService;
		_winServiceFunctions = winServiceFunctions;
		_commandLineService = cmd;
		_appSettings = appSettings.Value;
		_logger = logger;
	}

	public void MarkRebootRequired()
	{
		_rebootRequired = true;
	}

	public void ClearRebootRequired()
	{
		_rebootRequired = false;
	}

	public bool Stage(ClientFileSetRevision clientFileSetRevision)
	{
		bool result = true;
		try
		{
			string stagingFileSetRevisionPath = GetStagingFileSetRevisionPath(clientFileSetRevision);
			if (!Directory.Exists(stagingFileSetRevisionPath))
			{
				Directory.CreateDirectory(stagingFileSetRevisionPath);
			}
			List<FileData> fileData = GetFileData(clientFileSetRevision);
			fileData.ForEach(delegate(FileData eachFileData)
			{
				if (!_fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId, eachFileData.FileId, eachFileData.FileRevisionId))
				{
					_logger.LogError($"Missing file for FileId {eachFileData.FileId} Revision {eachFileData.FileRevisionId}");
					result = false;
				}
			});
			if (!result)
			{
				return result;
			}
			fileData.ForEach(delegate(FileData eachFileData)
			{
				if (!_fileCacheService.CopyFileToPath(clientFileSetRevision.FileSetId, eachFileData.FileId, eachFileData.FileRevisionId, eachFileData.StagePath))
				{
					_logger.LogError($"File for FileId {eachFileData.FileId} Revision {eachFileData.FileRevisionId} couldn't be copied to it's staging directory");
					result = false;
				}
			});
			if (!result)
			{
				return result;
			}
			fileData.ForEach(delegate(FileData eachFileData)
			{
				if (!_fileCacheService.IsFileHashValid(eachFileData.StagePath, eachFileData.Hash))
				{
					_logger.LogError($"Hash check failed for FileId {eachFileData.FileId} Revision {eachFileData.FileRevisionId}");
					result = false;
				}
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception while staging files for revision with " + clientFileSetRevision?.IdentifyingText());
		}
		return result;
	}

	public bool CheckMeetsDependency(ClientFileSetRevision clientFileSetRevision, Dictionary<long, FileSetDependencyState> dependencyStates)
	{
		bool result = false;
		try
		{
			if (clientFileSetRevision?.Dependencies?.Any() != true)
			{
				return true;
			}
			result = true;
			foreach (ClientFileSetRevisionDependency dependency in clientFileSetRevision.Dependencies)
			{
				if (!dependencyStates.ContainsKey(dependency.DependsOnFileSetId))
				{
					_logger.LogWarningWithSource($"Missing dependency FileSet with FileSetId {dependency.DependsOnFileSetId} for revision {clientFileSetRevision.IdentifyingText()} Revision {clientFileSetRevision.RevisionId}", "CheckMeetsDependency", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
					result = false;
					continue;
				}
				FileSetDependencyState fileSetDependencyState = dependencyStates[dependency.DependsOnFileSetId];
				if (!fileSetDependencyState.IsDependencyMet(dependency.DependencyType, dependency.MinimumVersion, dependency.MaximumVersion))
				{
					_logger.LogWarningWithSource("Missing dependency version for " + clientFileSetRevision.IdentifyingText() + ". Expected version " + ExpectedDependencyVersionInfo(dependency, fileSetDependencyState) + ". Current version: '" + ((fileSetDependencyState.InProgressRevisionId > 0) ? fileSetDependencyState.InProgressVersion : fileSetDependencyState.ActiveVersion) + "'", "CheckMeetsDependency", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
					result = false;
				}
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception while checking dependencies for revision " + clientFileSetRevision?.IdentifyingText());
		}
		return result;
	}

	private static string ExpectedDependencyVersionInfo(ClientFileSetRevisionDependency clientFileSetRevisionDependency, FileSetDependencyState fileSetDependencyState)
	{
		return new
		{
			FileSetId = fileSetDependencyState.FileSetId,
			DependencyType = clientFileSetRevisionDependency.DependencyType.ToString(),
			MinimumVersion = clientFileSetRevisionDependency.MinimumVersion,
			MaximumVersion = clientFileSetRevisionDependency.MaximumVersion
		}.ToJson();
	}

	private async Task CallUpdateClientInstaller(ClientFileSetRevision clientFileSetRevision)
	{
		_logger.LogInfoWithSource("Calling UpdateClientInstaller for revision with " + clientFileSetRevision.ToJsonIndented(), "CallUpdateClientInstaller", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
		APIResponse<object> aPIResponse = await _httpService.SendRequestAsync<object>(_appSettings.UpdateClientInstallerUrl, "api/activation?waitForResponse=false", clientFileSetRevision, HttpMethod.Post, null, isForm: false, null, "CallUpdateClientInstaller", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs", null, logRequest: false);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			_logger.LogErrorWithSource("Error while calling UpdateClientInstaller.Error-> " + aPIResponse.Errors.ToJson(), "CallUpdateClientInstaller", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
		}
	}

	public bool BeforeActivate(ClientFileSetRevision clientFileSetRevision)
	{
		bool flag = true;
		try
		{
			FileSetJsonModel fileSetJsonModel = GetFileSetJsonModel(clientFileSetRevision, beforeActivations: true);
			if (fileSetJsonModel == null)
			{
				_logger.LogWarningWithSource("FileSetJson was null skipping BeforeActivate.", "BeforeActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return flag;
			}
			if (fileSetJsonModel?.WindowsServiceName == "updateclient$service")
			{
				Task.Run(async delegate
				{
					await CallUpdateClientInstaller(clientFileSetRevision);
				});
				return false;
			}
			if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName) && _winServiceFunctions.ServiceExists(fileSetJsonModel.WindowsServiceName))
			{
				_winServiceFunctions.StopService(fileSetJsonModel.WindowsServiceName);
			}
			List<FileData> fileData = GetFileData(clientFileSetRevision);
			FileData fileData2 = fileData.Where((FileData f) => string.Equals(Path.GetFileName(f.FileDestination), "beforeactivate.ps1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (fileData2 != null)
			{
				flag = RunScript(clientFileSetRevision, fileData2.StagePath);
				if (!flag)
				{
					_logger.LogErrorWithSource("Error running BeforeActivate script for " + clientFileSetRevision?.IdentifyingText(), "BeforeActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while processing BeforeActivate for " + clientFileSetRevision?.IdentifyingText(), "BeforeActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			flag = false;
		}
		return flag;
	}

	private FileSetJsonModel GetFileSetJsonModel(ClientFileSetRevision clientFileSetRevision, bool beforeActivations)
	{
		FileSetJsonModel result = null;
		List<FileData> fileData = GetFileData(clientFileSetRevision);
		FileData fileData2 = fileData.FirstOrDefault((FileData x) => Path.GetFileName(x.FileDestination).ToLower() == "fileset.json");
		if (fileData2 == null)
		{
			_logger.LogWarningWithSource("fileset.json does not exist for revision: " + clientFileSetRevision.IdentifyingText(), "GetFileSetJsonModel", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			return null;
		}
		string text = (beforeActivations ? fileData2.StagePath : fileData2.FileDestination);
		try
		{
			string obj = File.ReadAllText(text);
			result = obj.ToObject<FileSetJsonModel>();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting fileset.json from " + text + ".", "GetFileSetJsonModel", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
		}
		return result;
	}

	public bool AfterActivate(ClientFileSetRevision clientFileSetRevision)
	{
		bool result = true;
		try
		{
			FileSetJsonModel fileSetJsonModel = GetFileSetJsonModel(clientFileSetRevision, beforeActivations: false);
			if (fileSetJsonModel == null)
			{
				_logger.LogWarningWithSource("FileSetJson was null skipping AfterActivate.", "AfterActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return result;
			}
			if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName))
			{
				result = InstallAndStartWindowsService(fileSetJsonModel);
			}
			List<FileData> fileData = GetFileData(clientFileSetRevision);
			FileData fileData2 = fileData.Where((FileData f) => string.Equals(Path.GetFileName(f.FileDestination), "afteractivate.ps1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (fileData2 != null && !RunScript(clientFileSetRevision, fileData2.FileDestination))
			{
				_logger.LogErrorWithSource("Error while running afteractivate script for " + clientFileSetRevision?.IdentifyingText(), "AfterActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				result = false;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while running afteractivate script for " + clientFileSetRevision?.IdentifyingText(), "AfterActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			result = false;
		}
		return result;
	}

	private bool InstallAndStartWindowsService(FileSetJsonModel fileSetJson)
	{
		bool result = true;
		if (!_winServiceFunctions.ServiceExists(fileSetJson.WindowsServiceName))
		{
			if (!string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceStartFile))
			{
				string displayName = (string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceDisplayName) ? fileSetJson.WindowsServiceName : fileSetJson.WindowsServiceDisplayName);
				string binPath = Path.Combine(fileSetJson.InstallPath, fileSetJson.WindowsServiceStartFile);
				_winServiceFunctions.InstallService(fileSetJson.WindowsServiceName, displayName, binPath);
			}
			else
			{
				_logger.LogErrorWithSource("windowsServiceStartPath in fileSetJson is null for windowsServiceName " + fileSetJson.WindowsServiceName, "InstallAndStartWindowsService", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				result = false;
			}
		}
		_winServiceFunctions.StartService(fileSetJson.WindowsServiceName);
		return result;
	}

	public bool Activate(ClientFileSetRevision clientFileSetRevision)
	{
		bool flag = true;
		try
		{
			List<FileData> fileData = GetFileData(clientFileSetRevision);
			flag = StagedFilesExist(clientFileSetRevision, fileData);
			if (!flag)
			{
				return flag;
			}
			Action<string, string> action = delegate(string sourceFilePath, string destinationfilePath)
			{
				if (File.Exists(destinationfilePath))
				{
					File.Delete(destinationfilePath);
				}
				File.Move(sourceFilePath, destinationfilePath);
			};
			foreach (FileData item in fileData)
			{
				flag = CreateFileDestinationDirectory(item);
				if (!flag)
				{
					return flag;
				}
				try
				{
					action(item.StagePath, item.FileDestination);
				}
				catch (Exception ex)
				{
					try
					{
						_logger.LogInfoWithSource(item.FileDestination + " is locked by anther process attempting to retry.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						_logger.LogErrorWithSource(ex, "Error from activate copy - retrying.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						Task.Delay(100).Wait();
						action(item.StagePath, item.FileDestination);
					}
					catch (Exception ex2)
					{
						_logger.LogInfoWithSource(item.FileDestination + " is locked by anther process doing a deferred move.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						_logger.LogErrorWithSource(ex2, "Error from activate copy.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						string tempFileName = Path.GetTempFileName();
						action(item.StagePath, tempFileName);
						MoveLockedFileSystemEntry(tempFileName, item.FileDestination);
						MarkRebootRequired();
					}
				}
			}
		}
		catch (Exception ex3)
		{
			_logger.LogErrorWithSource(ex3, "Exception while activing revision with " + clientFileSetRevision?.IdentifyingText(), "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			flag = false;
		}
		return flag;
	}

	private bool StagedFilesExist(ClientFileSetRevision clientFileSetRevision, List<FileData> fileDataList)
	{
		bool result = true;
		fileDataList?.ForEach(delegate(FileData eachFileData)
		{
			if (!File.Exists(eachFileData.StagePath))
			{
				_logger.LogErrorWithSource($"Missing file {eachFileData.StagePath} for FileId {eachFileData.FileId} and revision with {clientFileSetRevision?.IdentifyingText()}", "StagedFilesExist", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				result = false;
			}
		});
		return result;
	}

	private bool CreateFileDestinationDirectory(FileData fileData)
	{
		bool result = true;
		try
		{
			string directoryName = Path.GetDirectoryName(fileData.FileDestination);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while creating directory " + fileData?.FileDestination, "CreateFileDestinationDirectory", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			result = false;
		}
		return result;
	}

	private static void MoveLockedFileSystemEntry(string source, string destination)
	{
		MoveFileFlags moveFileFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
		if (!Directory.Exists(source) && !Directory.Exists(destination))
		{
			moveFileFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
		}
		MoveFileEx(source, destination, moveFileFlags);
	}

	[DllImport("kernel32.dll")]
	private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

	private List<FileData> GetFileData(ClientFileSetRevision clientFileSetRevision)
	{
		List<FileData> list = new List<FileData>();
		string stagingFileSetRevisionPath = GetStagingFileSetRevisionPath(clientFileSetRevision);
		foreach (ClientFileSetFile file in clientFileSetRevision.Files)
		{
			FileData item = new FileData
			{
				FileId = file.FileId,
				FileRevisionId = file.FileRevisionId,
				Size = file.ContentSize,
				Hash = file.ContentHash,
				StagePath = Path.Combine(stagingFileSetRevisionPath, _fileCacheService.GetRevisionFileName(file.FileId, file.FileRevisionId)),
				FileDestination = file.FileDestination
			};
			list.Add(item);
		}
		return list;
	}

	private bool RunScript(ClientFileSetRevision clientFileSetRevision, string scriptPath)
	{
		try
		{
			if (string.IsNullOrEmpty(scriptPath))
			{
				_logger.LogWarningWithSource("Script was empty for " + clientFileSetRevision.IdentifyingText() + ". Skipping...", "RunScript", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return true;
			}
			if (Path.GetExtension(scriptPath) == ".file")
			{
				string text = Path.GetTempFileName().Replace(".tmp", ".ps1");
				File.Copy(scriptPath, text);
				bool result = _commandLineService.TryExecutePowerShellScriptFromFile(text);
				File.Delete(text);
				return result;
			}
			return _commandLineService.TryExecutePowerShellScriptFromFile(scriptPath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while running script " + scriptPath, "RunScript", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			return false;
		}
	}

	private string GetStagingDirectory()
	{
		return Path.Combine(Constants.FileSetsRoot, "staging");
	}

	private string GetStagingFileSetPath(ClientFileSetRevision clientFileSetRevision)
	{
		return Path.Combine(GetStagingDirectory(), clientFileSetRevision.FileSetId.ToString());
	}

	private string GetStagingFileSetRevisionPath(ClientFileSetRevision clientFileSetRevision)
	{
		return Path.Combine(GetStagingFileSetPath(clientFileSetRevision), clientFileSetRevision.RevisionId.ToString());
	}
}
