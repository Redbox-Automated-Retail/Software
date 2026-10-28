namespace Redbox.HAL.Core;

public interface IClearImageReader : IBarcodeReader
{
	bool Init(int data);
}
