namespace Redbox.DirectShow;

public interface IVideoSource
{
	string Source { get; }

	int FramesReceived { get; }

	bool IsRunning { get; }

	event NewFrameEventHandler NewFrame;

	event VideoSourceErrorEventHandler VideoSourceError;

	event PlayingFinishedEventHandler PlayingFinished;

	bool Start();

	void SignalToStop();

	void WaitForStop();

	void Stop();
}
