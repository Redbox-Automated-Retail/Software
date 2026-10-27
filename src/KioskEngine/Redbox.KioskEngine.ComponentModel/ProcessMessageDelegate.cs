using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public delegate bool ProcessMessageDelegate(IMessage message, out IDictionary<string, object> response);
