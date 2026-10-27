using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
				resourceMan = new ResourceManager("Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap error => (Bitmap)ResourceManager.GetObject("error", resourceCulture);

	internal static Bitmap error2 => (Bitmap)ResourceManager.GetObject("error2", resourceCulture);

	internal static Bitmap pending => (Bitmap)ResourceManager.GetObject("pending", resourceCulture);

	internal static Bitmap running => (Bitmap)ResourceManager.GetObject("running", resourceCulture);

	internal static Bitmap running1 => (Bitmap)ResourceManager.GetObject("running1", resourceCulture);

	internal static Bitmap Spinner32x32 => (Bitmap)ResourceManager.GetObject("Spinner32x32", resourceCulture);

	internal static Bitmap sucess => (Bitmap)ResourceManager.GetObject("sucess", resourceCulture);

	internal Resources()
	{
	}
}
