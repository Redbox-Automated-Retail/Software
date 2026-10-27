namespace Redbox.Rental.Services.App;

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
}
