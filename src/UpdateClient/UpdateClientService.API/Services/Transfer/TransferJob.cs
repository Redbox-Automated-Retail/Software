using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.Transfer;

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

	public List<Error> SetNoProgressTimeout(uint timeout)
	{
		List<Error> list = new List<Error>();
		if (timeout < 1)
		{
			return list;
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
			HandleCOMException("Error setting no progress timeout of job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error setting no progress timeout of job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> SetMinimumRetryDelay(uint seconds)
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error setting minimum retry delay of job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error setting minimum retry delay of job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> TakeOwnership()
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error taking ownership of job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error taking ownership of job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> AddItem(string url, string file)
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error adding item to job. Given url: " + url + " Given file path: " + file, backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error adding item to job. Given url: " + url + " Given file path: " + file, list, e);
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
		return list;
	}

	public List<Error> Complete()
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error completing job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error completing job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> Cancel()
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error canceling job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error canceling job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> Suspend()
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error suspending job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error suspending job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> Resume()
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error resuming job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error resuming job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> SetPriority(TransferJobPriority priority)
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error setting job priority on job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error setting job priority on job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> SetCallback(ITransferCallbackParameters parameters)
	{
		List<Error> list = new List<Error>();
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
			HandleCOMException("Error setting callback on job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error setting callback on job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> GetErrors()
	{
		List<Error> list = new List<Error>();
		if (Status != TransferStatus.TransientError && Status != TransferStatus.Error)
		{
			return list;
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
			list.Add(new Error
			{
				Code = "B999",
				Message = pErrorDescription + ". " + pContextDescription
			});
		}
		catch (COMException ex)
		{
			HandleCOMException("Error getting errors on job", backgroundCopyManager, list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException("Error getting errors on job", list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> GetItems(out List<ITransferItem> items)
	{
		items = new List<ITransferItem>(m_items.ToArray());
		return new List<Error>();
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

	private void HandleCOMException(string baseMessage, IBackgroundCopyManager manager, List<Error> errors, COMException ex)
	{
		manager.GetErrorDescription(ex.ErrorCode, 0u, out var pErrorDescription);
		errors.AddRange(new List<Error>
		{
			new Error
			{
				Code = "C999",
				Message = $"{baseMessage}: {ID}. {pErrorDescription}. Exception -> {ex.GetFullMessage()}"
			},
			GetLastWin32Error()
		});
	}

	private void HandleGenericException(string baseMessage, List<Error> errors, Exception e)
	{
		errors.Add(new Error
		{
			Code = "C998",
			Message = $"{baseMessage}: {ID} -> Exception {e.GetFullMessage()}"
		});
	}

	private static Error GetLastWin32Error()
	{
		return new Error
		{
			Code = "B999",
			Message = "Error accessing BITS job. Exception -> " + new Win32Exception(Marshal.GetLastWin32Error()).GetFullMessage()
		};
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
			ModifiedTime = pVal2.ModificationTime.ToUTCDateTime() ?? StartTime;
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
			if (Status == TransferStatus.Error)
			{
				job.GetError(out var _);
				job.GetErrorCount(out var _);
			}
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
