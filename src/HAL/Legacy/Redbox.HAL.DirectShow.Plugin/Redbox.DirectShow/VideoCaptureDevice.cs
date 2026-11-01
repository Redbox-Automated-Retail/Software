using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Core;

namespace Redbox.DirectShow;

public class VideoCaptureDevice : IVideoSource
{
	private string deviceMoniker;

	private int framesReceived;

	private long bytesReceived;

	private VideoCapabilities videoResolution;

	private VideoCapabilities snapshotResolution;

	private bool provideSnapshots;

	private Thread thread;

	private ManualResetEvent stopEvent;

	private VideoCapabilities[] videoCapabilities;

	private VideoCapabilities[] snapshotCapabilities;

	private bool needToSetVideoInput;

	private bool needToSimulateTrigger;

	private bool needToDisplayPropertyPage;

	private bool needToDisplayCrossBarPropertyPage;

	private IntPtr parentWindowForPropertyPage = IntPtr.Zero;

	private object sourceObject;

	private DateTime startTime = default(DateTime);

	private object sync = new object();

	private bool? isCrossbarAvailable = null;

	private VideoInput[] crossbarVideoInputs;

	private VideoInput crossbarVideoInput = VideoInput.Default;

	private static Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new Dictionary<string, VideoCapabilities[]>();

	private static Dictionary<string, VideoCapabilities[]> cacheSnapshotCapabilities = new Dictionary<string, VideoCapabilities[]>();

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

	public virtual string Source
	{
		get
		{
			return deviceMoniker;
		}
		set
		{
			deviceMoniker = value;
			videoCapabilities = null;
			snapshotCapabilities = null;
			crossbarVideoInputs = null;
			isCrossbarAvailable = null;
		}
	}

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

	public VideoCapabilities[] SnapshotCapabilities
	{
		get
		{
			if (snapshotCapabilities == null)
			{
				lock (cacheSnapshotCapabilities)
				{
					if (!string.IsNullOrEmpty(deviceMoniker) && cacheSnapshotCapabilities.ContainsKey(deviceMoniker))
					{
						snapshotCapabilities = cacheSnapshotCapabilities[deviceMoniker];
					}
				}
				if (snapshotCapabilities == null)
				{
					if (!IsRunning)
					{
						WorkerThread(runGraph: false);
					}
					else
					{
						for (int i = 0; i < 500; i++)
						{
							if (snapshotCapabilities != null)
							{
								break;
							}
							Thread.Sleep(10);
						}
					}
				}
			}
			if (snapshotCapabilities == null)
			{
				return new VideoCapabilities[0];
			}
			return snapshotCapabilities;
		}
	}

	public object SourceObject => sourceObject;

	public event NewFrameEventHandler NewFrame;

	public event NewFrameEventHandler SnapshotFrame;

	public event VideoSourceErrorEventHandler VideoSourceError;

	public event PlayingFinishedEventHandler PlayingFinished;

	public VideoCaptureDevice()
	{
	}

	public VideoCaptureDevice(string deviceMoniker)
	{
		this.deviceMoniker = deviceMoniker;
	}

