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
		string[] array = specifier.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 2)
		{
			throw new ArgumentException("specifier must be in the format: product,version");
		}
		Name = array[0];
		Version = new Version((array[1] == "*") ? "999999.999999" : array[1].Replace("*", "999999"));
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
