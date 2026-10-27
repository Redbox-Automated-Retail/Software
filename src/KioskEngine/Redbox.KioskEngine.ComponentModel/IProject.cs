using System;
using System.Collections.ObjectModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.ComponentModel;

public interface IProject
{
	Guid Id { get; }

	string Version { get; }

	DateTime? CreatedOn { get; }

	DateTime? ModifiedOn { get; }

	ReadOnlyCollection<IBundleRef> Bundles { get; }

	ErrorList Save();

	ErrorList Build();

	ErrorList AddBundleRef(string name, string path);

	ErrorList AddBundleRef(string name, IResourceBundle bundle);

	ErrorList RemoveBundleRef(string name);

	ErrorList RemoveBundleRef(IBundleRef bundleRef);
}
