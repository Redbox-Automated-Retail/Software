using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeReaderFactory : IBarcodeReaderFactory
{
	private readonly IBarcodeReader NilReader = new NilReader();

	private ClearImageReaderV2 m_clearImageReader;

	private CortexDecoder m_cortexDecoder;

	private IBarcodeReader Reader;

	public string ImagePath => BarcodeConfiguration.Instance.WorkingPath;

	public IBarcodeReader GetConfiguredReader()
	{
		return Reader;
	}

	public void Initialize(ErrorList errors)
	{
		SetConfiguredReader();
		if (Reader == null)
		{
			errors.Add(Error.NewError("D001", "No reader found", "Unable to find a licensed reader"));
		}
	}

	public BarcodeReaderFactory()
	{
		LogHelper.Instance.Log("Barcode Reader Factory - constructor.");
	}

	private void SetConfiguredReader()
	{
		LogHelper.Instance.Log("Set Configured Reader -- get cortex decoder");
		IBarcodeReader barcodeReader = GetCortexDecoder();
		if (barcodeReader == null || !barcodeReader.IsLicensed)
		{
			LogHelper.Instance.Log("Set Configured Reader -- get inlite reader");
			barcodeReader = GetInliteReader();
		}
		if (barcodeReader == null)
		{
			barcodeReader = NilReader;
		}
		Reader = barcodeReader;
		LogHelper.Instance.Log("[BarcodeReaderFactory] Configured reader {0} {1} licensed.", Reader.Service, Reader.IsLicensed ? "is" : "is not");
	}

	private CortexDecoder GetCortexDecoder()
	{
		if (m_cortexDecoder == null)
		{
			m_cortexDecoder = new CortexDecoder();
		}
		return m_cortexDecoder;
	}

	private ClearImageReaderV2 GetInliteReader()
	{
		if (m_clearImageReader == null)
		{
			ClearImageReaderV2 clearImageReaderV = new ClearImageReaderV2();
			if (clearImageReaderV.IsLicensed)
			{
				m_clearImageReader = clearImageReaderV;
			}
		}
		return m_clearImageReader;
	}
}
