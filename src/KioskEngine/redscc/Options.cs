using System.Collections.Generic;
using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.REDS.Compiler;

[Usage("kecc [--quiet] [-D:property=value ...] --output|-o[:|=]filename --path[:|=]bundle-path")]
public class Options
{
	[Description("Don't echo progress during resource bundle compilation.")]
	[Option]
	public bool Quiet;

	[Description("The path to the root of the bundle project.")]
	[Option(LongName = "path", Required = true)]
	public string Path;

	[Option(LongName = "output", ShortName = "o", Required = true)]
	[Description("The name of the resource bundle output by the compiler.")]
	public string OutputFile;

	[Option(ShortName = "D")]
	[Description("Define properties to inject into the compiled resource bundle.")]
	public IDictionary<string, string> Properties = new Dictionary<string, string>();

	public static Options Instance => Singleton<Options>.Instance;

	private Options()
	{
	}
}
