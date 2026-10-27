using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class StatusMessageService : IPollRequestReply, IStatusMessageService
{
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	private static string QueueLabel = "StatusMessage";

	public static StatusMessageService Instance => Singleton<StatusMessageService>.Instance;

	public void Initialize()
	{
	}

	public ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string subKey, string description, string data, bool encodeData)
	{
		StatusMessage statusMessage = new StatusMessage
		{
			Type = type,
			Key = key,
			SubKey = subKey,
			Description = description,
			Data = data,
			TimeStamp = DateTime.UtcNow,
			Encode = encodeData
		};
		if (encodeData)
		{
			statusMessage.EncodeData();
		}
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService.EnqueueMessage", "TryEnterWriteLock timeout expired prior to acquiring the lock.") };
		}
		try
		{
			ServiceLocator.Instance.GetService<IQueueService>().Enqueue(QueueLabel, statusMessage);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return new ErrorList();
	}

	public ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string description, string data, bool encodeData)
	{
		return EnqueueMessage(type, key, null, description, data, encodeData);
	}

	public ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string subKey, string description, string data)
	{
		return EnqueueMessage(type, key, subKey, description, data, encodeData: true);
	}

	public ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string description, string data)
	{
		return EnqueueMessage(type, key, null, description, data, encodeData: true);
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequests)
	{
		pollRequests = new List<PollRequest>();
		ErrorList errorList = new ErrorList();
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService.GetPollRequest", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return errorList;
		}
		try
		{
			IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
			for (StatusMessage statusMessage = service.Peek<StatusMessage>(QueueLabel); statusMessage != null; statusMessage = service.Peek<StatusMessage>(QueueLabel))
			{
				PollRequest item = new PollRequest
				{
					PollRequestType = PollRequestType.StatusMessage,
					Data = statusMessage.ToJson()
				};
				pollRequests.Add(item);
				LogHelper.Instance.Log("Dequeued status message key: {0}", statusMessage.Key);
				service.Dequeue(QueueLabel);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService", "Unhandled exception occurred in GetPollRequest.", e));
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		return new ErrorList();
	}

	private StatusMessageService()
	{
		ServiceLocator.Instance.AddService(typeof(IStatusMessageService), this);
	}
}
