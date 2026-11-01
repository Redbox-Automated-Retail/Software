using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

internal class ManifestInfo : IManifestInfo
{
	public string ProductName { get; internal set; }

	public string ProductVersion { get; internal set; }

	public string StartupScript { get; internal set; }

	public string TestControl { get; internal set; }

	public string ControlAssembly { get; internal set; }

	public BundleType BundleType { get; internal set; }

	public ReadOnlyCollection<IBundleSpecifier> Requires => InnerRequires.AsReadOnly();

	private List<IBundleSpecifier> InnerRequires { get; set; }

	public ManifestInfo(IResource resource)
	{
		InnerRequires = new List<IBundleSpecifier>();
		BundleType = (BundleType)resource["bundle_type"];
		ProductName = (string)resource["product_name"];
		ProductVersion = (string)resource["product_version"];
		StartupScript = (string)resource["startup_script"];
		TestControl = (string)resource["test_control"];
		ControlAssembly = (string)resource["control_assembly"];
		if (resource["requires"] is string text)
		{
			string[] array = text.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string specifier in array)
			{
				InnerRequires.Add(new BundleSpecifier(specifier));
			}
		}
		else
		{
			if (!(resource["requires"] is List<object> list))
			{
				return;
			}
			foreach (object item in list)
			{
				InnerRequires.Add(new BundleSpecifier(item as string));
			}
		}
	}
}
