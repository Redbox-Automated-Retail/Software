using Redbox.HAL.Client;

namespace HALUtilities;

internal class CortexDecode
{
	// Image, Matrix and Location are set by CortexDecoderFile's object initializer.
	internal string Image = null!;

	internal string Matrix = null!;

	internal int MatrixCount;

	internal int SecureCount;

	internal Location Location = null!;
}
