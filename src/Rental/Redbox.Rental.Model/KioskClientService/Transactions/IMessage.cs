using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public interface IMessage
{
	Guid MessageId { get; }

	long KioskId { get; set; }

	string EngineVersion { get; set; }

	string BundleVersion { get; set; }

	string MessageType { get; set; }

	bool UseMessageControl { get; }

	Dictionary<string, string> PropertyBag { get; set; }
}
