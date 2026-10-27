namespace Redbox.HAL.Component.Model;

public interface IDataTableDescriptor
{
	string Source { get; }

	bool ExclusiveReadWrite { get; }

	bool SupportsPooling { get; }

	bool UseTransaction { get; }
}
