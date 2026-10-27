namespace Redbox.HAL.Component.Model;

public interface IConfigurationFile
{
	SystemConfigurations Type { get; }

	string Path { get; }

	string FileName { get; }

	string FullSourcePath { get; }

	void ImportFrom(IConfigurationFile config, ErrorList errors);

	ConversionResult ConvertTo(KioskConfiguration newConfig, ErrorList errors);
}
