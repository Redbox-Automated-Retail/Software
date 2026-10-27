using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public delegate void MessageProcessedSuccessfullyDelegate(IMessage message, IDictionary<string, object> result, object clientData);
