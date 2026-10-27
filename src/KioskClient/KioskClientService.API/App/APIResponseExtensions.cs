namespace KioskClientService.API.App;

public static class APIResponseExtensions
{
	public static APIResponse<T> CopyTo<T>(this APIResponse<T> apiResponse, BaseResponse baseResponse)
	{
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

	public static void CopyTo<T>(this APIResponse<T> apiResponse, ApiBaseResponse baseResponse)
	{
		baseResponse.Error = apiResponse.Errors?.ToJson();
		baseResponse.StatusCode = apiResponse.StatusCode;
	}
}
