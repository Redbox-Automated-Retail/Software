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
		List<string> list = new List<string>(Directory.GetFiles(Options.Path, "*.resource"));
		string text = list.Find((string each) => string.Compare("manifest.resource", Path.GetFileName(each), ignoreCase: true) == 0);
		if (text == null)
		{
			errorList.Add(Error.NewError("C004", "The manifest.resource file is missing from the bundle.", "Provide a valid manifest.resource file and recompile."));
			return errorList;
		}
		bool flag = false;
		if (Options.Properties.Count > 0)
		{
			defaultConsole.Write($"Update manifest.resource properties ... ");
			list.Remove(text);
			string text2 = Path.Combine(Path.GetTempPath(), "manifest.resource");
			File.Copy(text, text2, overwrite: true);
			list.Add(text2);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text2);
			if (xmlDocument.DocumentElement != null)
			{
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
				if (xmlNodeList != null)
				{
					foreach (XmlNode item in xmlNodeList)
					{
						string attributeValue = item.GetAttributeValue<string>("name");
						if (Options.Properties.ContainsKey(attributeValue))
						{
							item.SetAttributeValue("value", Options.Properties[attributeValue]);
							flag = true;
						}
					}
				}
				xmlDocument.Save(text2);
			}
			defaultConsole.WriteLine("done.");
		}
		foreach (string item2 in list)
		{
			try
			{
				defaultConsole.Write($"Verify resource file: {Path.GetFileName(item2)} ... ");
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.Load(item2);
				defaultConsole.WriteLine("done.");
			}
			catch (Exception e)
			{
				defaultConsole.WriteLine("errored.");
				errorList.Add(Error.NewError("C999", $"Unable to parse the resource XML file: {item2}.", e));
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
			foreach (string item3 in list)
			{
				if (Path.GetFileName(item3) == "manifest.resource" && flag)
				{
					resourceBundle.AddEntry(item3, Path.GetDirectoryName(item3));
				}
				else
				{
					resourceBundle.AddEntry(item3);
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
			ErrorList errorList2 = resourceBundle2.LoadResources();
			if (errorList2.Count > 0)
			{
				defaultConsole.WriteLine("errored.");
				foreach (Error item4 in errorList2)
				{
					errorList.Add(item4.IsWarning ? Error.NewWarning(item4.Code, item4.Description, item4.Details) : Error.NewError(item4.Code, item4.Description, item4.Details));
				}
			}
			else
			{
				using (LuaVirtualMachine luaVirtualMachine = new LuaVirtualMachine())
				{
					foreach (IResource allResource in resourceBundle2.GetAllResources("script"))
					{
						string text3 = string.Empty;
						object content = allResource.GetAspect("content").GetContent();
						if (content is string)
						{
							text3 = (string)content;
						}
						else if (content is byte[])
						{
							text3 = Encoding.ASCII.GetString((byte[])content);
						}
						if (!string.IsNullOrEmpty(text3))
						{
							string text4 = Path.GetTempFileName().Escaped();
							try
							{
								File.WriteAllText(text4, text3);
								luaVirtualMachine.DoString($"assert( loadfile( \"{text4}\" ) )");
							}
							catch (Exception ex)
							{
								errorList.Add(Error.NewError("C671", $"Lua compile failed for resource: {allResource.Name}", ex.Message));
							}
							finally
							{
								File.Delete(text4);
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
