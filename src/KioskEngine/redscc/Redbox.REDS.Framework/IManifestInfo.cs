using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public interface IManifestInfo
{
	string ProductName { get; }

	string ProductVersion { get; }

	string StartupScript { get; }

	string TestControl { get; }

	string ControlAssembly { get; }

	BundleType BundleType { get; }

	ReadOnlyCollection<IBundleSpecifier> Requires { get; }
}
