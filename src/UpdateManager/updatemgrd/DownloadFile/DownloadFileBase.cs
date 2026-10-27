using System;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileBase : IDownloadFile
{
	private IDownloadFileData _downloadFileData;

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

	internal ErrorList SendDownloadFileStatus(StatusMessage.StatusMessageType statusMessageType, string description, string message)
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
		if (!TimeSpan.TryParse(DownloadFileData.StartTime, out var result))
		{
			return true;
		}
		if (!TimeSpan.TryParse(DownloadFileData.EndTime, out var result2))
		{
			return true;
		}
		if (result == result2)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		if (result2 < result)
		{
			if (!(now.TimeOfDay <= result2))
			{
				return now.TimeOfDay >= result;
			}
			return true;
		}
		if (now.TimeOfDay >= result)
		{
			return now.TimeOfDay <= result2;
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
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		string text = string.Empty;
		string text2 = Encoding.Unicode.GetString(DownloadFileData.ActivateScript.Base64ToBytes());
		LogHelper.Instance.Log("DownloadFileBase.RunScript: DownloadFile Id {0} Name {1} running attached script", DownloadFileData.Id, DownloadFileData.Name);
		Exception ex = null;
		try
		{
			service.ExecuteChunkNoLock(text2, reset: true, "Results", out var result, out scriptCompleted);
			if (result != null)
			{
				text = result.ToJson();
			}
		}
		catch (Exception ex2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS9999", $"DownloadFileBase.RunScript: unhandled exception in script. Script for download file Id {DownloadFileData.Id} Name {DownloadFileData.Name}", ex2));
			ex = ex2;
		}
		if (ex != null)
		{
			var instance = new
			{
				Result = false,
				Exception = true,
				Message = ex.Message
			};
			errorList.AddRange(SendResult(StatusMessage.StatusMessageType.Error, instance.ToJson()));
			return errorList;
		}
		bool flag = text2.IndexOf("kernel.markscriptcomplete", StringComparison.CurrentCultureIgnoreCase) > -1;
		if (service.ScriptCompleted || !flag)
		{
			scriptCompleted = true;
			if (!string.IsNullOrEmpty(text))
			{
				errorList.AddRange(SendResult(StatusMessage.StatusMessageType.Info, text));
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
