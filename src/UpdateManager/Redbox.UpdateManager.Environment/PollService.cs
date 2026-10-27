using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class PollService : IPollService
{
	private IPoll m_poll;

	public static PollService Instance => Singleton<PollService>.Instance;

	public void Initialize(IPoll poll)
	{
		m_poll = poll;
		if (ServiceLocator.Instance.GetService<IPollService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IPollService), this);
		}
	}

	public ErrorList ServerPoll()
	{
		return ServerPoll(suppressHealthUpdate: false);
	}

	public ErrorList ServerPoll(bool suppressHealthUpdate)
	{
		return ServerPoll(m_poll, suppressHealthUpdate);
	}

	public ErrorList ServerPoll(IPoll poll)
	{
		return ServerPoll(poll, suppressHealthUpdate: false);
	}

	public ErrorList ServerPoll(IPoll poll, bool suppressHealthUpdate)
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Entering PollService.ServerPoll, using poll type " + poll.GetType());
		try
		{
			ErrorList pollRequest = ConfigFileService.Instance.GetPollRequest(out var pollRequestList);
			if (errorList.ContainsError())
			{
				errorList.AddRange(pollRequest);
			}
			ErrorList pollRequest2 = StoreInfoService.Instance.GetPollRequest(out var pollRequestList2);
			if (pollRequest2.ContainsError())
			{
				errorList.AddRange(pollRequest2);
			}
			ErrorList pollRequest3 = StatusMessageService.Instance.GetPollRequest(out var pollRequests);
			if (pollRequest3.ContainsError())
			{
				errorList.AddRange(pollRequest3);
			}
			ErrorList pollRequest4 = TransferStatisticService.Instance.GetPollRequest(out var pollRequestList3);
			if (pollRequest4.ContainsError())
			{
				errorList.AddRange(pollRequest4);
			}
			ErrorList pollRequest5 = StoreFileService.Instance.GetPollRequest(out var pollRequestList4);
			if (pollRequest5.ContainsError())
			{
				errorList.AddRange(pollRequest5);
			}
			IFileSetService service = ServiceLocator.Instance.GetService<IFileSetService>();
			ErrorList pollRequest6 = ((IPollRequestReply)service).GetPollRequest(out var pollRequestList5);
			if (pollRequest6.ContainsError())
			{
				errorList.AddRange(pollRequest6);
			}
			pollRequestList.AddRange(pollRequestList5);
			pollRequestList.AddRange(pollRequestList4);
			pollRequestList.AddRange(pollRequestList3);
			pollRequestList.AddRange(pollRequests);
			pollRequestList.AddRange(pollRequestList2);
			ErrorList errorList2 = poll.Poll(pollRequestList, out var pollReplyList);
			if (errorList2.ContainsError())
			{
				errorList.AddRange(errorList2);
			}
			if (!suppressHealthUpdate)
			{
				ServiceLocator.Instance.GetService<IHealthService>().Update("UPDATE_SERVICE_SERVERPOLL");
			}
			if (pollReplyList != null && pollReplyList.Count > 0)
			{
				ErrorList errorList3 = ConfigFileService.Instance.ProcessPollReply((from pr in pollReplyList.ToList()
					where pr.PollReplyType == PollReplyType.UpdateConfigFiles || pr.PollReplyType == PollReplyType.ConfigFileChangeSet
					select pr).ToList());
				if (errorList3.ContainsError())
				{
					errorList.AddRange(errorList3);
				}
				ErrorList errorList4 = StoreInfoService.Instance.ProcessPollReply((from pr in pollReplyList.ToList()
					where pr.PollReplyType == PollReplyType.UpdateStoreInfo || pr.PollReplyType == PollReplyType.StoreInfoChangeSet
					select pr).ToList());
				if (errorList4.ContainsError())
				{
					errorList.AddRange(errorList4);
				}
				IDownloadFileService service2 = ServiceLocator.Instance.GetService<IDownloadFileService>();
				if (service2 != null)
				{
					errorList.AddRange(service2.ProcessPollReply(pollReplyList.Where((PollReply pr) => pr.PollReplyType == PollReplyType.DownloadFile).ToList()));
				}
				IEnumerable<PollReply> source = pollReplyList.Where((PollReply pr) => pr.PollReplyType == PollReplyType.StoreFileChangeSet);
				errorList.AddRange(StoreFileService.Instance.ProcessPollReply(source.ToList()));
				IEnumerable<PollReply> source2 = pollReplyList.Where((PollReply pr) => pr.PollReplyType == PollReplyType.FileSetChangeSet);
				errorList.AddRange(service.ProcessPollReply(source2.ToList()));
			}
		}
		finally
		{
			LogHelper.Instance.Log("Exiting PollService.ServerPoll");
		}
		return errorList;
	}

	private PollService()
	{
	}
}
