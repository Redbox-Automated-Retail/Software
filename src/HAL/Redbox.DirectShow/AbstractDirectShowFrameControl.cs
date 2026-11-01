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

public abstract class AbstractDirectShowFrameControl : IVideoSource, ISampleGrabberCB
{
	protected readonly string ControlMoniker;

	protected readonly int GrabWaitTime;

	protected readonly bool Debug;

	protected string bitmapToSave;

	private string deviceMoniker;

	private int framesReceived;

	private Size FrameSize;

	private VideoCapabilities videoResolution;

	private Thread thread;

	private VideoCapabilities[] videoCapabilities;

	private bool needToSetVideoInput;

	private object sourceObject;

	private int m_request;

	private bool? isCrossbarAvailable;

	private VideoInput[] crossbarVideoInputs;

	private VideoInput crossbarVideoInput = VideoInput.Default;

	private readonly ManualResetEvent StartEvent = new ManualResetEvent(initialState: false);

	private readonly ManualResetEvent StopEvent = new ManualResetEvent(initialState: false);

	private readonly object sync = new object();

	private static Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new Dictionary<string, VideoCapabilities[]>();

	private static Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new Dictionary<string, VideoInput[]>();

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

	protected abstract bool SnapRequested { get; }

	public event NewFrameEventHandler NewFrame
	{
		add
		{
		}
		remove
		{
		}
	}

	public event VideoSourceErrorEventHandler VideoSourceError
	{
		add
		{
		}
		remove
		{
		}
	}

	public event PlayingFinishedEventHandler PlayingFinished;

	public static AbstractDirectShowFrameControl GetControl(FrameControls control, string moniker, int grab, bool debug)
	{
		if (control == FrameControls.Unknown)
		{
			throw new ArgumentException("control");
		}
		if (FrameControls.Modern == control)
		{
			return new FrameControlModern(moniker, grab, debug);
		}
		return new FrameControlLegacy(moniker, grab, debug);
	}

	public int SampleCB(double sampleTime, IntPtr sample)
	{
		return 0;
	}

