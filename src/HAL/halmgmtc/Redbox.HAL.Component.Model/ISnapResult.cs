namespace Redbox.HAL.Component.Model;

public interface ISnapResult
{
	bool SnapOk { get; }

	string Path { get; }
}
