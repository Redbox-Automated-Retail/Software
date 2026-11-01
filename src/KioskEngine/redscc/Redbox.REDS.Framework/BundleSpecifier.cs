using System;

namespace Redbox.REDS.Framework;

public class BundleSpecifier : IBundleSpecifier
{
	public string Name { get; internal set; }

	public Version Version { get; internal set; }

	public BundleType Type { get; internal set; }

	public IResourceBundle Instance { get; internal set; }

	public BundleSpecifier(string specifier)
	{
		if (string.IsNullOrEmpty(specifier))
		{
			throw new ArgumentException("specifier cannot be null.");
		}
		string[] parts = specifier.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 2)
		{
			throw new ArgumentException("specifier must be in the format: product,version");
		}
		Name = parts[0];
		Version = new Version((parts[1] == "*") ? "999999.999999" : parts[1].Replace("*", "999999"));
	}

	public BundleSpecifier(string productName, string productVersion, BundleType type, IResourceBundle bundle)
		: this($"{productName},{productVersion}")
	{
		Instance = bundle;
		Type = type;
	}

	public override string ToString()
	{
		return $"Bundle Specifier: Product={Name}, Version={Version}";
	}
}
