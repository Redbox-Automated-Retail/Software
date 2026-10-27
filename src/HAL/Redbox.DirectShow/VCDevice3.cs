using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class VCDevice3 : IVideoSource
{
	private readonly List<Bitmap> ReceivedFrames = new List<Bitmap>();

	private readonly ManualResetEvent StartEvent = new ManualResetEvent(initialState: false);

	private string deviceMoniker;

	private int framesReceived;

	private VideoCapabilities videoResolution;

	private VideoCapabilities snapshotResolution;

	private Thread thread;

	private ManualResetEvent stopEvent;

	private VideoCapabilities[] videoCapabilities;

	private bool needToSetVideoInput;

	private object sourceObject;

	private readonly int GrabWaitTime;

	private readonly bool Debug;

	private readonly object sync = new object();

	private readonly object m_listLock = new object();

	private bool? isCrossbarAvailable;

	private VideoInput[] crossbarVideoInputs;

	private VideoInput crossbarVideoInput = VideoInput.Default;

	private static Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new Dictionary<string, VideoCapabilities[]>();

	private static Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new Dictionary<string, VideoInput[]>();

	public VideoInput CrossbarVideoInput
	{
		get
		{
			return crossbarVideoInput;
		}
		set
		{
			needToSetVideoInput = true;
			crossbarVideoInput = value;
		}
	}

	public VideoInput[] AvailableCrossbarVideoInputs
	{
		get
		{
			if (crossbarVideoInputs == null)
			{
				lock (cacheCrossbarVideoInputs)
				{
					if (!string.IsNullOrEmpty(deviceMoniker) && cacheCrossbarVideoInputs.ContainsKey(deviceMoniker))
					{
						crossbarVideoInputs = cacheCrossbarVideoInputs[deviceMoniker];
					}
				}
				if (crossbarVideoInputs == null)
				{
					if (!IsRunning)
					{
						WorkerThread(runGraph: false);
					}
					else
					{
						for (int i = 0; i < 500; i++)
						{
							if (crossbarVideoInputs != null)
							{
								break;
							}
							Thread.Sleep(10);
						}
					}
				}
			}
			if (crossbarVideoInputs == null)
			{
				return new VideoInput[0];
			}
			return crossbarVideoInputs;
		}
	}

	public virtual string Source => deviceMoniker;

	public int FramesReceived
	{
		get
		{
			int result = framesReceived;
			framesReceived = 0;
			return result;
		}
	}

	public bool IsRunning
	{
		get
		{
			if (thread != null)
			{
				if (!thread.Join(0))
				{
					return true;
				}
				Free();
			}
			return false;
		}
	}

	[Obsolete]
	public Size DesiredFrameSize
	{
		get
		{
			return Size.Empty;
		}
		set
		{
		}
	}

	[Obsolete]
	public Size DesiredSnapshotSize
	{
		get
		{
			return Size.Empty;
		}
		set
		{
		}
	}

	[Obsolete]
	public int DesiredFrameRate
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public VideoCapabilities VideoResolution
	{
		get
		{
			return videoResolution;
		}
		set
		{
			videoResolution = value;
		}
	}

	public VideoCapabilities SnapshotResolution
	{
		get
		{
			return snapshotResolution;
		}
		set
		{
			snapshotResolution = value;
		}
	}

	public VideoCapabilities[] VideoCapabilities
	{
		get
		{
			if (videoCapabilities == null)
			{
				lock (cacheVideoCapabilities)
				{
					if (!string.IsNullOrEmpty(deviceMoniker) && cacheVideoCapabilities.ContainsKey(deviceMoniker))
					{
						videoCapabilities = cacheVideoCapabilities[deviceMoniker];
					}
				}
				if (videoCapabilities == null)
				{
					if (!IsRunning)
					{
						WorkerThread(runGraph: false);
					}
					else
					{
						for (int i = 0; i < 500; i++)
						{
							if (videoCapabilities != null)
							{
								break;
							}
							Thread.Sleep(10);
						}
					}
				}
			}
			if (videoCapabilities == null)
			{
				return new VideoCapabilities[0];
			}
			return videoCapabilities;
		}
	}

	public object SourceObject => sourceObject;

	private bool Triggered { get; set; }

	public event NewFrameEventHandler SnapshotFrame;

	public event NewFrameEventHandler NewFrame
	{
		add
		{
		}
		remove
		{
		}
	}

	public event VideoSourceErrorEventHandler VideoSourceError;

	public event PlayingFinishedEventHandler PlayingFinished;

	public VCDevice3(string deviceMoniker, int snapTimeout, bool debug)
	{
		this.deviceMoniker = deviceMoniker;
		GrabWaitTime = snapTimeout;
		Debug = debug;
	}

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		if (string.IsNullOrEmpty(deviceMoniker))
		{
			LogHelper.Instance.Log("[DirectShow] Video source is not specified - cannot start graph.");
			return false;
		}
		framesReceived = 0;
		isCrossbarAvailable = null;
		needToSetVideoInput = true;
		stopEvent = new ManualResetEvent(initialState: false);
		lock (sync)
		{
			thread = new Thread(WorkerThread);
			thread.Name = deviceMoniker;
			thread.Start();
		}
		if (!StartEvent.WaitOne(10000))
		{
			LogHelper.Instance.Log("timed out waiting for start event.");
			return false;
		}
		return true;
	}

	public void SignalToStop()
	{
		if (thread != null)
		{
			stopEvent.Set();
		}
	}

	public void WaitForStop()
	{
		if (thread != null)
		{
			thread.Join();
			Free();
		}
	}

	public void Stop()
	{
		if (IsRunning)
		{
			thread.Abort();
			WaitForStop();
		}
	}

	private void Free()
	{
		thread = null;
		stopEvent.Close();
		stopEvent = null;
	}

	public bool CheckIfCrossbarAvailable()
	{
		lock (sync)
		{
			if (!isCrossbarAvailable.HasValue)
			{
				if (!IsRunning)
				{
					WorkerThread(runGraph: false);
				}
				else
				{
					for (int i = 0; i < 500; i++)
					{
						if (isCrossbarAvailable.HasValue)
						{
							break;
						}
						Thread.Sleep(10);
					}
				}
			}
			return isCrossbarAvailable.HasValue && isCrossbarAvailable.Value;
		}
	}

	public void SimulateTrigger()
	{
		if (Debug)
		{
			LogHelper.Instance.Log("[DirectShow] Simulate trigger.");
		}
		Triggered = true;
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		SampleGrabber sampleGrabber = new SampleGrabber(OnNewFrame);
		object obj = null;
		object obj2 = null;
		object obj3 = null;
		object retInterface = null;
		ICaptureGraphBuilder2 captureGraphBuilder = null;
		IFilterGraph2 filterGraph = null;
		IBaseFilter baseFilter = null;
		IBaseFilter baseFilter2 = null;
		ISampleGrabber sampleGrabber2 = null;
		IMediaControl mediaControl = null;
		IMediaEventEx mediaEventEx = null;
		IAMCrossbar iAMCrossbar = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[DirectShow] Failed creating capture graph builder");
				return;
			}
			obj = Activator.CreateInstance(typeFromCLSID);
			captureGraphBuilder = (ICaptureGraphBuilder2)obj;
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("Failed creating filter graph");
				return;
			}
			obj2 = Activator.CreateInstance(typeFromCLSID);
			filterGraph = (IFilterGraph2)obj2;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				LogHelper.Instance.Log("[DirectShow] Failed creating device object for moniker");
				return;
			}
			baseFilter = (IBaseFilter)sourceObject;
			try
			{
				_ = (IAMVideoControl)sourceObject;
			}
			catch
			{
			}
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[DirectShow] Failed creating sample grabber");
				return;
			}
			obj3 = Activator.CreateInstance(typeFromCLSID);
			sampleGrabber2 = (ISampleGrabber)obj3;
			baseFilter2 = (IBaseFilter)obj3;
			filterGraph.AddFilter(baseFilter, "source");
			filterGraph.AddFilter(baseFilter2, "grabber_video");
			AMMediaType aMMediaType = new AMMediaType();
			aMMediaType.MajorType = MediaType.Video;
			aMMediaType.SubType = MediaSubType.RGB24;
			sampleGrabber2.SetMediaType(aMMediaType);
			captureGraphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, baseFilter, typeof(IAMCrossbar).GUID, out retInterface);
			if (retInterface != null)
			{
				iAMCrossbar = (IAMCrossbar)retInterface;
			}
			isCrossbarAvailable = iAMCrossbar != null;
			crossbarVideoInputs = ColletCrossbarVideoInputs(iAMCrossbar);
			sampleGrabber2.SetBufferSamples(bufferThem: false);
			sampleGrabber2.SetOneShot(oneShot: false);
			sampleGrabber2.SetCallback(sampleGrabber, 1);
			GetPinCapabilitiesAndConfigureSizeAndRate(captureGraphBuilder, baseFilter, PinCategory.Capture, videoResolution, ref videoCapabilities);
			lock (cacheVideoCapabilities)
			{
				if (videoCapabilities != null && !cacheVideoCapabilities.ContainsKey(deviceMoniker))
				{
					cacheVideoCapabilities.Add(deviceMoniker, videoCapabilities);
				}
			}
			if (runGraph)
			{
				captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, baseFilter, null, baseFilter2);
				if (sampleGrabber2.GetConnectedMediaType(aMMediaType) == 0)
				{
					VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(aMMediaType.FormatPtr, typeof(VideoInfoHeader));
					sampleGrabber.Size = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
					aMMediaType.Dispose();
				}
				mediaControl = (IMediaControl)obj2;
				mediaEventEx = (IMediaEventEx)obj2;
				mediaControl.Run();
				StartEvent.Set();
				LogHelper.Instance.Log("[DirectShow] graph is running; avg rate = {0}.", videoResolution.AverageFrameRate);
				do
				{
					if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) == 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
						if (Debug)
						{
							LogHelper.Instance.Log("[DirectShow]Event: {0}", lEventCode.ToString());
						}
						if (lEventCode == DsEvCode.DeviceLost)
						{
							reason = ReasonToFinishPlaying.DeviceLost;
							break;
						}
					}
					if (needToSetVideoInput)
					{
						needToSetVideoInput = false;
						if (isCrossbarAvailable.Value)
						{
							SetCurrentCrossbarInput(iAMCrossbar, crossbarVideoInput);
							crossbarVideoInput = GetCurrentCrossbarInput(iAMCrossbar);
						}
					}
					if (!Triggered)
					{
						continue;
					}
					lock (m_listLock)
					{
						ReceivedFrames.Clear();
					}
					sampleGrabber.Grab = true;
					Thread.Sleep(GrabWaitTime);
					Triggered = false;
					sampleGrabber.Grab = false;
					if (Debug)
					{
						LogHelper.Instance.Log("There are {0} frames in the list.", ReceivedFrames.Count);
					}
					lock (m_listLock)
					{
						if (ReceivedFrames.Count > 0 && SnapshotFrame != null)
						{
							SnapshotFrame(this, new NewFrameEventArgs(ReceivedFrames[ReceivedFrames.Count - 1]));
						}
						else
						{
							LogHelper.Instance.Log("[DirectShow] There are no captured frames from the device.");
							if (mediaEventEx != null && mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
							{
								mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
								LogHelper.Instance.Log(">> [DirectShow]Event: {0}", lEventCode.ToString());
							}
						}
						foreach (Bitmap receivedFrame in ReceivedFrames)
						{
							receivedFrame.Dispose();
						}
						ReceivedFrames.Clear();
					}
				}
				while (!stopEvent.WaitOne(100, exitContext: false));
				mediaControl.Stop();
				if (Debug)
				{
					LogHelper.Instance.Log("there were {0} frames received via video.", framesReceived);
				}
			}
		}
		catch (Exception ex)
		{
			if (VideoSourceError != null)
			{
				VideoSourceError(this, new VideoSourceErrorEventArgs(ex.Message));
			}
		}
		finally
		{
			captureGraphBuilder = null;
			filterGraph = null;
			baseFilter = null;
			mediaControl = null;
			mediaEventEx = null;
			iAMCrossbar = null;
			baseFilter2 = null;
			sampleGrabber2 = null;
			if (obj2 != null)
			{
				Marshal.ReleaseComObject(obj2);
				obj2 = null;
			}
			if (sourceObject != null)
			{
				Marshal.ReleaseComObject(sourceObject);
				sourceObject = null;
			}
			if (obj3 != null)
			{
				Marshal.ReleaseComObject(obj3);
				obj3 = null;
			}
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
				obj = null;
			}
			if (retInterface != null)
			{
				Marshal.ReleaseComObject(retInterface);
				retInterface = null;
			}
		}
		if (PlayingFinished != null)
		{
			PlayingFinished(this, reason);
		}
	}

	private void SetResolution(IAMStreamConfig streamConfig, VideoCapabilities resolution)
	{
		if (resolution == null)
		{
			return;
		}
		int count = 0;
		int size = 0;
		AMMediaType mediaType = null;
		VideoStreamConfigCaps streamConfigCaps = new VideoStreamConfigCaps();
		streamConfig.GetNumberOfCapabilities(out count, out size);
		for (int i = 0; i < count; i++)
		{
			try
			{
				VideoCapabilities videoCapabilities = new VideoCapabilities(streamConfig, i);
				if (resolution == videoCapabilities && streamConfig.GetStreamCaps(i, out mediaType, streamConfigCaps) == 0)
				{
					break;
				}
			}
			catch
			{
			}
		}
		if (mediaType != null)
		{
			streamConfig.SetFormat(mediaType);
			mediaType.Dispose();
		}
	}

	private void GetPinCapabilitiesAndConfigureSizeAndRate(ICaptureGraphBuilder2 graphBuilder, IBaseFilter baseFilter, Guid pinCategory, VideoCapabilities resolutionToSet, ref VideoCapabilities[] capabilities)
	{
		graphBuilder.FindInterface(pinCategory, MediaType.Video, baseFilter, typeof(IAMStreamConfig).GUID, out var retInterface);
		if (retInterface != null)
		{
			IAMStreamConfig iAMStreamConfig = null;
			try
			{
				iAMStreamConfig = (IAMStreamConfig)retInterface;
			}
			catch (InvalidCastException)
			{
			}
			if (iAMStreamConfig != null)
			{
				if (capabilities == null)
				{
					try
					{
						capabilities = Redbox.DirectShow.VideoCapabilities.FromStreamConfig(iAMStreamConfig);
					}
					catch
					{
					}
				}
				if (resolutionToSet != null)
				{
					SetResolution(iAMStreamConfig, resolutionToSet);
				}
			}
		}
		if (capabilities == null)
		{
			capabilities = new VideoCapabilities[0];
		}
	}

	private VideoInput[] ColletCrossbarVideoInputs(IAMCrossbar crossbar)
	{
		lock (cacheCrossbarVideoInputs)
		{
			if (cacheCrossbarVideoInputs.ContainsKey(deviceMoniker))
			{
				return cacheCrossbarVideoInputs[deviceMoniker];
			}
			List<VideoInput> list = new List<VideoInput>();
			if (crossbar != null && crossbar.get_PinCounts(out var _, out var inputPinCount) == 0)
			{
				for (int i = 0; i < inputPinCount; i++)
				{
					if (crossbar.get_CrossbarPinInfo(isInputPin: true, i, out var _, out var physicalType) == 0 && physicalType < PhysicalConnectorType.AudioTuner)
					{
						list.Add(new VideoInput(i, physicalType));
					}
				}
			}
			VideoInput[] array = new VideoInput[list.Count];
			list.CopyTo(array);
			cacheCrossbarVideoInputs.Add(deviceMoniker, array);
			return array;
		}
	}

	private VideoInput GetCurrentCrossbarInput(IAMCrossbar crossbar)
	{
		VideoInput result = VideoInput.Default;
		if (crossbar.get_PinCounts(out var outputPinCount, out var _) == 0)
		{
			int num = -1;
			int pinIndexRelated;
			for (int i = 0; i < outputPinCount; i++)
			{
				if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out var physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
				{
					num = i;
					break;
				}
			}
			if (num != -1 && crossbar.get_IsRoutedTo(num, out var inputPinIndex) == 0)
			{
				crossbar.get_CrossbarPinInfo(isInputPin: true, inputPinIndex, out pinIndexRelated, out var physicalType2);
				result = new VideoInput(inputPinIndex, physicalType2);
			}
		}
		return result;
	}

	private void SetCurrentCrossbarInput(IAMCrossbar crossbar, VideoInput videoInput)
	{
		if (videoInput.Type == PhysicalConnectorType.Default || crossbar.get_PinCounts(out var outputPinCount, out var inputPinCount) != 0)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		int pinIndexRelated;
		PhysicalConnectorType physicalType;
		for (int i = 0; i < outputPinCount; i++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
			{
				num = i;
				break;
			}
		}
		for (int j = 0; j < inputPinCount; j++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: true, j, out pinIndexRelated, out physicalType) == 0 && physicalType == videoInput.Type && j == videoInput.Index)
			{
				num2 = j;
				break;
			}
		}
		if (num2 != -1 && num != -1 && crossbar.CanRoute(num, num2) == 0)
		{
			crossbar.Route(num, num2);
		}
	}

	private void OnNewFrame(Bitmap image)
	{
		framesReceived++;
		if (!Monitor.TryEnter(m_listLock))
		{
			return;
		}
		try
		{
			if (!stopEvent.WaitOne(0, exitContext: false) && Triggered)
			{
				ReceivedFrames.Add((Bitmap)image.Clone());
			}
		}
		finally
		{
			Monitor.Exit(m_listLock);
		}
	}

	private void OnSnapshotFrame(Bitmap image)
	{
		if (SnapshotFrame != null && !stopEvent.WaitOne(0, exitContext: false))
		{
			SnapshotFrame(this, new NewFrameEventArgs(image));
		}
	}
}
