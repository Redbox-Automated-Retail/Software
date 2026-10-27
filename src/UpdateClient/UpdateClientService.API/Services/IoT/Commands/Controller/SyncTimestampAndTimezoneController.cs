using System.Threading.Tasks;
using UpdateClientService.API.Services.Kernel;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class SyncTimestampAndTimezoneController : ICommandIoTController
{
	private readonly IKernelService _kernelService;

	public CommandEnum CommandEnum => CommandEnum.SyncTimestampAndTimezone;

	public int Version => 1;

	public SyncTimestampAndTimezoneController(IKernelService kernelService)
	{
		_kernelService = kernelService;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		_kernelService.SyncTimeAndTimezone(ioTCommand.Payload);
	}
}
