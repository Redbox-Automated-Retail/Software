using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class TransferStatisticService : IPollRequestReply
{
	public static TransferStatisticService Instance => Singleton<TransferStatisticService>.Instance;

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		pollRequestList = new List<PollRequest>();
		ErrorList errorList = new ErrorList();
		try
		{
			IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
			for (TransferStatisticReport? transferStatisticReport = queueService.Peek<TransferStatisticReport>("statistic"); transferStatisticReport != null; transferStatisticReport = queueService.Peek<TransferStatisticReport>("statistic"))
			{
				PollRequest item = new PollRequest
				{
					PollRequestType = PollRequestType.TransferStatisticReport,
					Data = transferStatisticReport.ToJson()
				};
				pollRequestList.Add(item);
				LogHelper.Instance.Log("Enqueued transfer statistics for revision: {0}", transferStatisticReport.ChangeSet);
				queueService.Dequeue("statistic");
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService", "Unhandled exception occurred in GetPollRequest.", e));
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		return new ErrorList();
	}

	private TransferStatisticService()
	{
	}
}
