using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class PollReplyDTO
{
	public PollReplyType PRT { get; set; }

	public int SId { get; set; }

	public string D { get; set; }

	public static List<PollReplyDTO> GetPollReplyDTOList(List<PollReply> list)
	{
		List<PollReplyDTO> list2 = new List<PollReplyDTO>();
		foreach (PollReply item in list)
		{
			list2.Add(GetPollReplyDTO(item));
		}
		return list2;
	}

	public static PollReplyDTO GetPollReplyDTO(PollReply pollReply)
	{
		return new PollReplyDTO
		{
			PRT = pollReply.PollReplyType,
			SId = pollReply.SyncId,
			D = pollReply.Data
		};
	}

	public static List<PollReply> GetPollReplyList(List<PollReplyDTO> list)
	{
		List<PollReply> list2 = new List<PollReply>();
		foreach (PollReplyDTO item in list)
		{
			PollReply pollReply = new PollReply();
			list2.Add(pollReply);
			pollReply.PollReplyType = item.PRT;
			pollReply.SyncId = item.SId;
			pollReply.Data = item.D;
		}
		return list2;
	}
}
