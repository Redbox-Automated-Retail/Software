using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets;

public class StateFileService : IStateFileService
{
	private readonly IStateFileRepository _stateFileRepository;

	private readonly ILogger<StateFileService> _logger;

	public StateFileService(ILogger<StateFileService> logger, IStateFileRepository stateFileRepository)
	{
		_logger = logger;
		_stateFileRepository = stateFileRepository;
	}

	public async Task<bool> Delete(long fileSetId)
	{
		try
		{
			return await _stateFileRepository.Delete(fileSetId);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while deleting StateFile for FileSetId {fileSetId}", "Delete", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return false;
		}
	}

	public async Task<bool> DeleteInProgress(long fileSetId)
	{
		try
		{
			return await _stateFileRepository.DeleteInProgress(fileSetId);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while deleting StateFile for FileSetId {fileSetId}", "DeleteInProgress", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return false;
		}
	}

	public async Task<StateFileResponse> Save(StateFile stateFile)
	{
		try
		{
			HttpStatusCode statusCode = ((await _stateFileRepository.Save(stateFile)) ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
			return new StateFileResponse
			{
				StatusCode = statusCode,
				StateFile = stateFile
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while saving StateFile for FileSetId {stateFile?.FileSetId}", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return new StateFileResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<StateFileResponse> Get(long fileSetId)
	{
		try
		{
			(bool, StateFile) tuple = await _stateFileRepository.Get(fileSetId);
			bool success = tuple.Item1;
			StateFile stateFile = tuple.Item2;
			HttpStatusCode statusCode = ((success && stateFile != null) ? HttpStatusCode.OK : ((success && stateFile == null) ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError));
			return new StateFileResponse
			{
				StatusCode = statusCode,
				StateFile = stateFile
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while getting StateFile for FileSetId {fileSetId}", "Get", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return new StateFileResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<StateFilesResponse> GetAll()
	{
		try
		{
			(bool, List<StateFile>) tuple = await _stateFileRepository.GetAll();
			bool success = tuple.Item1;
			List<StateFile> stateFiles = tuple.Item2;
			HttpStatusCode statusCode = ((success && stateFiles.Count > 0) ? HttpStatusCode.OK : ((success && stateFiles.Count == 0) ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError));
			return new StateFilesResponse
			{
				StatusCode = statusCode,
				StateFiles = stateFiles
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting StateFiles", "GetAll", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return new StateFilesResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<StateFilesResponse> GetAllInProgress()
	{
		try
		{
			(bool, List<StateFile>) tuple = await _stateFileRepository.GetAll();
			bool success = tuple.Item1;
			List<StateFile> stateFiles = tuple.Item2;
			List<StateFile> inProgress = stateFiles.Where((StateFile x) => x.IsRevisionDownloadInProgress).ToList();
			HttpStatusCode statusCode = ((success && inProgress.Count > 0) ? HttpStatusCode.OK : ((success && inProgress.Count == 0) ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError));
			return new StateFilesResponse
			{
				StatusCode = statusCode,
				StateFiles = inProgress
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting in progress StateFiles", "GetAllInProgress", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
			return new StateFilesResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}
}
