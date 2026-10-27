using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.HardwareServices.KernelExtension.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Redbox.HardwareServices.KernelExtension.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap Clean => (Bitmap)ResourceManager.GetObject("Clean", resourceCulture);

	internal static Bitmap Edit => (Bitmap)ResourceManager.GetObject("Edit", resourceCulture);

	internal static Bitmap Execute => (Bitmap)ResourceManager.GetObject("Execute", resourceCulture);

	internal static Bitmap Minus => (Bitmap)ResourceManager.GetObject("Minus", resourceCulture);

	internal static Bitmap Open => (Bitmap)ResourceManager.GetObject("Open", resourceCulture);

	internal static Bitmap Plus => (Bitmap)ResourceManager.GetObject("Plus", resourceCulture);

	internal static Bitmap Refresh => (Bitmap)ResourceManager.GetObject("Refresh", resourceCulture);

	internal static Bitmap SaveAs => (Bitmap)ResourceManager.GetObject("SaveAs", resourceCulture);

	internal Resources()
	{
	}
}
