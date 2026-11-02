// Written for the modernize branch (not decompiled). Compiled for .NET 10 only.
#if !NETFRAMEWORK
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Redbox.Core;

// What .NET Framework gave every Redbox process for free and .NET 10 does not.
public static class NetFrameworkCompatibility
{
	// .NET (Core) knows only UTF-* and ASCII until the code-page provider is registered. VistaDB (code page 1252) and
	// the ANSI code page used where .NET Framework code relied on Encoding.Default need it. Runs when Redbox.Core loads.
	[ModuleInitializer]
	internal static void RegisterCodePages()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	// Encoding.Default was the system ANSI code page on .NET Framework and is UTF-8 on .NET 10. Code whose bytes must stay
	// the same (hashes, encrypted values) uses this instead.
	public static Encoding AnsiCodePage => Encoding.GetEncoding(0);

	// Whether a file is a .NET assembly, from its PE header alone. Folder scans call this before Assembly.LoadFrom: on .NET,
	// LoadFrom maps a file that is not IL-only with the system loader before it finds there is no .NET metadata, which runs a
	// native DLL's start-up code (one of the ClearImage DLLs in halservice\bin crashes there). .NET Framework never ran it.
	public static bool IsNetAssembly(string path)
	{
		try
		{
			using FileStream stream = File.OpenRead(path);
			using System.Reflection.PortableExecutable.PEReader reader = new System.Reflection.PortableExecutable.PEReader(stream);
			return reader.HasMetadata;
		}
		catch (Exception)
		{
			return false;
		}
	}

	// .NET 10 reads <app>.dll.config, not <app>.exe.config, and rejects sections it has no declaration for (machine.config
	// declared system.serviceModel, startup, runtime, ... on .NET Framework). Kiosks, the update store and rb-reprovision
	// keep editing <app>.exe.config, so copy it to <app>.dll.config with only what .NET 10 can read: appSettings,
	// connectionStrings and the sections it declares itself. Call first thing in Main, before any configuration is read.
	public static void ImportExeConfig(Action<string>? log = null)
	{
		try
		{
			string? processPath = Environment.ProcessPath;
			// Where System.Configuration.ConfigurationManager looks on .NET: APP_CONFIG_FILE, else <entry assembly>.config.
			string? target = AppContext.GetData("APP_CONFIG_FILE") as string ?? (System.Reflection.Assembly.GetEntryAssembly()?.Location is { Length: > 0 } entry ? entry + ".config" : null);
			if (string.IsNullOrEmpty(processPath) || string.IsNullOrEmpty(target))
			{
				return;
			}
			string source = processPath + ".config";
			if (!File.Exists(source) || string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			XDocument document = XDocument.Load(source, LoadOptions.PreserveWhitespace);
			XElement? configuration = document.Root;
			if (configuration == null || configuration.Name.LocalName != "configuration")
			{
				return;
			}
			var declared = configuration.Element("configSections")?.Descendants().Where(e => e.Name.LocalName is "section" or "sectionGroup").Select(e => (string?)e.Attribute("name")).OfType<string>().ToHashSet(StringComparer.Ordinal) ?? new System.Collections.Generic.HashSet<string>();
			foreach (XElement section in configuration.Elements().ToList())
			{
				string name = section.Name.LocalName;
				if (name is not ("configSections" or "appSettings" or "connectionStrings") && !declared.Contains(name))
				{
					section.Remove();
				}
			}
			string text = document.Declaration + document.ToString(SaveOptions.DisableFormatting);
			if (File.Exists(target) && File.ReadAllText(target) == text)
			{
				return;
			}
			File.WriteAllText(target, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			log?.Invoke($"Imported {Path.GetFileName(source)} into {Path.GetFileName(target)} for .NET.");
		}
		catch (Exception e)
		{
			log?.Invoke($"Could not import the .exe.config for .NET: {e.Message}");
		}
	}
}
#endif
