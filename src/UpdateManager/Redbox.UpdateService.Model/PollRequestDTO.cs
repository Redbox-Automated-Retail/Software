using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class PollRequestDTO
{
	public PollRequestType PRT { get; set; }

	public int SId { get; set; }

	public string D { get; set; }

	public static List<PollRequestDTO> GetPollReplyDTOList(List<PollRequest> list)
	{
		List<PollRequestDTO> list2 = new List<PollRequestDTO>();
		foreach (PollRequest item in list)
		{
			PollRequestDTO pollRequestDTO = new PollRequestDTO();
			list2.Add(pollRequestDTO);
			pollRequestDTO.PRT = item.PollRequestType;
			pollRequestDTO.SId = item.SyncId;
			pollRequestDTO.D = item.Data;
		}
		return list2;
	}

	public static List<PollRequest> GetPollRequestList(List<PollRequestDTO> list)
	{
		List<PollRequest> list2 = new List<PollRequest>();
		foreach (PollRequestDTO item in list)
		{
			PollRequest pollRequest = new PollRequest();
			list2.Add(pollRequest);
			pollRequest.PollRequestType = item.PRT;
			pollRequest.SyncId = item.SId;
			pollRequest.Data = item.D;
		}
		return list2;
	}
}
