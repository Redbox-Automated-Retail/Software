using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilReader : AbstractBarcodeReader
{
	protected override void OnScan(ScanResult sr)
	{
		sr.ResetOnException();
	}

	internal NilReader()
		: base(BarcodeServices.None)
	{
	}
}
