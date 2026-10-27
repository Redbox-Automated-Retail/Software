namespace Redbox.NetCore.Middleware.Http;

public static class BaseResponseExtensions
{
	public static T AddError<T>(this T response, Error error) where T : BaseResponse
	{
		response.Success = false;
		response.Errors.Add(error);
		return response;
	}

	public static T AddError<T>(this T response, string code, string message) where T : BaseResponse
	{
		return response.AddError(new Error
		{
			Code = code,
			Message = message
		});
	}
}
