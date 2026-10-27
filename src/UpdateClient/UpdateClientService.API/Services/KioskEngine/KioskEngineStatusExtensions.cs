using Microsoft.AspNetCore.Mvc;

namespace UpdateClientService.API.Services.KioskEngine;

public static class KioskEngineStatusExtensions
{
	public static ObjectResult ToObjectResult(this KioskEngineStatus status)
	{
		ObjectResult objectResult = new ObjectResult(status);
		switch (status)
		{
		case KioskEngineStatus.Running:
			objectResult.StatusCode = 200;
			break;
		case KioskEngineStatus.Stopped:
			objectResult.StatusCode = 503;
			break;
		default:
			objectResult.StatusCode = 500;
			break;
		}
		return objectResult;
	}
}
