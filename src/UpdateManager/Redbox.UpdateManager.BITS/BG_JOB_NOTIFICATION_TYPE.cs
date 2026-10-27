using System;

namespace Redbox.UpdateManager.BITS;

[Flags]
public enum BG_JOB_NOTIFICATION_TYPE : uint
{
	BG_NOTIFY_JOB_TRANSFERRED = 1u,
	BG_NOTIFY_JOB_ERROR = 2u,
	BG_NOTIFY_DISABLE = 4u,
	BG_NOTIFY_JOB_MODIFICATION = 8u
}
