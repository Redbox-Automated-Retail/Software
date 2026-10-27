using System;

namespace HALUtilities;

[Flags]
internal enum SyncOptions
{
	None = 0,
	Debug = 1,
	Analyze = 2,
	Attach = 4,
	OutputSuccessfulSyncCompare = 8,
	AsyncSoftSync = 0x10,
	All = Debug | Analyze | Attach | OutputSuccessfulSyncCompare | AsyncSoftSync
}
