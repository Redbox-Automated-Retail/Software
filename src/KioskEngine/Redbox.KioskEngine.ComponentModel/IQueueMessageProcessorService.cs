using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IQueueMessageProcessorService
{
	string Name { get; }

	bool IsDefault { get; }

	List<string> SupportedMessageTypes { get; }

	BeforeMessageProcessingDelegate BeforeMessageProcessing { get; }

	ProcessMessageDelegate ProcessMessage { get; }

	MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully { get; }

	AfterMessageProcessedDelegate AfterMessageProcessed { get; }
}
