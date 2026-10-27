using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComVisible(false)]
public delegate void _DVideoCapEvents_CapturingEventHandler(int lFrame, int lDropFrame);
