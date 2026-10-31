namespace Redbox.Extensibility.ComponentModel;

public interface IStatusService
{
	string StatusMessage { get; set; }

	ICommandBarItem StatusItem { get; set; }
}
