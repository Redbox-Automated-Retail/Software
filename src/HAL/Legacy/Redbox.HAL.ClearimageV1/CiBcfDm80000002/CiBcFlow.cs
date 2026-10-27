using System.Runtime.InteropServices;

namespace CiBcfDm80000002;

[ComImport]
[Guid("CA8DCBED-AEFF-496F-9EB6-EC00534964E0")]
[CoClass(typeof(CiBcFlowClass))]
[ComVisible(true)]
public interface CiBcFlow : _CiBcFlow, __CiBcFlow_Event
{
}
