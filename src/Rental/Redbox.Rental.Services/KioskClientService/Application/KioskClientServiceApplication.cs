using System;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Application;

namespace Redbox.Rental.Services.KioskClientService.Application;

public class KioskClientServiceApplication : BaseKioskClientService, IKioskClientServiceApplication
{
	private const string _apiPath = "api/application";

	private readonly IViewService _viewService;

	public KioskClientServiceApplication()
	{
		_viewService = ServiceLocator.Instance.GetService<IViewService>();
		if (_viewService == null)
		{
			return;
		}
		_viewService.ViewStateChanged += delegate(object o, ViewStateChangedArgs e)
		{
			Task.Run(() => PostViewChange(e));
		};
	}

	public async Task<bool> Start()
	{
		WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/application/start"), null, "POST").ConfigureAwait(continueOnCapturedContext: false);
		LogResponse("KioskClientServiceApplication.Start", webResult);
		return webResult?.Response?.Success == true;
	}

	public async Task<bool> End()
	{
		WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/application/end"), null, "POST").ConfigureAwait(continueOnCapturedContext: false);
		LogResponse("KioskClientServiceApplication.End", webResult);
		return webResult?.Response?.Success == true;
	}

	private async Task<bool> PostViewChangeAsync(ViewStateChangedArgs e)
	{
		if (e.ToView != null && (e.Type == ViewStateChangedType.Pop || e.Type == ViewStateChangedType.Push))
		{
			WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/application/viewchangedevent"), e, "POST").ConfigureAwait(continueOnCapturedContext: false);
			LogResponse("KioskClientServiceApplication.PostViewChangedEventAsync", webResult);
			return webResult?.Response?.Success == true;
		}
		return false;
	}

	private bool PostViewChange(ViewStateChangedArgs e)
	{
		if (e.ToView != null && (e.Type == ViewStateChangedType.Pop || e.Type == ViewStateChangedType.Push))
		{
			WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/application/viewchangedevent"), e, "POST");
			LogResponse("KioskClientServiceApplication.PostViewChangedEvent", webResult);
			return webResult?.Response?.Success == true;
		}
		return false;
	}
}
