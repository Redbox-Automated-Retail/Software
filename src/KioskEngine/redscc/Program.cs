using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.REDS.Compiler;

public class Program : IDisposable
{
	private static CommandLine m_commandLine;

	public static Options Options => Singleton<Options>.Instance;

	public static CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Options);
			}
			return m_commandLine;
		}
	}

	public static int Main(string[] args)
	{
		using Program program = new Program();
		return program.Run();
	}

	public void Dispose()
	{
		CommandLine.Dispose();
	}

	public int Run()
	{
		if (CommandLine.Errors.Count > 0 && !CommandLine.HelpRequested)
		{
			CommandLine.WriteUsage(includeHelp: false);
			return 1;
		}
		if (CommandLine.HelpRequested)
		{
			return 0;
		}
		using (IConsole console = new DefaultConsole())
		{
			ErrorList errorList = Compile(console);
			if (errorList.Count > 0)
			{
				console.WriteLine("\nThe compilation failed for the following reasons:\n");
				foreach (Error item in errorList)
				{
					console.WriteLine(item.ToString());
					console.WriteLine(item.Details);
					console.WriteLine();
				}
				return 1;
			}
		}
		return 0;
	}

	internal ErrorList Compile(IConsole defaultConsole)
	{
		Options.Path = Path.GetFullPath(Options.Path);
		Options.OutputFile = Path.GetFullPath(Options.OutputFile);
		ErrorList errorList = new ErrorList();
		if (!Directory.Exists(Options.Path))
		{
			errorList.Add(Error.NewError("C003", $"The specified bundle project path '{Options.Path}' does not exist.", "Specify a valid bundle project path."));
			return errorList;
		}
		List<string> resourceFiles = new List<string>(Directory.GetFiles(Options.Path, "*.resource"));
		string manifestPath = resourceFiles.Find((string each) => string.Compare("manifest.resource", Path.GetFileName(each), ignoreCase: true) == 0);
		if (manifestPath == null)
		{
			errorList.Add(Error.NewError("C004", "The manifest.resource file is missing from the bundle.", "Provide a valid manifest.resource file and recompile."));
			return errorList;
		}
		bool propertiesUpdated = false;
		if (Options.Properties.Count > 0)
		{
			defaultConsole.Write($"Update manifest.resource properties ... ");
			resourceFiles.Remove(manifestPath);
			string tempManifestPath = Path.Combine(Path.GetTempPath(), "manifest.resource");
			File.Copy(manifestPath, tempManifestPath, overwrite: true);
			resourceFiles.Add(tempManifestPath);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(tempManifestPath);
			if (xmlDocument.DocumentElement != null)
			{
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
				if (xmlNodeList != null)
				{
					foreach (XmlNode propertyNode in xmlNodeList)
					{
						string attributeValue = propertyNode.GetAttributeValue<string>("name");
						if (Options.Properties.ContainsKey(attributeValue))
						{
							propertyNode.SetAttributeValue("value", Options.Properties[attributeValue]);
							propertiesUpdated = true;
						}
					}
				}
				xmlDocument.Save(tempManifestPath);
			}
			defaultConsole.WriteLine("done.");
		}
		foreach (string resourceFile in resourceFiles)
		{
			try
			{
				defaultConsole.Write($"Verify resource file: {Path.GetFileName(resourceFile)} ... ");
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(resourceFile);
				defaultConsole.WriteLine("done.");
			}
			catch (Exception e)
			{
				defaultConsole.WriteLine("errored.");
				errorList.Add(Error.NewError("C999", $"Unable to parse the resource XML file: {resourceFile}.", e));
			}
		}
		defaultConsole.Write($"Write resource bundle file: {Options.OutputFile} ... ");
		string directoryName = Path.GetDirectoryName(Options.OutputFile);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using (ResourceBundle resourceBundle = ResourceBundle.Create(Options.OutputFile, Options.Path, ResourceBundleStorageType.Zipped))
		{
			if (resourceBundle == null)
			{
				errorList.Add(Error.NewError("C120", $"Unable to create the resource bundle file {Options.OutputFile}.", "The file may be locked or the path may not be valid."));
				return errorList;
			}
			resourceBundle.BeginUpdate();
			foreach (string resourceFile in resourceFiles)
			{
				if (Path.GetFileName(resourceFile) == "manifest.resource" && propertiesUpdated)
				{
					resourceBundle.AddEntry(resourceFile, Path.GetDirectoryName(resourceFile));
				}
				else
				{
					resourceBundle.AddEntry(resourceFile);
				}
			}
			string[] files = Directory.GetFiles(Options.Path, "*.*", SearchOption.AllDirectories);
			foreach (string path in files)
			{
				string extension = Path.GetExtension(path);
				if (!(extension == ".resource") && !(extension == ".build"))
				{
					resourceBundle.AddEntry(path);
				}
			}
			resourceBundle.CommitUpdate();
			defaultConsole.WriteLine("done.");
		}
		using (ResourceBundle resourceBundle2 = new ResourceBundle(Options.OutputFile, ResourceBundleStorageType.Zipped))
		{
			defaultConsole.Write($"Verify resource bundle file: {Options.OutputFile} ... ");
			ErrorList loadErrors = resourceBundle2.LoadResources();
			if (loadErrors.Count > 0)
			{
				defaultConsole.WriteLine("errored.");
				foreach (Error error in loadErrors)
				{
					errorList.Add(error.IsWarning ? Error.NewWarning(error.Code, error.Description, error.Details) : Error.NewError(error.Code, error.Description, error.Details));
				}
			}
			else
			{
				using (LuaVirtualMachine luaVirtualMachine = new LuaVirtualMachine())
				{
					foreach (IResource allResource in resourceBundle2.GetAllResources("script"))
					{
						string scriptSource = string.Empty;
						object content = allResource.GetAspect("content").GetContent();
						if (content is string)
						{
							scriptSource = (string)content;
						}
						else if (content is byte[])
						{
							scriptSource = Encoding.ASCII.GetString((byte[])content);
						}
						if (!string.IsNullOrEmpty(scriptSource))
						{
							string tempScriptPath = Path.GetTempFileName().Escaped();
							try
							{
								File.WriteAllText(tempScriptPath, scriptSource);
								luaVirtualMachine.DoString($"assert( loadfile( \"{tempScriptPath}\" ) )");
							}
							catch (Exception ex)
							{
								errorList.Add(Error.NewError("C671", $"Lua compile failed for resource: {allResource.Name}", ex.Message));
							}
							finally
							{
								File.Delete(tempScriptPath);
							}
						}
						else
						{
							errorList.Add(Error.NewWarning("C672", $"Script resource '{allResource.Name}' contains no content aspect.", "Scripts with a missing content aspect will not have any affect at runtime."));
						}
					}
				}
				defaultConsole.WriteLine("done.");
			}
		}
		return errorList;
	}
}
