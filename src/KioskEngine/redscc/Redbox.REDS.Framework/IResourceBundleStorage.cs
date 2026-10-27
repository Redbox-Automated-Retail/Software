using System;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public interface IResourceBundleStorage : IDisposable
{
	bool SupportsSave { get; }

	string BundlePath { get; }

	void Open(string path);

	void Close();

	void Create(string path, string trimPrefix);

	void AddEntry(string path, string trimPrefix);

	void BeginUpdate();

	void CommitUpdate();

	byte[] GetEntryContent(string path);

	IResourceBundleStorageEntry GetEntry(string path);

	byte[] GetEntryContent(IResourceBundleStorageEntry entry);

	ReadOnlyCollection<IResourceBundleStorageEntry> GetResourceManifest();
}
