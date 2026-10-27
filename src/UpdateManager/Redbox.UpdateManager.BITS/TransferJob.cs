using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.BITS;

public class TransferJob : ITransferJob
{
	private List<ITransferItem> m_items;

	public TransferJobType JobType { get; private set; }

	public string Owner { get; private set; }

	public string Name { get; private set; }

	public Guid ID { get; private set; }

	public DateTime StartTime { get; private set; }

	public DateTime ModifiedTime { get; private set; }

	public DateTime? FinishTime { get; private set; }

	public ulong TotalBytesTransfered { get; private set; }

	public ulong TotalBytes { get; private set; }

	public TransferStatus Status { get; private set; }

	public ErrorList SetNoProgressTimeout(uint timeout)
	{
		ErrorList errorList = new ErrorList();
		if (timeout < 1)
		{
			return errorList;
		}
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.SetNoProgressTimeout(timeout);
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error setting no progress time out of job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error setting no progress time out of job  job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList SetMinimumRetryDelay(uint seconds)
	{
		ErrorList errorList = new ErrorList();
		if (seconds < 60)
		{
			seconds = 60u;
		}
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.SetMinimumRetryDelay(seconds);
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C998", $"Error setting minimum retry delay of job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error setting minimum retry delay of job  job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList TakeOwnership()
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.TakeOwnership();
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error taking ownership of job: {ID}. {pErrorDescription}", ex));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error taking ownership of job  job: {ID}.", e));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList AddItem(string url, string file)
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		IEnumBackgroundCopyFiles pEnum = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.TakeOwnership();
			ppJob.AddFile(url, file);
			ppJob.EnumFiles(out pEnum);
			m_items = GetFiles(pEnum);
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error adding item to job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error Suspend job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
			if (pEnum != null)
			{
				Marshal.ReleaseComObject(pEnum);
			}
		}
		return errorList;
	}

	public ErrorList Complete()
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.Complete();
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error completing job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error Completing job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList Cancel()
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.Cancel();
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error canceling job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error Canceling job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList Suspend()
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.Suspend();
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error suspending job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error Suspend job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList Resume()
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.Resume();
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error resume job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error Suspend job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList SetPriority(TransferJobPriority priority)
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.SetPriority((BG_JOB_PRIORITY)priority);
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error setting job priority on job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error setting priority on job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList SetCallback(ITransferCallbackParameters parameters)
	{
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.SetNotifyFlags(BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED | BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR);
			string text = Path.Combine(parameters.Path, parameters.Executable);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string argument in parameters.Arguments)
			{
				stringBuilder.Append($" {argument}");
			}
			string parameters2 = $"{text}{((stringBuilder.Length > 0) ? stringBuilder.ToString() : string.Empty)}";
			((IBackgroundCopyJob2)ppJob).SetNotifyCmdLine(text, parameters2);
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
			errorList.Add(Error.NewError("C999", $"Error setting callback on job: {ID}. {pErrorDescription}", ex));
			errorList.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("C998", $"Error setting callback job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public void GetErrors(out ErrorList errors)
	{
		errors = new ErrorList();
		if (Status != TransferStatus.TransientError && Status != TransferStatus.Error)
		{
			return;
		}
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			Guid jobID = ID;
			backgroundCopyManager.GetJob(ref jobID, out ppJob);
			ppJob.GetError(out var ppError);
			ppError.GetErrorDescription(0u, out var pErrorDescription);
			ppError.GetErrorContextDescription(0u, out var pContextDescription);
			errors.Add(Error.NewError("B999", pErrorDescription, pContextDescription));
		}
		catch (COMException ex)
		{
			backgroundCopyManager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription2);
			errors.Add(Error.NewError("C999", $"Error getting errors on job: {ID}. {pErrorDescription2}", ex));
			errors.Add(Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		catch (Exception e)
		{
			errors.Add(Error.NewError("C998", $"Error getting errors on job: {ID}.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
	}

	public ErrorList GetItems(out List<ITransferItem> items)
	{
		items = new List<ITransferItem>(m_items.ToArray());
		return new ErrorList();
	}

	internal TransferJob(Guid id, IBackgroundCopyJob job)
	{
		Initialize(id, job);
	}

	internal TransferJob(Guid id)
	{
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			backgroundCopyManager.GetJob(ref id, out ppJob);
			ID = id;
			Initialize(id, ppJob);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
	}

	private static List<ITransferItem> GetFiles(IEnumBackgroundCopyFiles files)
	{
		files.GetCount(out var puCount);
		List<ITransferItem> list = new List<ITransferItem>();
		for (int i = 0; i < puCount; i++)
		{
			files.Next(1u, out var rgelt, out var _);
			list.Add(new TransferItem(rgelt));
			Marshal.ReleaseComObject(rgelt);
		}
		return list;
	}

	private void Initialize(Guid id, IBackgroundCopyJob job)
	{
		IEnumBackgroundCopyFiles pEnum = null;
		try
		{
			job.GetOwner(out var pVal);
			Owner = pVal;
			job.GetTimes(out var pVal2);
			StartTime = pVal2.CreationTime.ToUTCDateTime().Value;
			FinishTime = pVal2.TransferCompletionTime.ToUTCDateTime();
			DateTime? dateTime = pVal2.ModificationTime.ToUTCDateTime();
			ModifiedTime = (dateTime.HasValue ? dateTime.Value : StartTime);
			job.GetProgress(out var pVal3);
			TotalBytesTransfered = pVal3.BytesTransferred;
			TotalBytes = pVal3.BytesTotal;
			job.GetDisplayName(out var pVal4);
			Name = pVal4;
			job.EnumFiles(out pEnum);
			m_items = GetFiles(pEnum);
			job.GetState(out var pVal5);
			job.GetType(out var pVal6);
			switch (pVal6)
			{
			case BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD:
				JobType = TransferJobType.Download;
				break;
			case BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD:
				JobType = TransferJobType.Upload;
				break;
			}
			Status = (TransferStatus)pVal5;
			ID = id;
		}
		finally
		{
			if (pEnum != null)
			{
				Marshal.ReleaseComObject(pEnum);
			}
		}
	}
}
