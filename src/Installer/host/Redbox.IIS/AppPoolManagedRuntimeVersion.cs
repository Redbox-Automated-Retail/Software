namespace Redbox.IIS;

internal static class AppPoolManagedRuntimeVersion
{
	public const string Net11 = "v1.1";

	public const string Net20 = "v2.0";

	public const string Net35 = "v3.5";

	public const string Net40 = "v4.0";

	public static string ToAppPoolManagedRuntimeVersion(this AspNetVersion version)
	{
		return version switch
		{
			AspNetVersion.AspNet11 => "v1.1", 
			AspNetVersion.AspNet20 => "v2.0", 
			AspNetVersion.AspNet35 => "v3.5", 
			AspNetVersion.AspNet40 => "v4.0", 
			_ => "v2.0", 
		};
	}

	public static AspNetVersion ToAspNetVersion(this string managedRuntimeVersion)
	{
		return managedRuntimeVersion switch
		{
			"v1.1" => AspNetVersion.AspNet11, 
			"v2.0" => AspNetVersion.AspNet20, 
			"v3.5" => AspNetVersion.AspNet35, 
			"v4.0" => AspNetVersion.AspNet40, 
			_ => AspNetVersion.AspNet20, 
		};
	}
}
