using System.Collections.Generic;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.ComponentModel;

public interface IFileSetService
{
	bool RebootRequired { get; }

	ErrorList ProcessPollReply(List<PollReply> pollReplyList);

	void MarkRebootRequired();
}
