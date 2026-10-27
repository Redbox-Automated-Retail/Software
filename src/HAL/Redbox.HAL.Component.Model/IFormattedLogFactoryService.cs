namespace Redbox.HAL.Component.Model;

public interface IFormattedLogFactoryService
{
	string LogsBasePath { get; }

	IFormattedLog NilLog { get; }

	string CreateSubpath(string subfolder);
}
