using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class DeployFileSet : ICommandIoTController
{
	private readonly ILogger<DeployFileSet> _logger;

	private readonly IFileSetService _service;

	public CommandEnum CommandEnum => CommandEnum.DeployFileSet;

	public int Version => 1;

	public DeployFileSet(ILogger<DeployFileSet> logger, IFileSetService service)
	{
		_logger = logger;
		_service = service;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		try
		{
			new List<Error>();
			ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet = JsonConvert.DeserializeObject<ClientFileSetRevisionChangeSet>(ioTCommand.Payload.ToJson());
			await _service.ProcessChangeSet(clientFileSetRevisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while executing command " + ioTCommand?.ToJson(), "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/DeployFileSet.cs");
		}
	}
}
