using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Serilog;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API;

public static class ResetEventExtensions
{
	public static async Task<bool> WaitAsyncAndDisposeOnFinish(this AsyncManualResetEvent mre, TimeSpan cancelAfter, IoTCommandModel request = null)
	{
		CancellationTokenSource tokenSource = new CancellationTokenSource(cancelAfter);
		bool isCancellationRequested;
		try
		{
			await mre.WaitAsync(tokenSource.Token);
		}
		catch (Exception exception)
		{
			string text = ((request != null) ? (" for request id " + request?.RequestId) : null);
			Log.Logger.Error(exception, "ResetEventExtensions.WaitAsyncAndDisposeOnFinish -> Error occured while waiting" + text);
		}
		finally
		{
			isCancellationRequested = tokenSource.IsCancellationRequested;
			tokenSource.Dispose();
		}
		return isCancellationRequested;
	}
}
