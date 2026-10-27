using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[TypeLibType(16)]
[ComVisible(false)]
public delegate void _DVideoCapEvents_MouseMoveEventHandler(short Button, short Shift, [ComAliasName("stdole.OLE_XPOS_PIXELS")] int x, [ComAliasName("stdole.OLE_YPOS_PIXELS")] int y);
