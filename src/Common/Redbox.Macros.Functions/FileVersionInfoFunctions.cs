using System;
using System.Diagnostics;

namespace Redbox.Macros.Functions;

[FunctionSet("fileversioninfo", "Version")]
public class FileVersionInfoFunctions : FunctionSetBase
{
	public FileVersionInfoFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-version-info")]
	public FileVersionInfo GetVersionInfo(string fileName)
	{
		return FileVersionInfo.GetVersionInfo(fileName);
	}

	[Function("get-company-name")]
	public static string GetCompanyName(FileVersionInfo fileVersionInfo)
	{
		return fileVersionInfo.CompanyName;
	}

	[Function("get-file-version")]
	public static Version GetFileVersion(FileVersionInfo fileVersionInfo)
	{
		return new Version(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
	}

	[Function("get-product-name")]
	public static string GetProductName(FileVersionInfo fileVersionInfo)
	{
		return fileVersionInfo.ProductName;
	}

	[Function("get-product-version")]
	public static Version GetProductVersion(FileVersionInfo fileVersionInfo)
	{
		return new Version(fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.ProductPrivatePart);
	}
}
