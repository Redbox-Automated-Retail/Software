using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

public struct InstructionHelper : IDisposable
{
	private class ClientControlResponse : IControlResponse
	{
		public bool Success { get; internal set; }

		public bool TimedOut { get; internal set; }

		public bool CommError { get; internal set; }

		public string Diagnostic { get; internal set; }

		internal ClientControlResponse()
		{
			Diagnostic = string.Empty;
		}
	}

	private const int DefaultTimeout = 120000;

	private readonly HardwareService Service;

	public void Dispose()
	{
	}

	public InstructionHelper(HardwareService s)
	{
		this = default(InstructionHelper);
		Service = s;
	}

	public HardwareJob Execute(string instruction)
	{
		return Execute(instruction, 120000);
	}

	public HardwareJob Execute(string instruction, int timeout)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate(instruction, timeout, out job);
		hardwareCommandResult.Dump();
		if (!hardwareCommandResult.Success)
		{
			return null;
		}
		return job;
	}

	public ErrorCodes ExecuteErrorCode(string instruction)
	{
		return ExecuteErrorCode(instruction, 120000);
	}

	public ErrorCodes ExecuteErrorCode(string instruction, int timeout)
	{
		string value = ExecuteGeneric(instruction, timeout);
		if (string.IsNullOrEmpty(value))
		{
			return ErrorCodes.ServiceChannelError;
		}
		return Enum<ErrorCodes>.ParseIgnoringCase(value, ErrorCodes.ServiceChannelError);
	}

	public IControlResponse ExecuteWithResponse(string instruction)
	{
		return ExecuteWithResponse(instruction, 120000);
	}

	public IControlResponse ExecuteWithResponse(string instruction, int timeout)
	{
		string serviceResponse = ExecuteGeneric(instruction, timeout);
		return From(serviceResponse);
	}

	public string ExecuteGeneric(string instruction)
	{
		return ExecuteGeneric(instruction, 120000);
	}

	public string ExecuteGeneric(string instruction, int timeout)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate(instruction, timeout, out job);
		hardwareCommandResult.Dump();
		if (!hardwareCommandResult.Success)
		{
			return string.Empty;
		}
		return job.GetTopOfStack();
	}

	private IControlResponse From(string serviceResponse)
	{
		ClientControlResponse clientControlResponse = new ClientControlResponse();
		if (string.IsNullOrEmpty(serviceResponse))
		{
			clientControlResponse.Diagnostic = ErrorCodes.ServiceChannelError.ToString().ToUpper();
			clientControlResponse.CommError = true;
		}
		else if (serviceResponse.Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase))
		{
			clientControlResponse.Success = true;
		}
		else if (serviceResponse.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase))
		{
			clientControlResponse.TimedOut = true;
		}
		else
		{
			bool commError = (clientControlResponse.TimedOut = true);
			clientControlResponse.CommError = commError;
			clientControlResponse.Diagnostic = serviceResponse;
		}
		return clientControlResponse;
	}
}
