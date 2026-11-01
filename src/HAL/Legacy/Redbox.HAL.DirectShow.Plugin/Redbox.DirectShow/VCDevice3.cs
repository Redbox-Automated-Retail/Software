using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Core;

namespace Redbox.DirectShow;

public class VCDevice3 : IVideoSource
{
	private readonly List<Bitmap> ReceivedFrames = new List<Bitmap>();

	private readonly ManualResetEvent StartEvent = new ManualResetEvent(initialState: false);

	private string deviceMoniker;

	private int framesReceived;

	private long bytesReceived;

	private VideoCapabilities videoResolution;

	private VideoCapabilities snapshotResolution;

	private bool provideSnapshots;

	private Thread thread;

	private ManualResetEvent stopEvent;

	private VideoCapabilities[] videoCapabilities;

	private bool needToSetVideoInput;

	private bool m_simulateTrigger;

	private object sourceObject;

	private readonly int GrabWaitTime;

	private readonly object sync = new object();

	private readonly object m_listLock = new object();

	private bool? isCrossbarAvailable = null;

	private bool? m_debug = null;

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

	public bool ProvideSnapshots
	{
		get
		{
			return provideSnapshots;
		}
		set
		{
			provideSnapshots = value;
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

	public long BytesReceived
	{
		get
		{
			long result = bytesReceived;
			bytesReceived = 0L;
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

	private bool Debug
	{
		get
		{
			if (!m_debug.HasValue)
			{
				m_debug = RegistryOptions.Instance.GetSecretValue<bool>("SOFTWARE\\Redbox\\HAL", "DebugDS", false);
			}
			return m_debug.Value;
		}
	}

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

	public VCDevice3(string deviceMoniker, int snapTimeout)
	{
		this.deviceMoniker = deviceMoniker;
		GrabWaitTime = snapTimeout;
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
		bytesReceived = 0L;
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
		m_simulateTrigger = true;
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		SampleGrabber sampleGrabber = new SampleGrabber(OnNewFrame);
		object captureGraphObject = null;
		object graphObject = null;
		object grabberObject = null;
		object retInterface = null;
		ICaptureGraphBuilder2 captureGraphBuilder = null;
		IFilterGraph2 filterGraph = null;
		IBaseFilter sourceBase = null;
		IBaseFilter grabberBase = null;
		ISampleGrabber sampleGrabberInterface = null;
		IMediaControl mediaControl = null;
		IMediaEventEx mediaEventEx = null;
		IAMCrossbar crossbar = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[DirectShow] Failed creating capture graph builder");
				return;
			}
			captureGraphObject = Activator.CreateInstance(typeFromCLSID);
			captureGraphBuilder = (ICaptureGraphBuilder2)captureGraphObject;
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("Failed creating filter graph");
				return;
			}
			graphObject = Activator.CreateInstance(typeFromCLSID);
			filterGraph = (IFilterGraph2)graphObject;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				LogHelper.Instance.Log("[DirectShow] Failed creating device object for moniker");
				return;
			}
			sourceBase = (IBaseFilter)sourceObject;
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
			grabberObject = Activator.CreateInstance(typeFromCLSID);
			sampleGrabberInterface = (ISampleGrabber)grabberObject;
			grabberBase = (IBaseFilter)grabberObject;
			filterGraph.AddFilter(sourceBase, "source");
			filterGraph.AddFilter(grabberBase, "grabber_video");
			AMMediaType mediaType = new AMMediaType();
			mediaType.MajorType = MediaType.Video;
			mediaType.SubType = MediaSubType.RGB24;
			sampleGrabberInterface.SetMediaType(mediaType);
			captureGraphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, sourceBase, typeof(IAMCrossbar).GUID, out retInterface);
			if (retInterface != null)
			{
				crossbar = (IAMCrossbar)retInterface;
			}
			isCrossbarAvailable = crossbar != null;
			crossbarVideoInputs = ColletCrossbarVideoInputs(crossbar);
			sampleGrabberInterface.SetBufferSamples(bufferThem: false);
			sampleGrabberInterface.SetOneShot(oneShot: false);
			sampleGrabberInterface.SetCallback(sampleGrabber, 1);
			GetPinCapabilitiesAndConfigureSizeAndRate(captureGraphBuilder, sourceBase, PinCategory.Capture, videoResolution, ref videoCapabilities);
			lock (cacheVideoCapabilities)
			{
				if (videoCapabilities != null && !cacheVideoCapabilities.ContainsKey(deviceMoniker))
				{
					cacheVideoCapabilities.Add(deviceMoniker, videoCapabilities);
				}
			}
			if (runGraph)
			{
				captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, sourceBase, null, grabberBase);
				if (sampleGrabberInterface.GetConnectedMediaType(mediaType) == 0)
				{
					VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
					sampleGrabber.Size = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
					mediaType.Dispose();
				}
				mediaControl = (IMediaControl)graphObject;
				mediaEventEx = (IMediaEventEx)graphObject;
				mediaControl.Run();
				StartEvent.Set();
				LogHelper.Instance.Log("[DirectShow] graph is running; avg rate = {0}.", new object[1] { videoResolution.AverageFrameRate });
				do
				{
					if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) == 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
						if (Debug)
						{
							LogHelper.Instance.Log("[DirectShow]Event: {0}", new object[1] { lEventCode.ToString() });
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
							SetCurrentCrossbarInput(crossbar, crossbarVideoInput);
							crossbarVideoInput = GetCurrentCrossbarInput(crossbar);
						}
					}
					if (!m_simulateTrigger)
					{
						continue;
					}
					lock (m_listLock)
					{
						ReceivedFrames.Clear();
					}
					sampleGrabber.Grab = true;
					Thread.Sleep(GrabWaitTime);
					bool simulateTrigger = (sampleGrabber.Grab = false);
					m_simulateTrigger = simulateTrigger;
					if (Debug)
					{
						LogHelper.Instance.Log("There are {0} frames in the list.", new object[1] { ReceivedFrames.Count });
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
								LogHelper.Instance.Log(">> [DirectShow]Event: {0}", new object[1] { lEventCode.ToString() });
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
					LogHelper.Instance.Log("there were {0} frames received via video.", new object[1] { framesReceived });
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
			sourceBase = null;
			mediaControl = null;
			mediaEventEx = null;
			crossbar = null;
			grabberBase = null;
			sampleGrabberInterface = null;
			if (graphObject != null)
			{
				Marshal.ReleaseComObject(graphObject);
				graphObject = null;
			}
			if (sourceObject != null)
			{
				Marshal.ReleaseComObject(sourceObject);
				sourceObject = null;
			}
			if (grabberObject != null)
			{
				Marshal.ReleaseComObject(grabberObject);
				grabberObject = null;
			}
			if (captureGraphObject != null)
			{
				Marshal.ReleaseComObject(captureGraphObject);
				captureGraphObject = null;
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
			List<VideoInput> videoInputsList = new List<VideoInput>();
			if (crossbar != null && crossbar.get_PinCounts(out var _, out var inputPinCount) == 0)
			{
				for (int i = 0; i < inputPinCount; i++)
				{
					if (crossbar.get_CrossbarPinInfo(isInputPin: true, i, out var _, out var physicalType) == 0 && physicalType < PhysicalConnectorType.AudioTuner)
					{
						videoInputsList.Add(new VideoInput(i, physicalType));
					}
				}
			}
			VideoInput[] videoInputs = new VideoInput[videoInputsList.Count];
			videoInputsList.CopyTo(videoInputs);
			cacheCrossbarVideoInputs.Add(deviceMoniker, videoInputs);
			return videoInputs;
		}
	}

