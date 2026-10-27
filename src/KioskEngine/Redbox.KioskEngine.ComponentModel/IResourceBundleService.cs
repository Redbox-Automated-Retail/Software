using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Xml;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.ComponentModel;

public interface IResourceBundleService
{
	bool IsRentalRunning { get; }

	IResourceBundleSet ActiveBundleSet { get; }

	IResourceBundle ActiveBundle { get; }

	IEnumerable<IBundleSpecifier> Bundles { get; }

	IResourceBundleFilter Filter { get; }

	string SwitchStatusMessage { get; set; }

	string CurrentBundleName { get; set; }

	string DefaultBundleName { get; set; }

	string[] SearchPath { get; set; }

	bool ShowDebugger { get; set; }

	bool SetSwitchToBundle(IResourceBundle bundle);

	bool IsPendingBundleSwitch();

	ErrorList Switch(out IResourceBundle previousBundle);

	ErrorList LoadBundles();

	ErrorList Activate(string productName, string productVersion);

	ErrorList ActivateDefaultBundle();

	ErrorList Deactivate();

	void Restart();

	Font GetFont(string resourceName, out bool resourceFound);

	bool HasActiveBundle();

	IResourceBundle GetBundle(IBundleSpecifier bundleSpecifier);

	IResource GetResource(string resourceName);

	IResource GetManifest(out IManifestInfo manifestInfo);

	ReadOnlyCollection<IResource> GetResources(string typeName);

	ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName);

	IDictionary<string, IResourceType> GetResourceTypes();

	XmlNode GetXml(string resourceName);

	byte[] GetSound(string resourceName);

	XpsDocument GetXpsDocument(string resourceName);

	string GetJsonFile(string resourceName);

	string GetScript(string resourceName);

	Image GetBitmap(string resourceName);

	BitmapImage GetBitmapImage(string resourceName);

	ReadOnlyCollection<object> ExecuteScript(string resourceName);

	void RegisterAssemblyFunctions(string name, Assembly assembly);
}
