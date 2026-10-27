using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class DirectShowFrameControl : IVideoSource, ISampleGrabberCB
{
	private string deviceMoniker;

	private int framesReceived;

	private Size FrameSize;

	private VideoCapabilities videoResolution;

	private VideoCapabilities snapshotResolution;

	private string bitmapToSave;

	private Thread thread;

	private VideoCapabilities[] videoCapabilities;

	private bool needToSetVideoInput;

	private int m_simulateTrigger = ControlConstants.NotTriggered;

	private object sourceObject;

	private int m_grabImage = ControlConstants.NotTriggered;

	private int m_request;

	private bool? isCrossbarAvailable;

	private VideoInput[] crossbarVideoInputs;

	private VideoInput crossbarVideoInput = VideoInput.Default;

	private const string Moniker = "DirectShowSlim";

	private readonly ManualResetEvent StartEvent = new ManualResetEvent(initialState: false);

	private readonly ManualResetEvent FrameReceivedEvent = new ManualResetEvent(initialState: false);

	private readonly ManualResetEvent ImageCapturedEvent = new ManualResetEvent(initialState: false);

	private readonly ManualResetEvent StopEvent = new ManualResetEvent(initialState: false);

	private readonly object sync = new object();

	private readonly int GrabWaitTime;

	private readonly bool Debug;

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

	public int SampleCB(double sampleTime, IntPtr sample)
	{
		return 0;
	}

	public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
	{
		if (Interlocked.CompareExchange(ref m_grabImage, ControlConstants.NotTriggered, ControlConstants.Triggered) == ControlConstants.NotTriggered)
		{
			return 0;
		}
		if (Debug)
		{
			LogHelper.Instance.Log("[{0}]: BufferCB called ( request = {1} ).", "DirectShowSlim", m_request);
			if (File.Exists(bitmapToSave))
			{
				LogHelper.Instance.Log("{0} WARNING: the file {1} already exists.", "DirectShowSlim", bitmapToSave);
			}
		}
		framesReceived++;
		using (Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height, PixelFormat.Format24bppRgb))
		{
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, FrameSize.Width, FrameSize.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			int stride = bitmapData.Stride;
			int stride2 = bitmapData.Stride;
			byte* ptr = (byte*)bitmapData.Scan0.ToPointer() + stride2 * (FrameSize.Height - 1);
			byte* ptr2 = (byte*)buffer.ToPointer();
			for (int i = 0; i < FrameSize.Height; i++)
			{
				Win32.memcpy(ptr, ptr2, stride);
				ptr -= stride2;
				ptr2 += stride;
			}
			bitmap.UnlockBits(bitmapData);
			bitmap.Save(bitmapToSave);
		}
		FrameReceivedEvent.Set();
		return 0;
	}

	public DirectShowFrameControl(string deviceMoniker, int snapTimeout, bool debug)
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
			LogHelper.Instance.Log("[{0}] Video source is not specified - cannot start graph.", "DirectShowSlim");
			return false;
		}
		framesReceived = 0;
		isCrossbarAvailable = null;
		needToSetVideoInput = true;
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
			StopEvent.Set();
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
		StopEvent.Close();
		StartEvent.Close();
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

	public bool SimulateTrigger(string fileName, int waitTime)
	{
		bitmapToSave = fileName;
		m_request++;
		if (Debug)
		{
			LogHelper.Instance.Log("[{0}] SnapRequest {1}", "DirectShowSlim", m_request);
		}
		try
		{
			ImageCapturedEvent.Reset();
			FrameReceivedEvent.Reset();
			if (Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.Triggered) == ControlConstants.Triggered)
			{
				return false;
			}
			if (ImageCapturedEvent.WaitOne(waitTime))
			{
				return true;
			}
			LogHelper.Instance.Log("[{0}] The trigger event was not set.", "DirectShowSlim");
			return false;
		}
		finally
		{
			bitmapToSave = null;
		}
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		object obj = null;
		object obj2 = null;
		object obj3 = null;
		object retInterface = null;
		ICaptureGraphBuilder2 captureGraphBuilder = null;
		IFilterGraph2 filterGraph = null;
		IBaseFilter baseFilter = null;
		IBaseFilter baseFilter2 = null;
		ISampleGrabber sampleGrabber = null;
		IMediaControl mediaControl = null;
		IMediaEventEx mediaEventEx = null;
		IAMCrossbar iAMCrossbar = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[{0}] Failed creating capture graph builder", "DirectShowSlim");
				return;
			}
			obj = Activator.CreateInstance(typeFromCLSID);
			captureGraphBuilder = (ICaptureGraphBuilder2)obj;
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[{0}]Failed creating filter graph", "DirectShowSlim");
				return;
			}
			obj2 = Activator.CreateInstance(typeFromCLSID);
			filterGraph = (IFilterGraph2)obj2;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				LogHelper.Instance.Log("[{0}] Failed creating device object for moniker", "DirectShowSlim");
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
				LogHelper.Instance.Log("[{0}] Failed creating sample grabber", "DirectShowSlim");
				return;
			}
			obj3 = Activator.CreateInstance(typeFromCLSID);
			sampleGrabber = (ISampleGrabber)obj3;
			baseFilter2 = (IBaseFilter)obj3;
			filterGraph.AddFilter(baseFilter, "source");
			filterGraph.AddFilter(baseFilter2, "grabber_video");
			AMMediaType aMMediaType = new AMMediaType();
			aMMediaType.MajorType = MediaType.Video;
			aMMediaType.SubType = MediaSubType.RGB24;
			sampleGrabber.SetMediaType(aMMediaType);
			captureGraphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, baseFilter, typeof(IAMCrossbar).GUID, out retInterface);
			if (retInterface != null)
			{
				iAMCrossbar = (IAMCrossbar)retInterface;
			}
			isCrossbarAvailable = iAMCrossbar != null;
			crossbarVideoInputs = ColletCrossbarVideoInputs(iAMCrossbar);
			sampleGrabber.SetBufferSamples(bufferThem: false);
			sampleGrabber.SetOneShot(oneShot: false);
			sampleGrabber.SetCallback(this, 1);
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
				if (sampleGrabber.GetConnectedMediaType(aMMediaType) == 0)
				{
					VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(aMMediaType.FormatPtr, typeof(VideoInfoHeader));
					FrameSize = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
					aMMediaType.Dispose();
				}
				mediaControl = (IMediaControl)obj2;
				mediaEventEx = (IMediaEventEx)obj2;
				mediaControl.Run();
				StartEvent.Set();
				LogHelper.Instance.Log("[{0}] graph is running; avg rate = {1}.", "DirectShowSlim", videoResolution.AverageFrameRate);
				do
				{
					if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) == 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
						if (Debug)
						{
							LogHelper.Instance.Log("[{0}]Event: {1}", "DirectShowSlim", lEventCode.ToString());
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
					if (Thread.VolatileRead(ref m_simulateTrigger) != ControlConstants.Triggered)
					{
						continue;
					}
					Interlocked.Exchange(ref m_grabImage, ControlConstants.Triggered);
					if (!FrameReceivedEvent.WaitOne(GrabWaitTime))
					{
						Interlocked.Exchange(ref m_grabImage, ControlConstants.NotTriggered);
						LogHelper.Instance.Log("[{0}] There are no captured frames from the device.", "DirectShowSlim");
						if (mediaEventEx != null && mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
						{
							mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
							LogHelper.Instance.Log(">> [{0}] Event: {1}", "DirectShowSlim", lEventCode.ToString());
						}
					}
					else
					{
						ImageCapturedEvent.Set();
					}
					Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.NotTriggered);
				}
				while (!StopEvent.WaitOne(100, exitContext: false));
				mediaControl.Stop();
				LogHelper.Instance.Log("[{0}] Shutdown statistics: there were {1} frames received via video.", "DirectShowSlim", framesReceived);
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
			sampleGrabber = null;
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
}
