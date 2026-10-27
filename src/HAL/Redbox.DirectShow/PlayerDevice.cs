using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class PlayerDevice : IVideoSource
{
	private string deviceMoniker;

	private int framesReceived;

	private VideoCapabilities videoResolution;

	private VideoCapabilities snapshotResolution;

	private Thread thread;

	private ManualResetEvent stopEvent;

	private VideoCapabilities[] videoCapabilities;

	private VideoCapabilities[] snapshotCapabilities;

	private bool needToSetVideoInput;

	private bool m_simulateTrigger;

	private bool needToDisplayPropertyPage;

	private bool needToDisplayCrossBarPropertyPage;

	private IntPtr parentWindowForPropertyPage = IntPtr.Zero;

	private object sourceObject;

	private object sync = new object();

	private bool? isCrossbarAvailable;

	private VideoInput[] crossbarVideoInputs;

	private VideoInput crossbarVideoInput = VideoInput.Default;

	private static Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new Dictionary<string, VideoCapabilities[]>();

	private static Dictionary<string, VideoCapabilities[]> cacheSnapshotCapabilities = new Dictionary<string, VideoCapabilities[]>();

	private static Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new Dictionary<string, VideoInput[]>();

	private readonly bool Debug;

	private readonly int GrabTime;

	private readonly List<Bitmap> ReceivedFrames = new List<Bitmap>();

	private readonly object m_listLock = new object();

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

	public PlayerDevice(string deviceMoniker, int grabTime, bool debug)
	{
		this.deviceMoniker = deviceMoniker;
		GrabTime = grabTime;
		Debug = debug;
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
			object obj = null;
			try
			{
				obj = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(obj is ISpecifyPropertyPages))
			{
				throw new NotSupportedException("The video source does not support configuration property page.");
			}
			DisplayPropertyPage(parentWindow, obj);
			Marshal.ReleaseComObject(obj);
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
		m_simulateTrigger = true;
	}

	public bool SetCameraProperty(CameraControlProperty property, int value, CameraControlFlags controlFlags)
	{
		bool flag = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object obj = null;
			try
			{
				obj = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(obj is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			flag = ((IAMCameraControl)obj).Set(property, value, controlFlags) >= 0;
			Marshal.ReleaseComObject(obj);
			return flag;
		}
	}

	public bool GetCameraProperty(CameraControlProperty property, out int value, out CameraControlFlags controlFlags)
	{
		bool flag = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object obj = null;
			try
			{
				obj = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(obj is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			flag = ((IAMCameraControl)obj).Get(property, out value, out controlFlags) >= 0;
			Marshal.ReleaseComObject(obj);
			return flag;
		}
	}

	public bool GetCameraPropertyRange(CameraControlProperty property, out int minValue, out int maxValue, out int stepSize, out int defaultValue, out CameraControlFlags controlFlags)
	{
		bool flag = true;
		if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
		{
			throw new ArgumentException("Video source is not specified.");
		}
		lock (sync)
		{
			object obj = null;
			try
			{
				obj = FilterInfo.CreateFilter(deviceMoniker);
			}
			catch
			{
				throw new ApplicationException("Failed creating device object for moniker.");
			}
			if (!(obj is IAMCameraControl))
			{
				throw new NotSupportedException("The video source does not support camera control.");
			}
			flag = ((IAMCameraControl)obj).GetRange(property, out minValue, out maxValue, out stepSize, out defaultValue, out controlFlags) >= 0;
			Marshal.ReleaseComObject(obj);
			return flag;
		}
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		SampleGrabber sampleGrabber = new SampleGrabber(OnNewFrame);
		sampleGrabber.Grab = true;
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
			obj = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2) ?? throw new ApplicationException("Failed creating capture graph builder"));
			captureGraphBuilder = (ICaptureGraphBuilder2)obj;
			obj2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph) ?? throw new ApplicationException("Failed creating filter graph"));
			filterGraph = (IFilterGraph2)obj2;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				throw new ApplicationException("Failed creating device object for moniker");
			}
			baseFilter = (IBaseFilter)sourceObject;
			try
			{
				_ = (IAMVideoControl)sourceObject;
			}
			catch
			{
			}
			obj3 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber) ?? throw new ApplicationException("Failed creating sample grabber"));
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
			snapshotCapabilities = new VideoCapabilities[0];
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
							SetCurrentCrossbarInput(iAMCrossbar, crossbarVideoInput);
							crossbarVideoInput = GetCurrentCrossbarInput(iAMCrossbar);
						}
					}
					if (m_simulateTrigger)
					{
						m_simulateTrigger = false;
						lock (m_listLock)
						{
							ReceivedFrames.Clear();
						}
						sampleGrabber.Grab = true;
						Thread.Sleep(GrabTime);
						bool simulateTrigger = (sampleGrabber.Grab = false);
						m_simulateTrigger = simulateTrigger;
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
							}
							foreach (Bitmap receivedFrame in ReceivedFrames)
							{
								receivedFrame.Dispose();
							}
							ReceivedFrames.Clear();
						}
					}
					if (needToDisplayPropertyPage)
					{
						needToDisplayPropertyPage = false;
						DisplayPropertyPage(parentWindowForPropertyPage, sourceObject);
						if (iAMCrossbar != null)
						{
							crossbarVideoInput = GetCurrentCrossbarInput(iAMCrossbar);
						}
					}
					if (needToDisplayCrossBarPropertyPage)
					{
						needToDisplayCrossBarPropertyPage = false;
						if (iAMCrossbar != null)
						{
							DisplayPropertyPage(parentWindowForPropertyPage, iAMCrossbar);
							crossbarVideoInput = GetCurrentCrossbarInput(iAMCrossbar);
						}
					}
				}
				while (!stopEvent.WaitOne(100, exitContext: false));
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

	private void DisplayPropertyPage(IntPtr parentWindow, object sourceObject)
	{
		try
		{
			((ISpecifyPropertyPages)sourceObject).GetPages(out var pPages);
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
		if (!stopEvent.WaitOne(0, exitContext: false) && NewFrame != null)
		{
			NewFrame(this, new NewFrameEventArgs(image));
		}
	}
}
