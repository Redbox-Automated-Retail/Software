using System;

namespace UpdateClientService.API.Services.Transfer;

[Flags]
internal enum BG_JOB_NOTIFICATION_TYPE : uint
{
	BG_NOTIFY_JOB_TRANSFERRED = 1u,
	BG_NOTIFY_JOB_ERROR = 2u,
	BG_NOTIFY_DISABLE = 4u,
	BG_NOTIFY_JOB_MODIFICATION = 8u
}
