using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.ComponentModel;

public interface IStatusMessageService
{
	ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string subKey, string description, string data);

	ErrorList EnqueueMessage(StatusMessage.StatusMessageType type, string key, string description, string data);
}
