using System;

namespace Redbox.UpdateManager.BITS;

[Flags]
public enum PEER_CACHING_FLAGS : uint
{
	BG_JOB_ENABLE_PEERCACHING_CLIENT = 1u,
	BG_JOB_ENABLE_PEERCACHING_SERVER = 2u
}