	public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
	{
		if (!SnapRequested)
		{
			return 0;
		}
		if (Debug)
		{
			LogHelper.Instance.Log("[{0}]: BufferCB called ( request = {1} ).", ControlMoniker, m_request);
			if (File.Exists(bitmapToSave))
			{
				LogHelper.Instance.Log("{0} WARNING: the file {1} already exists.", ControlMoniker, bitmapToSave);
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
		OnImageCaptured();
		return 0;
	}

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		if (string.IsNullOrEmpty(deviceMoniker))
		{
			LogHelper.Instance.Log("[{0}] Video source is not specified - cannot start graph.", ControlMoniker);
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
		SignalToStop(wait: true);
	}

	public void SignalToStop(bool wait)
	{
		if (thread != null)
		{
			StopEvent.Set();
			if (wait)
			{
				thread.Join();
			}
			Free();
		}
	}

	public void WaitForStop()
	{
		SignalToStop();
	}

	public void Stop()
	{
		SignalToStop();
	}

	public bool SimulateTrigger(string fileName, int waitTime)
	{
		bitmapToSave = fileName;
		m_request++;
		if (Debug)
		{
			LogHelper.Instance.Log("[{0}] SnapRequest {1}", ControlMoniker, m_request);
		}
		try
		{
			return OnSimulateTrigger(fileName, waitTime);
		}
		finally
		{
			bitmapToSave = null;
		}
	}

	protected AbstractDirectShowFrameControl(string deviceMoniker, int snapTimeout, bool debug)
	{
		this.deviceMoniker = deviceMoniker;
		GrabWaitTime = snapTimeout;
		Debug = debug;
		ControlMoniker = GetType().Name;
	}

	protected abstract bool OnSimulateTrigger(string file, int captureWait);

	protected abstract void OnImageCaptured();

	protected virtual bool OnGraphEvent()
	{
		return true;
	}

	protected abstract void OnFree();

	private void Free()
	{
		thread = null;
		StopEvent.Close();
		StartEvent.Close();
		try
		{
			OnFree();
		}
		catch
		{
		}
	}

	private void WorkerThread()
	{
		WorkerThread(runGraph: true);
	}

	private void WorkerThread(bool runGraph)
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		object graphBuilderObject = null;
		object graphObject = null;
		object grabberObject = null;
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
				LogHelper.Instance.Log("[{0}] Failed creating capture graph builder", ControlMoniker);
				return;
			}
			graphBuilderObject = Activator.CreateInstance(typeFromCLSID);
			captureGraphBuilder = (ICaptureGraphBuilder2)graphBuilderObject;
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if (typeFromCLSID == null)
			{
				LogHelper.Instance.Log("[{0}]Failed creating filter graph", ControlMoniker);
				return;
			}
			graphObject = Activator.CreateInstance(typeFromCLSID);
			filterGraph = (IFilterGraph2)graphObject;
			captureGraphBuilder.SetFiltergraph((IGraphBuilder)filterGraph);
			sourceObject = FilterInfo.CreateFilter(deviceMoniker);
			if (sourceObject == null)
			{
				LogHelper.Instance.Log("[{0}] Failed creating device object for moniker", ControlMoniker);
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
				LogHelper.Instance.Log("[{0}] Failed creating sample grabber", ControlMoniker);
				return;
			}
			grabberObject = Activator.CreateInstance(typeFromCLSID);
			sampleGrabber = (ISampleGrabber)grabberObject;
			baseFilter2 = (IBaseFilter)grabberObject;
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
				mediaControl = (IMediaControl)graphObject;
				mediaEventEx = (IMediaEventEx)graphObject;
				mediaControl.Run();
				StartEvent.Set();
				LogHelper.Instance.Log("[{0}] graph is running; avg rate = {1}.", ControlMoniker, videoResolution.AverageFrameRate);
				do
				{
					if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) == 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
						if (Debug)
						{
							LogHelper.Instance.Log("[{0}]Event: {1}", ControlMoniker, lEventCode.ToString());
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
					if (!OnGraphEvent() && mediaEventEx != null && mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
					{
						mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
						LogHelper.Instance.Log(">> [{0}] Event: {1}", ControlMoniker, lEventCode.ToString());
					}
				}
				while (!StopEvent.WaitOne(100, exitContext: false));
				mediaControl.Stop();
				LogHelper.Instance.Log("[{0}] Shutdown statistics: there were {1} frames received via video.", ControlMoniker, framesReceived);
			}
		}
		catch (Exception)
		{
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
			if (graphBuilderObject != null)
			{
				Marshal.ReleaseComObject(graphBuilderObject);
				graphBuilderObject = null;
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
			List<VideoInput> videoInputs = new List<VideoInput>();
			if (crossbar != null && crossbar.get_PinCounts(out var _, out var inputPinCount) == 0)
			{
				for (int i = 0; i < inputPinCount; i++)
				{
					if (crossbar.get_CrossbarPinInfo(isInputPin: true, i, out var _, out var physicalType) == 0 && physicalType < PhysicalConnectorType.AudioTuner)
					{
						videoInputs.Add(new VideoInput(i, physicalType));
					}
				}
			}
			VideoInput[] inputs = new VideoInput[videoInputs.Count];
			videoInputs.CopyTo(inputs);
			cacheCrossbarVideoInputs.Add(deviceMoniker, inputs);
			return inputs;
		}
	}

	private VideoInput GetCurrentCrossbarInput(IAMCrossbar crossbar)
	{
		VideoInput result = VideoInput.Default;
		if (crossbar.get_PinCounts(out var outputPinCount, out var _) == 0)
		{
			int decoderPin = -1;
			int pinIndexRelated;
			for (int i = 0; i < outputPinCount; i++)
			{
				if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out var physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
				{
					decoderPin = i;
					break;
				}
			}
			if (decoderPin != -1 && crossbar.get_IsRoutedTo(decoderPin, out var inputPinIndex) == 0)
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
		int decoderPin = -1;
		int inputPin = -1;
		int pinIndexRelated;
		PhysicalConnectorType physicalType;
		for (int i = 0; i < outputPinCount; i++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: false, i, out pinIndexRelated, out physicalType) == 0 && physicalType == PhysicalConnectorType.VideoDecoder)
			{
				decoderPin = i;
				break;
			}
		}
		for (int j = 0; j < inputPinCount; j++)
		{
			if (crossbar.get_CrossbarPinInfo(isInputPin: true, j, out pinIndexRelated, out physicalType) == 0 && physicalType == videoInput.Type && j == videoInput.Index)
			{
				inputPin = j;
				break;
			}
		}
		if (inputPin != -1 && decoderPin != -1 && crossbar.CanRoute(decoderPin, inputPin) == 0)
		{
			crossbar.Route(decoderPin, inputPin);
		}
	}
}
