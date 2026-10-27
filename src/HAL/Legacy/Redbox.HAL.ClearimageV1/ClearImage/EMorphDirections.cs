using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EMorphDirections
{
	ciMorphHorz = 1,
	ciMorphVert = 2,
	ciMorphDiag = 4,
	ciMorphHorzAndVert = 3,
	ciMorphAllNeighbours = 7
}
