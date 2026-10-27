using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[TypeLibType(16)]
[ComVisible(false)]
public delegate void _DVideoCapEvents_CapturingEventHandler(int lFrame, int lDropFrame);
