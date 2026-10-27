using Microsoft.AspNetCore.Mvc;

namespace Redbox.NetCore.Middleware.Http;

public static class APIResponseExtensions
{
	public static APIResponse<T> CopyTo<T>(this APIResponse<T> apiResponse, BaseResponse baseResponse)
	{
		if (apiResponse == null)
		{
			baseResponse.Success = false;
			baseResponse.StatusCode = 500;
		}
		baseResponse.Success = apiResponse.IsSuccessStatusCode;
		apiResponse.Errors.ForEach(delegate(Error x)
		{
			baseResponse.Errors.Add(new Error
			{
				Code = x.Code,
				Message = x.Message
			});
		});
		baseResponse.StatusCode = (int)apiResponse.StatusCode;
		baseResponse.ResponseTime = apiResponse.ResponseTime;
		return apiResponse;
	}

	public static ObjectResult ToObjectResult<T>(this APIResponse<T> api)
	{
		return new ObjectResult(api.Response)
		{
			StatusCode = (int)api.StatusCode
		};
	}

	public static ContentResult ToContentResult<T>(this APIResponse<T> api)
	{
		return new ContentResult
		{
			Content = (string)api.Scrub(),
			StatusCode = (int)api.StatusCode
		};
	}
}
