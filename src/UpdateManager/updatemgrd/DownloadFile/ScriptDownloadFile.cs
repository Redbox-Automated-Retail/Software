using System;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class ScriptDownloadFile : DownloadFileBase
{
	private const string DownloadFileTypeName = "Script download";

	public ScriptDownloadFile(IDownloadFileData downloadFileData)
		: base(downloadFileData)
	{
	}

	public override ErrorList ProcessDownloadFile()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.None)
		{
			errorList.AddRange(CheckNone());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostInstall)
		{
			errorList.AddRange(CheckPostInstall());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Complete)
		{
			errorList.AddRange(CheckComplete());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Error)
		{
			errorList.AddRange(CheckError());
		}
		return errorList;
	}

	private ErrorList CheckNone()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.None)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Script download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			if (string.IsNullOrEmpty(base.DownloadFileData.ActivateScript))
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Script cannot be empty.", null));
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostInstall;
			errorList.AddRange(SaveDownloadFile());
			errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Script is available pending execution", null));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in ScriptDownloadFile.CheckNone", e));
		}
		return errorList;
	}

	private ErrorList CheckPostInstall()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostInstall)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Script download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		if (!IsValidTime())
		{
			LogHelper.Instance.Log("ScriptDownloadFile: CheckPostInstall is skipping download file {0} until start: {1} and end time: {2} are valid.", base.DownloadFileData.Name, base.DownloadFileData.StartTime, base.DownloadFileData.EndTime);
			return errorList;
		}
		if (!string.IsNullOrEmpty(base.DownloadFileData.ActivateScript))
		{
			errorList.AddRange(RunScript(out var scriptCompleted));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (!scriptCompleted)
			{
				return errorList;
			}
		}
		base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
		errorList.AddRange(SaveDownloadFile());
		return errorList;
	}

	private ErrorList CheckComplete()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Complete)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Script download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile success", null));
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}

	private ErrorList CheckError()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Error)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Script download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile error", base.DownloadFileData.Message));
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}
}
