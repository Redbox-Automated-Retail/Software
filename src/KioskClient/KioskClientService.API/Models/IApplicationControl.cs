namespace KioskClientService.API.Models;

public interface IApplicationControl
{
	bool CanShutDown(ShutDownReason shutDownReason);

	bool ShutDown(bool forceShutdown, ShutDownReason shutDownReason = ShutDownReason.None);

	void SetCanShutDownClientResponse(bool clientAllowsShutDown);
}