	private VideoInput GetCurrentCrossbarInput(IAMCrossbar crossbar)
	{
		VideoInput result = VideoInput.Default;
		if (crossbar.get_PinCounts(out var outputPinCount, out var _) == 0)
		{
			int videoDecoderOutputPin = -1;
			int pinIndexRelated;
			for (int i = 0; i < outputPinCount; i++)
			{
				if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out var physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
				{
					videoDecoderOutputPin = i;
					break;
				}
			}
			if (videoDecoderOutputPin != -1 && crossbar.get_IsRoutedTo(videoDecoderOutputPin, out var inputPinIndex) == 0)
			{
				crossbar.get_CrossbarPinInfo(isInputPin: true, inputPinIndex, out pinIndexRelated, out var inputType);
				result = new VideoInput(inputPinIndex, inputType);
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
		int videoDecoderOutputPin = -1;
		int videoInputPin = -1;
		int pinIndexRelated;
		PhysicalConnectorType physicalType;
		for (int i = 0; i < outputPinCount; i++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
			{
				videoDecoderOutputPin = i;
				break;
			}
		}
		for (int j = 0; j < inputPinCount; j++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: true, j, out pinIndexRelated, out physicalType) == 0 && physicalType == videoInput.Type && j == videoInput.Index)
			{
				videoInputPin = j;
				break;
			}
		}
		if (videoInputPin != -1 && videoDecoderOutputPin != -1 && crossbar.CanRoute(videoDecoderOutputPin, videoInputPin) == 0)
		{
			crossbar.Route(videoDecoderOutputPin, videoInputPin);
		}
	}

	private void OnNewFrame(Bitmap image)
	{
		framesReceived++;
		bytesReceived += image.Width * image.Height * (System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) >> 3);
		if (!Monitor.TryEnter(m_listLock))
		{
			return;
		}
		try
		{
			if (!stopEvent.WaitOne(0, exitContext: false) && m_simulateTrigger)
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
