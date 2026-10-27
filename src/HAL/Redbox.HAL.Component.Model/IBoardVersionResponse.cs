namespace Redbox.HAL.Component.Model;

public interface IBoardVersionResponse
{
	bool ReadSuccess { get; }

	string Version { get; }

	string BoardName { get; }
}
