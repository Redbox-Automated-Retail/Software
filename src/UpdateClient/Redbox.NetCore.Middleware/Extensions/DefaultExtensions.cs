namespace Redbox.NetCore.Middleware.Extensions;

public static class DefaultExtensions
{
	public static bool IsNull<T>(this T value)
	{
		return value == null;
	}

	public static bool IsDefault<T>(this T value)
	{
		return object.Equals(value, default(T));
	}
}
