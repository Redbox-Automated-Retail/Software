using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IMessage
{
	int ID { get; }

	string GUID { get; }

	byte[] Data { get; set; }

	string Type { get; }

	byte Priority { get; }

	DateTime CreatedOn { get; }
}
