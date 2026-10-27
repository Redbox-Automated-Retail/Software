using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.DataMatrix.Framework.Cortex;

namespace Redbox.HAL.DataMatrix.Framework;

internal class CortexDecoder : AbstractBarcodeReader
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int ResultCallbackDelegate(int handle);

	public const int P_IMAGE_ADDRESS = 0;

	public const int P_IMAGE_WIDTH = 1;

	public const int P_IMAGE_HEIGHT = 2;

	public const int P_IMAGE_BUF_WIDTH = 3;

	public const int P_IMAGE_TYPE = 4;

	public const int P_IMAGE_ADDRESS_EX = 5;

	public const int P_CODE_RESERVED_1 = 8;

	public const int P_CODE_RESERVED_2 = 9;

	public const int P_CALLBACK_RESULT = 10;

	public const int P_CALLBACK_PROGRESS = 11;

	public const int P_CALLBACK_STATUS = 12;

	public const int P_CODE_RESERVED_3 = 20;

	public const int P_CODE_RESERVED_12 = 21;

	public const int P_CODE_RESERVED_13 = 22;

	public const int P_CODE_RESERVED_14 = 23;

	public const int P_CODE_RESERVED_15 = 24;

	public const int P_CODE_RESERVED_16 = 25;

	public const int P_CODE_RESERVED_17 = 26;

	public const int P_CODE_RESERVED_18 = 27;

	public const int P_AGC_COMPOUND_GAIN_INITIAL = 28;

	public const int P_AGC_COMPOUND_GAIN = 29;

	public const int P_AGC_DESIRED_LIGHT = 30;

	public const int P_CODE_RESERVED_19 = 31;

	public const int P_CODE_RESERVED_20 = 32;

	public const int P_ENABLE_GC = 100;

	public const int P_ENABLE_DM = 101;

	public const int P_ENABLE_QR_2005 = 102;

	public const int P_ENABLE_AZ = 103;

	public const int P_ENABLE_MAXICODE = 104;

	public const int P_ENABLE_PDF = 105;

	public const int P_ENABLE_MICRO_PDF = 106;

	public const int P_ENABLE_CCA = 107;

	public const int P_ENABLE_CCB = 108;

	public const int P_ENABLE_CCC = 109;

	public const int P_ENABLE_C128 = 110;

	public const int P_ENABLE_C39 = 111;

	public const int P_ENABLE_C93 = 112;

	public const int P_ENABLE_I25 = 113;

	public const int P_ENABLE_CBAR = 114;

	public const int P_ENABLE_UPCA = 115;

	public const int P_ENABLE_UPCE = 116;

	public const int P_ENABLE_EAN13 = 117;

	public const int P_ENABLE_EAN8 = 118;

	public const int P_ENABLE_DB_14 = 119;

	public const int P_ENABLE_DB_14_STACKED = 120;

	public const int P_ENABLE_DB_LIMITED = 121;

	public const int P_ENABLE_DB_EXPANDED = 122;

	public const int P_ENABLE_DB_EXPANDED_STACKED = 123;

	public const int P_ENABLE_ALL_1D = 124;

	public const int P_ENABLE_ALL_2D = 125;

	public const int P_ENABLE_ALL = 126;

	public const int P_ENABLE_HX = 127;

	public const int P_ENABLE_MICRO_QR = 128;

	public const int P_ENABLE_QR_MODEL1 = 129;

	public const int P_ENABLE_GM = 130;

	public const int P_ENABLE_CUSTOM_NC = 131;

	public const int P_ENABLE_CUSTOM_02 = 132;

	public const int P_ENABLE_C11 = 140;

	public const int P_ENABLE_C32 = 141;

	public const int P_ENABLE_PLE = 142;

	public const int P_ENABLE_MSI_PLE = 143;

	public const int P_ENABLE_TLP = 144;

	public const int P_ENABLE_TRI = 145;

	public const int P_ENABLE_PHA = 146;

	public const int P_ENABLE_M25 = 147;

	public const int P_ENABLE_S25 = 148;

	public const int P_ENABLE_C49 = 149;

	public const int P_ENABLE_C16K = 150;

	public const int P_ENABLE_CBLK = 151;

	public const int P_ENABLE_HK25 = 152;

	public const int P_ENABLE_POSTNET = 153;

	public const int P_ENABLE_PLANET = 154;

	public const int P_ENABLE_INTEL_MAIL = 155;

	public const int P_ENABLE_UPU = 156;

	public const int P_ENABLE_AUSTRA_POST = 157;

	public const int P_ENABLE_DUTCH_POST = 158;

	public const int P_ENABLE_JAPAN_POST = 159;

	public const int P_ENABLE_ROYAL_MAIL = 160;

	public const int P_ENABLE_KOREA_POST = 161;

	public const int P_ENABLE_NEC25 = 162;

	public const int P_ENABLE_IATA25 = 163;

	public const int P_ENABLE_CANADA_POST = 164;

	public const int P_ENABLE_BC412 = 165;

	public const int P_CHECKSUM_C39 = 200;

	public const int P_CHECKSUM_I25 = 201;

	public const int P_CHECKSUM_CBAR = 202;

	public const int P_FULL_ASCII_C39 = 203;

	public const int P_I25_LENGTH = 204;

	public const int P_DM_RECT = 205;

	public const int P_UPC_SUPPLEMENT = 206;

	public const int P_UPC_E_EXPANSION = 207;

	public const int P_POLARITY_LINEAR = 208;

	public const int P_POLARITY_DM = 209;

	public const int P_POLARITY_QR = 210;

	public const int P_POLARITY_AZ = 211;

	public const int P_POLARITY_GC = 212;

	public const int P_POLARITY_HX = 213;

	public const int P_POLARITY_GM = 214;

	public const int P_POLARITY_RESERVED2 = 215;

	public const int P_POLARITY_RESERVED3 = 216;

	public const int P_MIRROR_DM = 217;

	public const int P_MIRROR_QR = 218;

	public const int P_MIRROR_AZ = 219;

	public const int P_MIRROR_GC = 220;

	public const int P_MIRROR_HX = 221;

	public const int P_MIRROR_GM = 222;

	public const int P_MIRROR_RESERVED2 = 223;

	public const int P_MIRROR_RESERVED3 = 224;

	public const int P_DM_RECT_EXTENDED = 225;

	public const int P_CHECKSUM_C11 = 239;

	public const int P_CHECKSUM_C25 = 240;

	public const int P_CHECKSUM_MSI_PLE = 241;

	public const int P_STRIPCHAR_MSI_PLE = 242;

	public const int P_PHARMA_MIN_NUM_BARS = 243;

	public const int P_PHARMA_MAX_NUM_BARS = 244;

	public const int P_PHARMA_MIN_VALUE = 245;

	public const int P_PHARMA_MAX_VALUE = 246;

	public const int P_PHARMA_REV_DEC = 247;

	public const int P_PHARMA_COLOR_BARS = 248;

	public const int P_STRIPCHAR_C11 = 249;

	public const int P_UPCA_TO_EAN13 = 250;

	public const int P_EAN8_TO_EAN13 = 251;

	public const int P_BOOKLAND_TO_ISBN = 252;

	public const int P_BOOKLAND_TO_ISSN = 253;

	public const int P_STRIP_NUM_SYS_UPCA = 254;

	public const int P_STRIP_NUM_SYS_UPCE = 255;

	public const int P_STRIP_CHAR_UPCA = 256;

	public const int P_STRIP_CHAR_UPCE = 257;

	public const int P_STRIP_CHAR_EAN13 = 258;

	public const int P_STRIP_CHAR_EAN8 = 259;

	public const int P_STRIP_STARTSTOP_CBAR = 260;

	public const int P_SEND_STARTSTOP_C39 = 261;

	public const int P_SEND_STARTSTOP_TRI = 262;

	public const int P_DM_FOCUS_QUALITY = 263;

	public const int P_SEND_AIM_SYMB_ID = 264;

	public const int P_SEND_CBAR_ABC = 265;

	public const int P_SEND_CBAR_CX = 266;

	public const int P_REVERSE_TRIOPTIC = 267;

	public const int P_SEND_ECC_CODEWORDS = 268;

	public const int P_TELEPEN_OUTPUT_ASCII = 269;

	public const int P_SEND_UPC_AIM_MODIFIER = 270;

	public const int P_BC412_REV_DEC = 271;

	public const int P_SEND_ROYAL_MAIL_CHECK_CHAR = 272;

	public const int P_STRIP_AUSTRALIA_POST_CHECK_CHARS = 273;

	public const int P_OPERATION_MODE = 300;

	public const int P_STOP_DECODE = 301;

	public const int P_AGC_ROI_LEFT = 302;

	public const int P_AGC_ROI_TOP = 303;

	public const int P_AGC_ROI_WIDTH = 304;

	public const int P_AGC_ROI_HEIGHT = 305;

	public const int P_BC_ROI_LEFT = 306;

	public const int P_BC_ROI_TOP = 307;

	public const int P_BC_ROI_WIDTH = 308;

	public const int P_BC_ROI_HEIGHT = 309;

	public const int P_DECODE_TIME_LIMIT = 310;

	public const int P_SECURITY_LEVEL = 311;

	public const int P_TIME_OUT_FACTOR = 312;

	public const int P_TARGET_TOLERANCE_PERCENT = 313;

	public const int P_TARGET_LOCATION = 314;

	public const int P_MULTICODE_SPEED = 315;

	public const int P_ENSURE_ROI = 350;

	public const int P_BASIC_ETCH_DPM = 351;

	public const int P_BASIC_DOTS_DPM = 352;

	public const int P_CELL_IMAGE = 353;

	public const int P_CODE_RESERVED_4 = 354;

	public const int P_LOW_CONTRAST = 355;

	public const int P_CODE_RESERVED_5 = 356;

	public const int P_CODE_RESERVED_6 = 357;

	public const int P_CODE_RESERVED_11 = 358;

	public const int P_DPM_DOTPEEN_DL = 359;

	public const int P_DPM_DOTPEEN_LD = 360;

	public const int P_DPM_LASER_CHEM_ETCH = 361;

	public const int P_DPM_DOTPEEN2_LD = 362;

	public const int P_LONG_1D = 363;

	public const int P_ENHANCE_CONTRAST = 364;

	public const int P_DPM_SMALL_MIRRORED = 365;

	public const int P_QUICK_DECODE = 366;

	public const int P_NO_CR8000_SPEEDUP = 367;

	public const int P_NATIVE_DUAL_FIELD = 368;

	public const int P_MULTI_ALIGNED = 369;

	public const int P_BASIC_INKJET_DPM = 370;

	public const int P_BASIC_HANDHELD_DPM = 371;

	public const int P_HIGH_RES_SINGLE_LENS = 372;

	public const int P_VERSION_STRING_LENGTH = 398;

	public const int P_VERSION_STRING = 399;

	public const int P_RESULT_CORNERS = 400;

	public const int P_RESULT_CENTER = 401;

	public const int P_RESULT_SYMBOL_TYPE = 402;

	public const int P_RESULT_LENGTH = 403;

	public const int P_RESULT_STRING = 404;

	public const int P_RESULT_SYMBOL_MODIFIER = 405;

	public const int P_RESULT_LINKAGE = 406;

	public const int P_RESULT_QUALITY = 407;

	public const int P_RESULT_ECC_ERROR = 408;

	public const int P_RESULT_ECC_ERASURE = 409;

	public const int P_RESULT_DELTA_TIME = 410;

	public const int P_RESULT_TOTAL_TIME = 411;

	public const int P_RESULT_SYMBOL_TYPE_EX = 412;

	public const int P_RESULT_LOCATE_TIME = 413;

	public const int P_RESULT_DECODE_TIME = 414;

	public const int P_CODE_RESERVED_7 = 415;

	public const int P_CODE_RESERVED_8 = 416;

	public const int P_CODE_RESERVED_9 = 417;

	public const int P_CODE_RESERVED_10 = 418;

	public const int P_RESULT_DECODE_OUTPUT_FORMAT = 419;

	public const int P_RESULT_STATUS = 420;

	public const int P_RESULT_NUM_CODEWORDS = 421;

	public const int P_RESULT_CODEWORDS_BEFORE_ECC = 422;

	public const int P_RESULT_CODEWORDS_AFTER_ECC = 423;

	public const int P_RESULT_NUM_CODEWORDS_BLOCKS = 424;

	public const int P_RESULT_NUM_CODEWORDS_LONG_BLOCKS = 425;

	public const int P_RESULT_NUM_DATA_CODEWORDS = 426;

	public const int P_RESULT_NUM_EC_CODEWORDS = 427;

	public const int P_RESULT_STRUC_APPEND_POSITION = 428;

	public const int P_RESULT_STRUC_APPEND_TOTAL = 429;

	public const int P_RESULT_STRUC_APPEND_PARITY = 430;

	public const int P_RESULT_MISC_PROPERTY = 431;

	public const int P_RESULT_AGC_LIGHT_LEVEL = 432;

	public const int P_RESULT_SYMBOL_HEIGHT_WIDTH = 433;

	public const int P_RESULT_SYMBOL_ROWS_COLUMNS = 434;

	public const int P_RESULT_FOCUS_QUALITY = 499;

	public const int P_MIN_LENGTH_C39 = 500;

	public const int P_MIN_LENGTH_CBAR = 501;

	public const int P_MIN_LENGTH_C128 = 502;

	public const int P_MIN_LENGTH_C93 = 503;

	public const int P_THIS_SIZE = 504;

	public const int V_SYMB_GC = 1;

	public const int V_SYMB_DM = 2;

	public const int V_SYMB_QR_2005 = 4;

	public const int V_SYMB_AZ = 8;

	public const int V_SYMB_MC = 16;

	public const int V_SYMB_PDF = 32;

	public const int V_SYMB_MPDF = 64;

	public const int V_SYMB_CCA = 128;

	public const int V_SYMB_CCB = 256;

	public const int V_SYMB_CCC = 512;

	public const int V_SYMB_C39 = 1024;

	public const int V_SYMB_I25 = 2048;

	public const int V_SYMB_CBAR = 4096;

	public const int V_SYMB_C128 = 8192;

	public const int V_SYMB_C93 = 16384;

	public const int V_SYMB_UPCA = 32768;

	public const int V_SYMB_UPCE = 65536;

	public const int V_SYMB_EAN13 = 131072;

	public const int V_SYMB_EAN8 = 262144;

	public const int V_SYMB_DB_14 = 524288;

	public const int V_SYMB_DB_14_STA = 1048576;

	public const int V_SYMB_DB_LIM = 2097152;

	public const int V_SYMB_DB_EXP = 4194304;

	public const int V_SYMB_DB_EXP_STA = 8388608;

	public const int V_SYMB_HX = 16777216;

	public const int V_SYMB_QR_MICRO = 33554432;

	public const int V_SYMB_QR_MODEL1 = 67108864;

	public const int V_SYMB_GM = 134217728;

	public const int V_SYMB_CUSTOM_NC = 1073741824;

	public const int V_SYMB_CUSTOM_02 = 536870912;

	public const int V_SYMB_QR = 4;

	public const int V_SYMB_MQR = 33554432;

	public const int V_SYMB_EXTENDED = 0;

	public const int V_SYMB_C11 = 1;

	public const int V_SYMB_C32 = 2;

	public const int V_SYMB_PLE = 4;

	public const int V_SYMB_MSI_PLE = 8;

	public const int V_SYMB_TLP = 16;

	public const int V_SYMB_TRI = 32;

	public const int V_SYMB_PHA = 64;

	public const int V_SYMB_M25 = 128;

	public const int V_SYMB_S25 = 256;

	public const int V_SYMB_C49 = 512;

	public const int V_SYMB_C16K = 1024;

	public const int V_SYMB_CBLK = 2048;

	public const int V_SYMB_POSTNET = 4096;

	public const int V_SYMB_PLANET = 8192;

	public const int V_SYMB_INTEL_MAIL = 16384;

	public const int V_SYMB_AUSTRA_POST = 32768;

	public const int V_SYMB_DUTCH_POST = 65536;

	public const int V_SYMB_JAPAN_POST = 131072;

	public const int V_SYMB_ROYAL_MAIL = 262144;

	public const int V_SYMB_UPU = 524288;

	public const int V_SYMB_KOREA_POST = 1048576;

	public const int V_SYMB_HK25 = 2097152;

	public const int V_SYMB_NEC25 = 4194304;

	public const int V_SYMB_IATA25 = 8388608;

	public const int V_SYMB_CANADA_POST = 16777216;

	public const int V_SYMB_BC412 = 33554432;

	public const int V_SYMB_PRO1 = int.MinValue;

	public const int V_MISC_MIRROR_IMAGE = 1;

	public const int V_MISC_LIGHT_ON_DARK = 2;

	public const int V_POLARITY_DARK_ON_LIGHT = 1;

	public const int V_POLARITY_LIGHT_ON_DARK = -1;

	public const int V_POLARITY_EITHER = 0;

	public const int V_FALSE = 0;

	public const int V_TRUE = 1;

	public const int P_FORMAT_OUTPUT_OPTION = 800;

	public const int P_FORMAT_OUTPUT_CONFIG_STRING_LEN = 801;

	public const int P_FORMAT_OUTPUT_CONFIG_STRING = 802;

	public const int ERR_INVALID_HANDLE = 900;

	public const int ERR_INSUFFICIENT_MEMORY = 901;

	public const int ERR_INVALID_PROPERTY = 902;

	public const int ERR_INVALID_VALUE = 903;

	public const int ERR_RESERVED_1 = 904;

	public const int ERR_NO_SYMBOLOGY_ENABLED = 905;

	public const int ERR_MULTICODE_UNSUPPORTED = 906;

	public const int ERR_RESERVED_2 = 907;

	public const int ERR_RESERVED_3 = 908;

	public const int DECODE_QUIT = 999;

	public const int SEARCH_COMPLETED = 0;

	public const int FORMAT_OUTPUT_CONFIG_STRING_LEN = 2000;

	public const uint OEM_KEY_CUSTOMER = 2657668018u;

	private static List<CortexDecodeResult> Results = new List<CortexDecodeResult>();

	private ResultCallbackDelegate ResultCallback = delegate(int handle)
	{
		CortexDecodeResult cortexDecodeResult = new CortexDecodeResult
		{
			dataLength = GetInt(handle, 403)
		};
		byte[] bytes = GetBytes(handle, 404, cortexDecodeResult.dataLength);
		cortexDecodeResult.DecodeData = Encoding.ASCII.GetString(bytes, 0, cortexDecodeResult.dataLength);
		int[] ints = GetInts(handle, 400, 8);
		cortexDecodeResult.corner0 = new Point(ints[0], ints[1]);
		cortexDecodeResult.corner1 = new Point(ints[2], ints[3]);
		cortexDecodeResult.corner2 = new Point(ints[4], ints[5]);
		cortexDecodeResult.corner3 = new Point(ints[6], ints[7]);
		int[] ints2 = GetInts(handle, 401, 2);
		cortexDecodeResult.center = new Point(ints2[0], ints2[1]);
		cortexDecodeResult.symbolType = GetInt(handle, 402);
		cortexDecodeResult.symbolModifier = GetInt(handle, 405);
		Results.Add(cortexDecodeResult);
		return 0;
	};

	private readonly int handle = -1;

	[DllImport("CortexDecoder.dll")]
	private static extern int CRD_Create();

	[DllImport("CortexDecoder.dll")]
	private static extern int CRD_Destroy(int handle);

	[DllImport("CortexDecoder.dll")]
	private static extern int CRD_Decode(int handle);

	[DllImport("CortexDecoder.dll")]
	private unsafe static extern int CRD_Get(int handle, int property, void* value);

	[DllImport("CortexDecoder.dll")]
	private unsafe static extern int CRD_Set(int handle, int property, void* value);

	protected override void OnDispose(bool disposing)
	{
		CRD_Destroy(handle);
	}

	protected override void OnScan(ScanResult sr)
	{
		Results.Clear();
		using Bitmap bmp = (Bitmap)Image.FromFile(sr.ImagePath);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		int num = Decode(bmp);
		executionTimer.Stop();
		if (num == 0 && Results.Count > 0)
		{
			Results.ForEach(delegate(CortexDecodeResult each)
			{
				sr.Add(each.DecodeData);
			});
		}
		sr.ExecutionTime = TimeSpan.FromMilliseconds(executionTimer.ElapsedMilliseconds);
	}

	internal unsafe CortexDecoder()
		: base(BarcodeServices.Cortex)
	{
		LogHelper.Instance.Log("[CortexDecoder] CRD_Create start.");
		try
		{
			handle = CRD_Create();
			LogHelper.Instance.Log("[CortexDecoder] CRD_Create returned {0}", handle);
			if (handle > 0 && handle < 900)
			{
				int num = GetInt(handle, 9);
				bool flag = SetProperty(9, (void*)(num ^ 0x9E68CBB2u));
				IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(ResultCallback);
				base.IsLicensed = SetProperty(10, (void*)functionPointerForDelegate);
				LogHelper.Instance.Log($"[CortexDecoder] IsLicensed: {flag && base.IsLicensed}");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[CortexDecoder] CRD_Create caught an exception", e);
			base.IsLicensed = false;
		}
	}

	private unsafe int Decode(Bitmap bmp)
	{
		byte[] array = new byte[bmp.Width * bmp.Height];
		Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
		BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
		byte* ptr = (byte*)(void*)bitmapData.Scan0;
		int num = 0;
		int num2 = 2;
		for (int i = 0; i < bmp.Height; i++)
		{
			int num3 = num2;
			for (int j = 0; j < bmp.Width; j++)
			{
				array[num++] = ptr[num3];
				num3 += 3;
			}
			num2 += bitmapData.Stride;
		}
		bmp.UnlockBits(bitmapData);
		int num4;
		fixed (byte* data = &array[0])
		{
			SetProperty(0, data);
			SetProperty(1, (void*)bmp.Width);
			SetProperty(2, (void*)bmp.Height);
			SetProperty(3, (void*)bmp.Width);
			SetProperty(307, null);
			SetProperty(306, null);
			SetProperty(308, (void*)bmp.Width);
			SetProperty(309, (void*)bmp.Height);
			using ExecutionTimer executionTimer = new ExecutionTimer();
			num4 = CRD_Decode(handle);
			executionTimer.Stop();
			if (num4 != 0)
			{
				LogHelper.Instance.Log("[CortexDecoder] Decode returned error code {0}", num4);
			}
		}
		return num4;
	}

	private unsafe static byte[] GetBytes(int handle, int id, int maxlen)
	{
		byte[] array = new byte[maxlen];
		fixed (byte* value = &array[0])
		{
			CRD_Get(handle, id, value);
		}
		return array;
	}

	private unsafe static int[] GetInts(int handle, int id, int maxlen)
	{
		int[] array = new int[maxlen];
		fixed (int* value = &array[0])
		{
			CRD_Get(handle, id, value);
		}
		return array;
	}

	private unsafe static int GetInt(int handle, int id)
	{
		int result = default(int);
		CRD_Get(handle, id, &result);
		return result;
	}

	private unsafe bool SetProperty(int property, void* data)
	{
		int num = CRD_Set(handle, property, data);
		if (num != 0)
		{
			LogHelper.Instance.Log("[CortexDecoder] SetProperty ( handle = {0}, property = {1} ) returned {2}", handle, property, num);
			return false;
		}
		return true;
	}
}
