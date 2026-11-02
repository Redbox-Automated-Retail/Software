using System;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileBase : IDownloadFile
{
	private IDownloadFileData _downloadFileData = null!; // set by the constructor

	private const string ResultTableName = "Results";

	private const string MarkScriptCompletedText = "kernel.markscriptcomplete";

	public IDownloadFileData DownloadFileData
	{
		get
		{
			return _downloadFileData;
		}
		private set
		{
			_downloadFileData = value;
		}
	}

	public DownloadFileBase(IDownloadFileData downloadFileData)
	{
		DownloadFileData = downloadFileData;
	}

	public virtual ErrorList ProcessDownloadFile()
	{
		return new ErrorList();
	}

	public virtual ErrorList DeleteDownloadFile()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DownloadFileFactory.Instance.DeleteDownloadFile(DownloadFileData);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFB001", "Unhandled exception occurred in DownloadFilebase.DeleteDownloadFile", e));
		}
		return errorList;
	}

	public virtual ErrorList SaveDownloadFile()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DownloadFileFactory.Instance.SaveDownloadFile(DownloadFileData);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFB001", "Unhandled exception occurred in DownloadFilebase.SaveDownloadFile", e));
		}
		return errorList;
	}

	internal ErrorList SendDownloadFileStatus(StatusMessage.StatusMessageType statusMessageType, string description, string? message)
	{
		var data = new
		{
			DownloadFileId = DownloadFileData.Id,
			Name = DownloadFileData.Name,
			DownloadFileDataType = DownloadFileData.DownloadFileDataType.ToString(),
			DownloadFileDataState = DownloadFileData.DownloadFileDataState.ToString(),
			Message = message
		};
		return SendStatus(statusMessageType, description, data);
	}

	internal ErrorList SendResult(StatusMessage.StatusMessageType statusMessageType, string result)
	{
		return SendStatus(statusMessageType, "DownloadFile script result", result);
	}

	internal ErrorList SendStatus(StatusMessage.StatusMessageType statusMessageType, string description, object data)
	{
		return DownloadFileFactory.Instance.SendStatus(statusMessageType, DownloadFileData.StatusKey, description, data);
	}

	internal bool IsValidTime()
	{
		if (string.IsNullOrEmpty(DownloadFileData.StartTime) || string.IsNullOrEmpty(DownloadFileData.EndTime))
		{
			return true;
		}
		if (!TimeSpan.TryParse(DownloadFileData.StartTime, out var startTime))
		{
			return true;
		}
		if (!TimeSpan.TryParse(DownloadFileData.EndTime, out var endTime))
		{
			return true;
		}
		if (startTime == endTime)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		if (endTime < startTime)
		{
			if (!(now.TimeOfDay <= endTime))
			{
				return now.TimeOfDay >= startTime;
			}
			return true;
		}
		if (now.TimeOfDay >= startTime)
		{
			return now.TimeOfDay <= endTime;
		}
		return false;
	}

	internal ErrorList RunScript(out bool scriptCompleted)
	{
		ErrorList errorList = new ErrorList();
		scriptCompleted = true;
		if (string.IsNullOrEmpty(DownloadFileData.ActivateScript))
		{
			return errorList;
		}
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		string resultJson = string.Empty;
		string scriptText = Encoding.Unicode.GetString(DownloadFileData.ActivateScript.Base64ToBytes());
		LogHelper.Instance.Log("DownloadFileBase.RunScript: DownloadFile Id {0} Name {1} running attached script", DownloadFileData.Id, DownloadFileData.Name);
		Exception? ex = null;
		try
		{
			kernelService.ExecuteChunkNoLock(scriptText, reset: true, "Results", out var result, out scriptCompleted);
			if (result != null)
			{
				resultJson = result.ToJson();
			}
		}
		catch (Exception ex2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS9999", $"DownloadFileBase.RunScript: unhandled exception in script. Script for download file Id {DownloadFileData.Id} Name {DownloadFileData.Name}", ex2));
			ex = ex2;
		}
		if (ex != null)
		{
			var failure = new
			{
				Result = false,
				Exception = true,
				Message = ex.Message
			};
			errorList.AddRange(SendResult(StatusMessage.StatusMessageType.Error, failure.ToJson()));
			return errorList;
		}
		bool marksScriptComplete = scriptText.IndexOf("kernel.markscriptcomplete", StringComparison.CurrentCultureIgnoreCase) > -1;
		if (kernelService.ScriptCompleted || !marksScriptComplete)
		{
			scriptCompleted = true;
			if (!string.IsNullOrEmpty(resultJson))
			{
				errorList.AddRange(SendResult(StatusMessage.StatusMessageType.Info, resultJson));
			}
		}
		else
		{
			scriptCompleted = false;
			LogHelper.Instance.Log("DownloadFile Id {0} Name {1} script is incomplete.", DownloadFileData.Id, DownloadFileData.Name);
		}
		return errorList;
	}
}
