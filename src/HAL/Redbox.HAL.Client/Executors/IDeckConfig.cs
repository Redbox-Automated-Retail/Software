namespace Redbox.HAL.Client.Executors;

public interface IDeckConfig
{
	int Number { get; }

	int SlotCount { get; }

	bool IsQlm { get; }
}
