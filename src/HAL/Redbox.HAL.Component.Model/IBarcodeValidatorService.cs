namespace Redbox.HAL.Component.Model;

public interface IBarcodeValidatorService
{
	bool IsValid(string barcode);
}
