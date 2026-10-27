using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class ConfigFileService : IConfigFileService, IPollRequestReply
{
	private class ConfigFileStateLabel
	{
		public long GenerationId { get; set; }

		public DateTime? Created { get; set; }
	}

	private int _syncID;

	private string _clientConfigFileHash;

	private const string ConfigFile = ".config";

	private const string ConfigFileStaged = ".configstaged";

	private const string StagedDate = ".staged";

	private const string ActiveDate = ".active";

	private const string ArchiveDirectory = "archive";

	public static ConfigFileService Instance => Singleton<ConfigFileService>.Instance;

	private string _root { get; set; }

	public void Initialize(string rootPath)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		_root = service.ExpandProperties(rootPath);
		if (!Path.IsPathRooted(_root))
		{
			_root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), rootPath);
		}
		MakePathExist();
		if (ServiceLocator.Instance.GetService<IConfigFileService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IConfigFileService), this);
		}
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			ErrorList cCFDTOList = GetCCFDTOList(out var clientConfigFileDataList);
			errorList.AddRange(cCFDTOList);
			string text = clientConfigFileDataList.ToJson();
			string text2 = Encoding.ASCII.GetBytes(text).ToASCIISHA1Hash();
			if (!string.IsNullOrEmpty(_clientConfigFileHash) && _clientConfigFileHash != text2)
			{
				_syncID = 0;
			}
			errorList.AddRange(SetHash());
			if (_syncID == 0)
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.ConfigFiles,
					SyncId = 0,
					Data = text
				});
			}
			else
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.ConfigFiles,
					SyncId = _syncID
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.GetPollRequest.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if ((from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.UpdateConfigFiles
				select pr).FirstOrDefault() != null)
			{
				_syncID = 0;
				_clientConfigFileHash = string.Empty;
				LogHelper.Instance.Log("Config File server is clearing the sync id as requested by the server.");
				return new ErrorList();
			}
			PollReply pollReply = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.ConfigFileChangeSet
				select pr).FirstOrDefault();
			if (pollReply != null)
			{
				if (string.IsNullOrEmpty(pollReply.Data))
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0092", "Error, A poll reply contained a type of config file changeset without the changeset.", pollReply.ToJson()));
				}
				else
				{
					foreach (ConfigFileChangeSetData item in pollReply.Data.ToObject<List<ConfigFileChangeSetData>>())
					{
						ErrorList errorList2 = ProcessChangeSet(item);
						errorList.AddRange(errorList2);
						if (!errorList2.ContainsError())
						{
							errorList.AddRange(StageChangeSet(item));
						}
					}
				}
				_syncID = pollReply.SyncId;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public List<long> GetAllConfigFiles()
	{
		List<long> list = new List<long>();
		try
		{
			MakePathExist();
			string[] directories = Directory.GetDirectories(_root);
			for (int i = 0; i < directories.Length; i++)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
				try
				{
					long item = Convert.ToInt64(directoryInfo.Name);
					list.Add(item);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"(CFS9988) Invalid name in .configfiles directory.  Remove ({directoryInfo.Name})", e);
				}
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.GetAllConfigFiles.", e2);
		}
		return list;
	}

	public ErrorList ActivateConfigFile(long configFileId)
	{
		ErrorList configFileChangeSetData = GetConfigFileChangeSetData(configFileId, out var changeSet, out var _, out var _);
		if (configFileChangeSetData.ContainsError())
		{
			return configFileChangeSetData;
		}
		return ActivateChangeSet(changeSet);
	}

	private ErrorList SetHash()
	{
		ErrorList cCFDTOList = GetCCFDTOList(out var clientConfigFileDataList);
		string s = clientConfigFileDataList.ToJson();
		string clientConfigFileHash = Encoding.ASCII.GetBytes(s).ToASCIISHA1Hash();
		_clientConfigFileHash = clientConfigFileHash;
		return cCFDTOList;
	}

	private ErrorList GetCCFDTOList(out List<CCFDTO> clientConfigFileDataList)
	{
		ErrorList errorList = new ErrorList();
		clientConfigFileDataList = new List<CCFDTO>();
		foreach (long allConfigFile in GetAllConfigFiles())
		{
			ErrorList configFileChangeSetData = GetConfigFileChangeSetData(allConfigFile, out var changeSet, out var stagedState, out var activeState);
			errorList.AddRange(configFileChangeSetData);
			if (!configFileChangeSetData.ContainsError())
			{
				CCFDTO cCFDTO = new CCFDTO();
				cCFDTO.Id = allConfigFile;
				cCFDTO.Name = changeSet.Name;
				if (stagedState != null)
				{
					cCFDTO.SGId = stagedState.GenerationId;
					cCFDTO.SGD = stagedState.Created;
				}
				if (activeState != null)
				{
					cCFDTO.AGId = activeState.GenerationId;
					cCFDTO.AGD = activeState.Created;
				}
				clientConfigFileDataList.Add(cCFDTO);
			}
		}
		return errorList;
	}

	private ErrorList GetConfigFileChangeSetData(long configFileId, out ConfigFileChangeSetData changeSet, out ConfigFileStateLabel stagedState, out ConfigFileStateLabel activeState)
	{
		changeSet = null;
		stagedState = null;
		activeState = null;
		ErrorList errorList = new ErrorList();
		try
		{
			string configFilePath = GetConfigFilePath(configFileId);
			string stagedStatePath = GetStagedStatePath(configFileId);
			string activeStatePath = GetActiveStatePath(configFileId);
			if (!File.Exists(configFilePath))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", $"Configuration file number {configFileId} does not exist.", ""));
			}
			if (!File.Exists(stagedStatePath))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", $"Configuration staged date file number {configFileId} does not exist.", ""));
			}
			string json = File.ReadAllText(configFilePath);
			changeSet = json.ToObject<ConfigFileChangeSetData>();
			string text = File.ReadAllText(stagedStatePath);
			if (string.IsNullOrEmpty(text))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", $"Configuration staged date file number {configFileId} contents should not be empty.", ""));
			}
			else
			{
				stagedState = text.ToObject<ConfigFileStateLabel>();
			}
			if (File.Exists(activeStatePath))
			{
				string text2 = File.ReadAllText(activeStatePath);
				if (!string.IsNullOrEmpty(text2))
				{
					activeState = text2.ToObject<ConfigFileStateLabel>();
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", $"Unhandled exception occurred in ConfigFileService.GetConfigFilechangeSetData for config file {configFileId}.", e));
		}
		if (errorList.ContainsError())
		{
			ClearConfigFile(configFileId);
		}
		return errorList;
	}

	private ErrorList ProcessChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			WriteConfigFile(changeSet);
			DateTime utcNow = DateTime.UtcNow;
			WriteStagedState(changeSet.OID, changeSet.GenerationOID, utcNow);
			WriteConfigFileArchive(changeSet, changeSet.GenerationOID);
			WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, utcNow);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0089", $"Unhandled exception occurred in ConfigFileService.ProcessChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private ErrorList ActivateChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string destinationActivateFilePath = GetDestinationActivateFilePath(changeSet);
			if (File.Exists(destinationActivateFilePath))
			{
				DateTime utcNow = DateTime.UtcNow;
				WriteActiveState(changeSet.OID, changeSet.GenerationOID, utcNow);
				WriteActiveStateArchive(changeSet.OID, changeSet.GenerationOID, utcNow);
				File.Delete(destinationActivateFilePath);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0088", $"Unhandled exception occurred in ConfigFileService.ActivateChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private string GetDestinationFilePath(ConfigFileChangeSetData changeSet)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(changeSet.Path);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		return Path.ChangeExtension(Path.Combine(text, changeSet.Name), ".config");
	}

	private string GetDestinationActivateFilePath(ConfigFileChangeSetData changeSet)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(changeSet.Path);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		return Path.ChangeExtension(Path.Combine(text, changeSet.Name), ".configstaged");
	}

	private ErrorList StageChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string destinationFilePath = GetDestinationFilePath(changeSet);
			string destinationActivateFilePath = GetDestinationActivateFilePath(changeSet);
			File.WriteAllText(destinationFilePath, changeSet.ToJson());
			File.WriteAllText(destinationActivateFilePath, "This file is a placeholder to indicate that config file activation is needed.");
			WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, DateTime.UtcNow);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0087", $"Unhandled exception occurred in ConfigFileService.StageChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private void ClearConfigFile(long configFileId)
	{
		try
		{
			_syncID = 0;
			LogHelper.Instance.Log("Clearing configFile {0}", configFileId);
			string configFilePath = GetConfigFilePath(configFileId);
			string stagedStatePath = GetStagedStatePath(configFileId);
			string activeStatePath = GetActiveStatePath(configFileId);
			if (File.Exists(configFilePath))
			{
				File.Delete(configFilePath);
			}
			if (File.Exists(stagedStatePath))
			{
				File.Delete(stagedStatePath);
			}
			if (File.Exists(activeStatePath))
			{
				File.Delete(activeStatePath);
			}
			string configDirectory = GetConfigDirectory(configFileId);
			if (Directory.Exists(configDirectory))
			{
				Directory.Delete(configDirectory, recursive: true);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.ClearConfigFile for config file {0}.", e, configFileId);
		}
	}

	private void WriteConfigFile(ConfigFileChangeSetData changeSet)
	{
		File.WriteAllText(GetConfigFilePath(changeSet.OID), changeSet.ToJson());
	}

	private void WriteActiveState(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel instance = new ConfigFileStateLabel
		{
			GenerationId = generationId,
			Created = date
		};
		File.WriteAllText(GetActiveStatePath(configFileId), instance.ToJson());
	}

	private void WriteStagedState(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel instance = new ConfigFileStateLabel
		{
			GenerationId = generationId,
			Created = date
		};
		File.WriteAllText(GetStagedStatePath(configFileId), instance.ToJson());
	}

	private string GetConfigDirectory(long configFileId)
	{
		string text = Path.Combine(_root, configFileId.ToString());
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	private string GetConfigFilePath(long configFileId)
	{
		return Path.Combine(GetConfigDirectory(configFileId), ".config");
	}

	private string GetActiveStatePath(long configFileId)
	{
		return Path.Combine(GetConfigDirectory(configFileId), ".active");
	}

	private string GetStagedStatePath(long configFileId)
	{
		return Path.Combine(GetConfigDirectory(configFileId), ".staged");
	}

	private void WriteConfigFileArchive(ConfigFileChangeSetData changeSet, long generationId)
	{
		File.WriteAllText(GetConfigFilePathArchive(changeSet.OID, generationId), changeSet.ToJson());
	}

	private void WriteActiveStateArchive(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel instance = new ConfigFileStateLabel
		{
			GenerationId = generationId,
			Created = date
		};
		File.WriteAllText(GetActiveStatePathArchive(configFileId, generationId), instance.ToJson());
	}

	private void WriteStagedStateArchive(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel instance = new ConfigFileStateLabel
		{
			GenerationId = generationId,
			Created = date
		};
		File.WriteAllText(GetStagedStatePathArchive(configFileId, generationId), instance.ToJson());
	}

	private string GetConfigDirectoryArchive(long configFileId)
	{
		string text = Path.Combine(GetConfigDirectory(configFileId), "archive");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	private string GetConfigFilePathArchive(long configFileId, long generationId)
	{
		return Path.Combine(GetConfigDirectoryArchive(configFileId), generationId + ".config");
	}

	private string GetActiveStatePathArchive(long configFileId, long generationId)
	{
		return Path.Combine(GetConfigDirectoryArchive(configFileId), generationId + ".active");
	}

	private string GetStagedStatePathArchive(long configFileId, long generationId)
	{
		return Path.Combine(GetConfigDirectoryArchive(configFileId), generationId + ".staged");
	}

	private ConfigFileService()
	{
	}

	private void MakePathExist()
	{
		if (!Directory.Exists(_root))
		{
			Directory.CreateDirectory(_root);
		}
	}
}
