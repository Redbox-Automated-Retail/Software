using System;

namespace Redbox.HAL.Client;

public delegate void HardwareEvent(HardwareJob job, DateTime eventTime, string eventMessage);
