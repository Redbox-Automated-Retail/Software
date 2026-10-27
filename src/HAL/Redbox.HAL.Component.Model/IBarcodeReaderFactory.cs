namespace Redbox.HAL.Component.Model;

public interface IBarcodeReaderFactory
{
	string ImagePath { get; }

	IBarcodeReader GetConfiguredReader();

	void Initialize(ErrorList errors);
}