	public bool Start()
	{
		if (!IsRunning)
		{
			if (string.IsNullOrEmpty(deviceMoniker))
			{
				LogHelper.Instance.Log("[DirectShow] Video source is not specified.");
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

	public void DisplayPropertyPage(IntPtr parentWindow)
	{
		if (deviceMoniker == null || deviceMoniker == string.Empty)
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			if (IsRunning)
			{
				parentWindowForPropertyPage = parentWindow;
				needToDisplayPropertyPage = true;
				return;
			}
			object tempSourceObject = null;
			try
			{
				tempSourceObject = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(tempSourceObject is ISpecifyPropertyPages))
			{
				throw new NotSupportedException("The video source does not support configuration property page.");
			}
			DisplayPropertyPage(parentWindow, tempSourceObject);
			Marshal.ReleaseComObject(tempSourceObject);
		}
	}

	public void DisplayCrossbarPropertyPage(IntPtr parentWindow)
	{
		lock (sync)
		{
			for (int i = 0; i < 500; i++)
			{
				if (isCrossbarAvailable.HasValue)
				{
					break;
				}
				if (!IsRunning)
				{
					break;
				}
				Thread.Sleep(10);
			}
			if (!IsRunning || !isCrossbarAvailable.HasValue)
			{
				throw new ApplicationException("The video source must be running in order to display crossbar property page.");
			}
			if (!isCrossbarAvailable.Value)
			{
				throw new NotSupportedException("Crossbar configuration is not supported by currently running video source.");
			}
			parentWindowForPropertyPage = parentWindow;
			needToDisplayCrossBarPropertyPage = true;
		}
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
		needToSimulateTrigger = true;
	}

	public bool SetCameraProperty(CameraControlProperty property, int value, CameraControlFlags controlFlags)
	{
		bool ret = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object tempSourceObject = null;
			try
			{
				tempSourceObject = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(tempSourceObject is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			IAMCameraControl iAMCameraControl = (IAMCameraControl)tempSourceObject;
			int hr = iAMCameraControl.Set(property, value, controlFlags);
			ret = hr >= 0;
			Marshal.ReleaseComObject(tempSourceObject);
			return ret;
		}
	}

	public bool GetCameraProperty(CameraControlProperty property, out int value, out CameraControlFlags controlFlags)
	{
		bool ret = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object tempSourceObject = null;
			try
			{
				tempSourceObject = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(tempSourceObject is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			IAMCameraControl iAMCameraControl = (IAMCameraControl)tempSourceObject;
			int hr = iAMCameraControl.Get(property, out value, out controlFlags);
			ret = hr >= 0;
			Marshal.ReleaseComObject(tempSourceObject);
			return ret;
		}
	}

	public bool GetCameraPropertyRange(CameraControlProperty property, out int minValue, out int maxValue, out int stepSize, out int defaultValue, out CameraControlFlags controlFlags)
	{
		bool ret = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object tempSourceObject = null;
			try
			{
				tempSourceObject = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(tempSourceObject is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			IAMCameraControl iAMCameraControl = (IAMCameraControl)tempSourceObject;
			int range = iAMCameraControl.GetRange(property, out minValue, out maxValue, out stepSize, out defaultValue, out controlFlags);
			ret = range >= 0;
			Marshal.ReleaseComObject(tempSourceObject);
			return ret;
		}
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		bool isSnapshotSupported = false;
		SampleGrabber videoGrabberCallback = new SampleGrabber(OnNewFrame);
		SampleGrabber snapshotGrabberCallback = new SampleGrabber(OnSnapshotFrame);
		object captureGraphObject = null;
		object graphObject = null;
		object videoGrabberObject = null;
		object snapshotGrabberObject = null;
		object retInterface = null;
		ICaptureGraphBuilder2 captureGraphBuilder = null;
		IFilterGraph2 filterGraph = null;
		IBaseFilter sourceBase = null;
		IBaseFilter videoGrabberBase = null;
		IBaseFilter snapshotGrabberBase = null;
		ISampleGrabber videoSampleGrabber = null;
		ISampleGrabber snapshotSampleGrabber = null;
		IMediaControl mediaControl = null;
		IAMVideoControl iAMVideoControl = null;
		IMediaEventEx mediaEventEx = null;
		IPin pin = null;
		IAMCrossbar crossbar = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating capture graph builder");
			}
			captureGraphObject = Activator.CreateInstance(typeFromCLSID);
			captureGraphBuilder = (ICaptureGraphBuilder2)captureGraphObject;
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating filter graph");
			}
			graphObject = Activator.CreateInstance(typeFromCLSID);
			filterGraph = (IFilterGraph2)graphObject;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				throw new ApplicationException("Failed creating device object for moniker");
			}
			sourceBase = (IBaseFilter)sourceObject;
			try
			{
				iAMVideoControl = (IAMVideoControl)sourceObject;
			}
			catch
			{
			}
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating sample grabber");
			}
			videoGrabberObject = Activator.CreateInstance(typeFromCLSID);
			videoSampleGrabber = (ISampleGrabber)videoGrabberObject;
			videoGrabberBase = (IBaseFilter)videoGrabberObject;
			snapshotGrabberObject = Activator.CreateInstance(typeFromCLSID);
			snapshotSampleGrabber = (ISampleGrabber)snapshotGrabberObject;
			snapshotGrabberBase = (IBaseFilter)snapshotGrabberObject;
			filterGraph.AddFilter(sourceBase, "source");
			filterGraph.AddFilter(videoGrabberBase, "grabber_video");
			filterGraph.AddFilter(snapshotGrabberBase, "grabber_snapshot");
			AMMediaType mediaType = new AMMediaType();
			mediaType.MajorType = MediaType.Video;
			mediaType.SubType = MediaSubType.RGB24;
			videoSampleGrabber.SetMediaType(mediaType);
			snapshotSampleGrabber.SetMediaType(mediaType);
			captureGraphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, sourceBase, typeof(IAMCrossbar).GUID, out retInterface);
			if (retInterface != null)
			{
				crossbar = (IAMCrossbar)retInterface;
			}
			isCrossbarAvailable = crossbar != null;
			crossbarVideoInputs = ColletCrossbarVideoInputs(crossbar);
			if (iAMVideoControl != null)
			{
				captureGraphBuilder.FindPin(sourceObject, PinDirection.Output, PinCategory.StillImage, MediaType.Video, unconnected: false, 0, out pin);
				if (pin != null)
				{
					iAMVideoControl.GetCaps(pin, out var flags);
					isSnapshotSupported = (flags & VideoControlFlags.ExternalTriggerEnable) != 0;
				}
			}
			videoSampleGrabber.SetBufferSamples(bufferThem: false);
			videoSampleGrabber.SetOneShot(oneShot: false);
			videoSampleGrabber.SetCallback(videoGrabberCallback, 1);
			snapshotSampleGrabber.SetBufferSamples(bufferThem: true);
			snapshotSampleGrabber.SetOneShot(oneShot: false);
			snapshotSampleGrabber.SetCallback(snapshotGrabberCallback, 1);
			GetPinCapabilitiesAndConfigureSizeAndRate(captureGraphBuilder, sourceBase, PinCategory.Capture, videoResolution, ref videoCapabilities);
			if (isSnapshotSupported)
			{
				GetPinCapabilitiesAndConfigureSizeAndRate(captureGraphBuilder, sourceBase, PinCategory.StillImage, snapshotResolution, ref snapshotCapabilities);
			}
			else
			{
				snapshotCapabilities = new VideoCapabilities[0];
			}
			lock (cacheVideoCapabilities)
			{
				if (videoCapabilities != null && !cacheVideoCapabilities.ContainsKey(deviceMoniker))
				{
					cacheVideoCapabilities.Add(deviceMoniker, videoCapabilities);
				}
			}
			lock (cacheSnapshotCapabilities)
			{
				if (snapshotCapabilities != null && !cacheSnapshotCapabilities.ContainsKey(deviceMoniker))
				{
					cacheSnapshotCapabilities.Add(deviceMoniker, snapshotCapabilities);
				}
			}
			if (runGraph)
			{
				captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, sourceBase, null, videoGrabberBase);
				if (videoSampleGrabber.GetConnectedMediaType(mediaType) == 0)
				{
					VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
					videoGrabberCallback.Size = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
					mediaType.Dispose();
				}
				if (isSnapshotSupported && provideSnapshots)
				{
					captureGraphBuilder.RenderStream(PinCategory.StillImage, MediaType.Video, sourceBase, null, snapshotGrabberBase);
					if (snapshotSampleGrabber.GetConnectedMediaType(mediaType) == 0)
					{
						VideoInfoHeader videoInfoHeader2 = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
						snapshotGrabberCallback.Size = new Size(videoInfoHeader2.BmiHeader.Width, videoInfoHeader2.BmiHeader.Height);
						mediaType.Dispose();
					}
				}
				mediaControl = (IMediaControl)graphObject;
				mediaEventEx = (IMediaEventEx)graphObject;
				mediaControl.Run();
				snapshotGrabberCallback.Grab = true;
				if (isSnapshotSupported && provideSnapshots)
				{
					startTime = DateTime.Now;
					iAMVideoControl.SetMode(pin, VideoControlFlags.ExternalTriggerEnable);
				}
				do
				{
					if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) >= 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
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
					if (needToSimulateTrigger)
					{
						needToSimulateTrigger = false;
						if (isSnapshotSupported && provideSnapshots)
						{
							iAMVideoControl.SetMode(pin, VideoControlFlags.Trigger);
						}
					}
					if (needToDisplayPropertyPage)
					{
						needToDisplayPropertyPage = false;
						DisplayPropertyPage(parentWindowForPropertyPage, sourceObject);
						if (crossbar != null)
						{
							crossbarVideoInput = GetCurrentCrossbarInput(crossbar);
						}
					}
					if (needToDisplayCrossBarPropertyPage)
					{
						needToDisplayCrossBarPropertyPage = false;
						if (crossbar != null)
						{
							DisplayPropertyPage(parentWindowForPropertyPage, crossbar);
							crossbarVideoInput = GetCurrentCrossbarInput(crossbar);
						}
					}
				}
				while (!stopEvent.WaitOne(100, exitContext: false));
				snapshotGrabberCallback.Grab = false;
				mediaControl.Stop();
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
			iAMVideoControl = null;
			mediaEventEx = null;
			pin = null;
			crossbar = null;
			videoGrabberBase = null;
			snapshotGrabberBase = null;
			videoSampleGrabber = null;
			snapshotSampleGrabber = null;
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
			if (videoGrabberObject != null)
			{
				Marshal.ReleaseComObject(videoGrabberObject);
				videoGrabberObject = null;
			}
			if (snapshotGrabberObject != null)
			{
				Marshal.ReleaseComObject(snapshotGrabberObject);
				snapshotGrabberObject = null;
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

	private void DisplayPropertyPage(IntPtr parentWindow, object sourceObject)
	{
		try
		{
			ISpecifyPropertyPages specifyPropertyPages = (ISpecifyPropertyPages)sourceObject;
			specifyPropertyPages.GetPages(out var pPages);
			FilterInfo filterInfo = new FilterInfo(deviceMoniker);
			Win32.OleCreatePropertyFrame(parentWindow, 0, 0, filterInfo.Name, 1, ref sourceObject, pPages.cElems, pPages.pElems, 0, 0, IntPtr.Zero);
			Marshal.FreeCoTaskMem(pPages.pElems);
		}
		catch
		{
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
		if (!stopEvent.WaitOne(0, exitContext: false) && NewFrame != null)
		{
			NewFrame(this, new NewFrameEventArgs(image));
		}
	}

	private void OnSnapshotFrame(Bitmap image)
	{
		if ((DateTime.Now - startTime).TotalSeconds >= 4.0 && !stopEvent.WaitOne(0, exitContext: false) && SnapshotFrame != null)
		{
			SnapshotFrame(this, new NewFrameEventArgs(image));
		}
	}
}
