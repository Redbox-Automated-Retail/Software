using System;

namespace Redbox.REDS.Framework;

public interface IBundleSpecifier
{
	string Name { get; }

	Version Version { get; }

	BundleType Type { get; }

	IResourceBundle Instance { get; }
}
