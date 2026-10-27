namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IReconcileResult
{
	bool Success { get; set; }

	int StatusCode { get; set; }

	string CustomerProfileNumber { get; set; }
}
