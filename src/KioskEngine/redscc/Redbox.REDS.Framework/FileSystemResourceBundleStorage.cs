using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Redbox.REDS.Framework;

public class FileSystemResourceBundleStorage : IResourceBundleStorage, IDisposable
{
	public bool SupportsSave => true;

	public string BundlePath { get; internal set; }

	public void Dispose()
	{
	}

	public void Open(string path)
	{
		BundlePath = path;
	}

	public void Close()
	{
	}

	public void Create(string path, string trimPrefix)
	{
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			BundlePath = path;
		}
		catch
		{
		}
	}

	public void AddEntry(string path, string trimPrefix)
	{
	}

	public void BeginUpdate()
	{
	}

	public void CommitUpdate()
	{
	}

	public byte[] GetEntryContent(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		string path2 = Path.Combine(BundlePath, path);
		if (File.Exists(path2))
		{
			return File.ReadAllBytes(path2);
		}
		return null;
	}

	public IResourceBundleStorageEntry GetEntry(string path)
	{
		string text = Path.Combine(BundlePath, path);
		if (!File.Exists(text))
		{
			return null;
		}
		FileInfo fileInfo = new FileInfo(text);
		ResourceBundleStorageEntry resourceBundleStorageEntry = new ResourceBundleStorageEntry();
		resourceBundleStorageEntry.Name = fileInfo.Name;
		resourceBundleStorageEntry.Size = fileInfo.Length;
		resourceBundleStorageEntry.Path = fileInfo.FullName;
		return resourceBundleStorageEntry;
	}

	public byte[] GetEntryContent(IResourceBundleStorageEntry entry)
	{
		return GetEntryContent(entry.Path);
	}

	public ReadOnlyCollection<IResourceBundleStorageEntry> GetResourceManifest()
	{
		List<IResourceBundleStorageEntry> list = new List<IResourceBundleStorageEntry>();
		string[] files = Directory.GetFiles(BundlePath, "*.*", SearchOption.AllDirectories);
		foreach (string fileName in files)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			list.Add(new ResourceBundleStorageEntry
			{
				Name = fileInfo.Name,
				Size = fileInfo.Length,
				Path = fileInfo.FullName
			});
		}
		return list.AsReadOnly();
	}
}
