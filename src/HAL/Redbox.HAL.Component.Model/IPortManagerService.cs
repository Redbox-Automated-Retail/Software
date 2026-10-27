using System;
using System.IO.Ports;

namespace Redbox.HAL.Component.Model;

public interface IPortManagerService
{
	bool Register(ICommPort port);

	void Dispose(ICommPort port);

	ICommChannelConfiguration CreateConfiguration();

	ICommPort Create(SerialPort port);

	ICommPort Create(SerialPort port, CommPortReadModes mode);

	ICommPort Scan(ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode);

	ICommPort Scan(string tryFirst, ICommChannelConfiguration conf, Predicate<ICommPort> probe, CommPortReadModes mode);
}
