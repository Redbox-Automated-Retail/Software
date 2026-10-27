namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IKioskClientServiceResult
{
	bool Success { get; }

	int StatusCode { get; }

	Error Error { get; }
}
