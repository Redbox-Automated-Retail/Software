using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.KioskEngine.Environment.Properties;

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
				resourceMan = new ResourceManager("Redbox.KioskEngine.Environment.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap Authentication => (Bitmap)ResourceManager.GetObject("Authentication", resourceCulture);

	internal static Bitmap DatabaseSchema => (Bitmap)ResourceManager.GetObject("DatabaseSchema", resourceCulture);

	internal static Bitmap Health => (Bitmap)ResourceManager.GetObject("Health", resourceCulture);

	internal static Bitmap Inventory => (Bitmap)ResourceManager.GetObject("Inventory", resourceCulture);

	internal static Bitmap MerchantService => (Bitmap)ResourceManager.GetObject("MerchantService", resourceCulture);

	internal static Bitmap Promotion => (Bitmap)ResourceManager.GetObject("Promotion", resourceCulture);

	internal static Bitmap QueueService => (Bitmap)ResourceManager.GetObject("QueueService", resourceCulture);

	internal static Bitmap Reservation => (Bitmap)ResourceManager.GetObject("Reservation", resourceCulture);

	internal static Bitmap ShoppingSession => (Bitmap)ResourceManager.GetObject("ShoppingSession", resourceCulture);

	internal static Bitmap Store => (Bitmap)ResourceManager.GetObject("Store", resourceCulture);

	internal Resources()
	{
	}
}
