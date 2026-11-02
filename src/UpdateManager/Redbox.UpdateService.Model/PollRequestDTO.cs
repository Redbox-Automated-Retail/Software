using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class PollRequestDTO
{
	public PollRequestType PRT { get; set; }

	public int SId { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string D { get; set; } = null!;

	public static List<PollRequestDTO> GetPollReplyDTOList(List<PollRequest> list)
	{
		List<PollRequestDTO> converted = new List<PollRequestDTO>();
		foreach (PollRequest item in list)
		{
			PollRequestDTO pollRequestDTO = new PollRequestDTO();
			converted.Add(pollRequestDTO);
			pollRequestDTO.PRT = item.PollRequestType;
			pollRequestDTO.SId = item.SyncId;
			pollRequestDTO.D = item.Data;
		}
		return converted;
	}

	public static List<PollRequest> GetPollRequestList(List<PollRequestDTO> list)
	{
		List<PollRequest> converted = new List<PollRequest>();
		foreach (PollRequestDTO item in list)
		{
			PollRequest pollRequest = new PollRequest();
			converted.Add(pollRequest);
			pollRequest.PollRequestType = item.PRT;
			pollRequest.SyncId = item.SId;
			pollRequest.Data = item.D;
		}
		return converted;
	}
}
