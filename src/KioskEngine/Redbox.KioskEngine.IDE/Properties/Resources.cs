using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.KioskEngine.IDE.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
				resourceMan = new ResourceManager("Redbox.KioskEngine.IDE.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap Bundle => (Bitmap)ResourceManager.GetObject("Bundle", resourceCulture);

	internal static Bitmap CallStack => (Bitmap)ResourceManager.GetObject("CallStack", resourceCulture);

	internal static Bitmap Clean => (Bitmap)ResourceManager.GetObject("Clean", resourceCulture);

	internal static Bitmap Copy => (Bitmap)ResourceManager.GetObject("Copy", resourceCulture);

	internal static Bitmap Critical => (Bitmap)ResourceManager.GetObject("Critical", resourceCulture);

	internal static Bitmap Cross => (Bitmap)ResourceManager.GetObject("Cross", resourceCulture);

	internal static Bitmap Cube => (Bitmap)ResourceManager.GetObject("Cube", resourceCulture);

	internal static Bitmap Cut => (Bitmap)ResourceManager.GetObject("Cut", resourceCulture);

	internal static Bitmap DebugBreakpoint => (Bitmap)ResourceManager.GetObject("DebugBreakpoint", resourceCulture);

	internal static Bitmap DebugBreakpointClear => (Bitmap)ResourceManager.GetObject("DebugBreakpointClear", resourceCulture);

	internal static Bitmap Debugger => (Bitmap)ResourceManager.GetObject("Debugger", resourceCulture);

	internal static Bitmap DebugPause => (Bitmap)ResourceManager.GetObject("DebugPause", resourceCulture);

	internal static Bitmap DebugPlay => (Bitmap)ResourceManager.GetObject("DebugPlay", resourceCulture);

	internal static Bitmap DebugStepInto => (Bitmap)ResourceManager.GetObject("DebugStepInto", resourceCulture);

	internal static Bitmap DebugStepOut => (Bitmap)ResourceManager.GetObject("DebugStepOut", resourceCulture);

	internal static Bitmap DebugStepOver => (Bitmap)ResourceManager.GetObject("DebugStepOver", resourceCulture);

	internal static Bitmap DebugStop => (Bitmap)ResourceManager.GetObject("DebugStop", resourceCulture);

	internal static Bitmap Delete => (Bitmap)ResourceManager.GetObject("Delete", resourceCulture);

	internal static Bitmap Dependencies => (Bitmap)ResourceManager.GetObject("Dependencies", resourceCulture);

	internal static Bitmap ErrorList => (Bitmap)ResourceManager.GetObject("ErrorList", resourceCulture);

	internal static Bitmap Execute => (Bitmap)ResourceManager.GetObject("Execute", resourceCulture);

	internal static Bitmap Filter => (Bitmap)ResourceManager.GetObject("Filter", resourceCulture);

	internal static Bitmap Folder => (Bitmap)ResourceManager.GetObject("Folder", resourceCulture);

	internal static Bitmap FolderClosed => (Bitmap)ResourceManager.GetObject("FolderClosed", resourceCulture);

	internal static Bitmap ImmediateWindow => (Bitmap)ResourceManager.GetObject("ImmediateWindow", resourceCulture);

	internal static Bitmap Information => (Bitmap)ResourceManager.GetObject("Information", resourceCulture);

	internal static Bitmap Minus => (Bitmap)ResourceManager.GetObject("Minus", resourceCulture);

	internal static Bitmap NewDocument => (Bitmap)ResourceManager.GetObject("NewDocument", resourceCulture);

	internal static Bitmap Open => (Bitmap)ResourceManager.GetObject("Open", resourceCulture);

	internal static Bitmap Output => (Bitmap)ResourceManager.GetObject("Output", resourceCulture);

	internal static Bitmap PageSetup => (Bitmap)ResourceManager.GetObject("PageSetup", resourceCulture);

	internal static Bitmap Paste => (Bitmap)ResourceManager.GetObject("Paste", resourceCulture);

	internal static Bitmap Plus => (Bitmap)ResourceManager.GetObject("Plus", resourceCulture);

	internal static Bitmap Print => (Bitmap)ResourceManager.GetObject("Print", resourceCulture);

	internal static Bitmap Properties => (Bitmap)ResourceManager.GetObject("Properties", resourceCulture);

	internal static Bitmap Redo => (Bitmap)ResourceManager.GetObject("Redo", resourceCulture);

	internal static Bitmap Refresh => (Bitmap)ResourceManager.GetObject("Refresh", resourceCulture);

	internal static Bitmap Save => (Bitmap)ResourceManager.GetObject("Save", resourceCulture);

	internal static Bitmap SaveAll => (Bitmap)ResourceManager.GetObject("SaveAll", resourceCulture);

	internal static Bitmap SaveAs => (Bitmap)ResourceManager.GetObject("SaveAs", resourceCulture);

	internal static Bitmap Sitemap => (Bitmap)ResourceManager.GetObject("Sitemap", resourceCulture);

	internal static Bitmap Timers => (Bitmap)ResourceManager.GetObject("Timers", resourceCulture);

	internal static Bitmap Tools => (Bitmap)ResourceManager.GetObject("Tools", resourceCulture);

	internal static Bitmap Undo => (Bitmap)ResourceManager.GetObject("Undo", resourceCulture);

	internal static Bitmap Views => (Bitmap)ResourceManager.GetObject("Views", resourceCulture);

	internal static Bitmap Warning => (Bitmap)ResourceManager.GetObject("Warning", resourceCulture);

	internal Resources()
	{
	}
}
