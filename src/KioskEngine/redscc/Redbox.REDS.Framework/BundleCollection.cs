using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework;

public sealed class BundleCollection
{
	private readonly OrderedMultiDictionary<string, IBundleSpecifier> m_bundles = new OrderedMultiDictionary<string, IBundleSpecifier>(allowDuplicateValues: false, (string lhs, string rhs) => string.Compare(lhs, rhs, ignoreCase: true), (IBundleSpecifier lhs, IBundleSpecifier rhs) => lhs.Version.CompareTo(rhs.Version));

	public int Count => m_bundles.Count;

	public void Clear()
	{
		foreach (KeyValuePair<string, ICollection<IBundleSpecifier>> bundle in m_bundles)
		{
			foreach (IBundleSpecifier item in bundle.Value)
			{
				if (item.Instance != null)
				{
					item.Instance.Dispose();
				}
			}
		}
		m_bundles.Clear();
	}

	public ErrorList AddBundleVersion(IResourceBundle bundle)
	{
		ErrorList errorList = new ErrorList();
		if (bundle == null)
		{
			errorList.Add(Error.NewError("R901", "The bundle parameter cannot be null.", "Ensure a valid bundle instance is passed to the BundleCollection.AddBundle method."));
			return errorList;
		}
		IResource manifest = bundle.GetManifest(bundle.CreateFilter(), out var manifestInfo);
		if (manifest != null && manifestInfo != null)
		{
			try
			{
				m_bundles.Add(manifestInfo.ProductName, new BundleSpecifier(manifestInfo.ProductName, manifestInfo.ProductVersion, manifestInfo.BundleType, bundle));
			}
			catch (Exception e)
			{
				errorList.Add(Error.NewError("R999", "An unhandled exception was raised in BundleCollection.AddBundle.", e));
			}
		}
		return errorList;
	}

	public ErrorList RemoveBundleVersion(IResourceBundle bundle)
	{
		return new ErrorList();
	}

	public void RemoveAllVersions(string productName)
	{
		m_bundles.Remove(productName);
	}

	public ErrorList GetBundle(string productName, string productVersion, out IResourceBundle bundle, out IManifestInfo manifestInfo)
	{
		Console.WriteLine("REDS.BundleCollection.GetBundle(" + productName + ", " + productVersion + ")");
		bundle = null;
		manifestInfo = null;
		ErrorList errorList = new ErrorList();
		ICollection<IBundleSpecifier> collection = m_bundles[productName];
		if (collection == null || collection.Count == 0)
		{
			Console.WriteLine("> found NO bundles named " + productName);
			errorList.Add(Error.NewError("B125", $"Found NO bundles named: {productName}", "Check the bundles folder under the Kiosk Engine folder and ensure the bundle is actually installed."));
			return errorList;
		}
		List<IBundleSpecifier> list = new List<IBundleSpecifier>(collection);
		Console.WriteLine("> found [" + list.Count + "] bundles named " + productName);
		BundleSpecifier targetVersion = new BundleSpecifier($"{productName},{productVersion}");
		IBundleSpecifier bundleSpecifier = list.Find((IBundleSpecifier wv) => wv.Name == targetVersion.Name && wv.Version == targetVersion.Version);
		if (bundleSpecifier != null)
		{
			bundle = bundleSpecifier.Instance;
		}
		else
		{
			list.Add(targetVersion);
			list.Sort((IBundleSpecifier lhs, IBundleSpecifier rhs) => lhs.Version.CompareTo(rhs.Version));
			int num = list.IndexOf(targetVersion);
			bundle = ((num == 0) ? null : list[num - 1].Instance);
		}
		if (bundle != null)
		{
			IResource manifest = bundle.GetManifest(bundle.CreateFilter(), out manifestInfo);
			if (manifest == null || manifestInfo == null)
			{
				errorList.Add(Error.NewError("B120", $"No manifest resource was found for the bundle: {bundle.Storage.BundlePath}.", "The bundle may not be compiled correctly or the manifest.resource file was moved or deleted after the bundle was scanned."));
			}
		}
		else
		{
			errorList.Add(Error.NewError("B130", $"The requested bundle was not found: {productName},{productVersion}", "Check the bundles folder under the Kiosk Engine folder and ensure the bundle is actually installed."));
		}
		return errorList;
	}

	public IEnumerable<string> GetBundles()
	{
		return m_bundles.Keys;
	}

	public IEnumerable<IBundleSpecifier> GetAllBundleVersions()
	{
		return m_bundles.Values;
	}

	public IEnumerable<IBundleSpecifier> GetBundleVersions(string productName)
	{
		return m_bundles[productName];
	}
}
