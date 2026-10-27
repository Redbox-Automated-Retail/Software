using System;
using System.Collections;
using ClearImage;

namespace RBDM;

public class clsRBDM
{
	private const int m_a = 32;

	private const int m_b = 64;

	private const int c = 128;

	private const int d = 1;

	private const int e = 8192;

	private const int f = 5;

	private const int g = 4;

	public CiServer Ci;

	public CiImage Image;

	public CiBarcodes Barcodes;

	private CiDataMatrix h;

	private CiTools i;

	private CiImage j;

	private CiAdvColor k;

	private int l;

	public int Find(int maxBcIn)
	{
		switch (0)
		{
		default:
		{
			int num = 1;
			int result = default(int);
			while (true)
			{
				switch (num)
				{
				case 0:
					try
					{
						string text = ((ICiServer)Ci).get_Info((EInfoType)7745, 1061);
						text = ((ICiServer)Ci).get_Info((EInfoType)7746, 0);
						result = 0;
						int.TryParse(text, out result);
						text = ((ICiServer)Ci).get_Info((EInfoType)7747, result ^ 0x425);
						i = Ci.CreateTools();
						k = Ci.CreateAdvColor();
						l = maxBcIn;
						return a(Image);
					}
					catch (Exception ex)
					{
						throw ex;
					}
					finally
					{
						while (true)
						{
							IL_0115:
							i = null;
							k = null;
							((ICiServer)Ci).get_Info((EInfoType)7748, result ^ 0x425);
							num = 2;
							while (true)
							{
								switch (num)
								{
								case 2:
									if (j != null)
									{
										num = 1;
										continue;
									}
									goto case 0;
								case 1:
									j.Close();
									num = 0;
									continue;
								case 0:
									GC.Collect();
									GC.WaitForPendingFinalizers();
									goto end_IL_0103;
								}
								goto IL_0115;
								continue;
								end_IL_0103:
								break;
							}
							break;
						}
					}
				case 4:
					throw new Exception("RBDM Image is not intialized");
				case 3:
					throw new Exception("RBDM Server is not intialized");
				case 2:
					_ = 0;
					if (Image == null)
					{
						num = 4;
						continue;
					}
					a();
					result = 0;
					num = 0;
					continue;
				}
				switch (1 == 1)
				{
				default:
					if (true)
					{
					}
					if (false)
					{
					}
					if (Ci != null)
					{
						break;
					}
					num = 3;
					continue;
				case false:
				case true:
					break;
				}
				num = 2;
			}
		}
		}
	}

	private bool a()
	{
		while (h != null)
		{
			_ = 0;
			switch (1 == 1)
			{
			case false:
			case true:
				continue;
			}
			if (true)
			{
			}
			if (false)
			{
			}
			return true;
		}
		try
		{
			h = Ci.CreateDataMatrix();
			return true;
		}
		catch (Exception ex)
		{
			h = null;
			throw ex;
		}
	}

	private bool a(CiRect A_0, long A_1, long A_2)
	{
		int num = 6;
		while (true)
		{
			switch (num)
			{
			default:
				if (A_1 >= A_0.left)
				{
					num = 5;
					break;
				}
				goto IL_00d5;
			case 2:
				if (A_2 <= A_0.bottom)
				{
					return true;
				}
				goto IL_0059;
			case 3:
				if (A_2 >= A_0.top)
				{
					num = 7;
					break;
				}
				goto case 1;
			case 7:
				_ = 0;
				num = 2;
				break;
			case 5:
				num = 4;
				break;
			case 4:
				num = ((A_1 <= A_0.right) ? 3 : 0);
				break;
			case 0:
				switch (1 == 1)
				{
				case false:
				case true:
					break;
				default:
					goto IL_00b7;
				}
				goto IL_0059;
			case 1:
				{
					return false;
				}
				IL_00b7:
				if (true)
				{
				}
				if (0 == 0)
				{
				}
				goto IL_00d5;
				IL_0059:
				num = 1;
				break;
				IL_00d5:
				return false;
			}
		}
	}

	private bool a(CiBarcode A_0, CiImage A_1)
	{
		switch (0)
		{
		default:
			{
				IEnumerator enumerator = Barcodes.GetEnumerator();
				bool result = default(bool);
				try
				{
					int num = 5;
					CiBarcode ciBarcode = default(CiBarcode);
					while (true)
					{
						switch (num)
						{
						case 4:
							break;
						default:
							switch (1 == 1)
							{
							case false:
							case true:
								goto IL_00a6;
							}
							if (0 == 0)
							{
							}
							goto IL_007e;
						case 1:
							if (!enumerator.MoveNext())
							{
								num = 6;
								continue;
							}
							ciBarcode = (CiBarcode)enumerator.Current;
							num = 0;
							continue;
						case 0:
							goto IL_00a6;
						case 3:
							result = false;
							num = 2;
							continue;
						case 2:
							goto end_IL_002f;
						case 6:
							{
								num = 4;
								continue;
							}
							IL_00a6:
							if (a(ciBarcode.Rect, (A_0.Rect.left + A_0.Rect.right) / 2, (A_0.Rect.top + A_0.Rect.bottom) / 2))
							{
								num = 3;
								continue;
							}
							goto IL_007e;
							IL_007e:
							num = 1;
							continue;
						}
						break;
					}
					goto IL_0029;
					end_IL_002f:;
				}
				finally
				{
					while (true)
					{
						IL_0125:
						IDisposable disposable = enumerator as IDisposable;
						int num = 2;
						while (true)
						{
							switch (num)
							{
							case 2:
								if (disposable != null)
								{
									num = 0;
									continue;
								}
								goto end_IL_0113;
							case 0:
								disposable.Dispose();
								num = 1;
								continue;
							case 1:
								goto end_IL_0113;
							}
							goto IL_0125;
							continue;
							end_IL_0113:
							break;
						}
						break;
					}
				}
				if (true)
				{
				}
				return result;
			}
			IL_0029:
			_ = 0;
			return true;
		}
	}

	private long a(CiImage A_0, int A_1, int A_2, int A_3, int A_4, int A_5)
	{
		switch (0)
		{
		}
		CiBarcode ciBarcode = default(CiBarcode);
		IEnumerator enumerator = default(IEnumerator);
		while (true)
		{
			int num = 9;
			while (true)
			{
				switch (num)
				{
				case 9:
					if (A_1 != 32)
					{
						num = 0;
						continue;
					}
					goto case 4;
				case 8:
					i.Image = A_0;
					i.Fatten(1, (EMorphDirections)8192);
					num = 6;
					continue;
				case 2:
					k.Image = A_0;
					k.ConvertToBitonal(EBiTonalConversion.ciBtcEdgeOne, A_2, A_3, A_4);
					num = 5;
					continue;
				case 12:
					num = 11;
					continue;
				case 11:
					if (A_1 == 128)
					{
						num = 7;
						continue;
					}
					goto case 4;
				case 13:
					try
					{
						num = 5;
						while (true)
						{
							switch (num)
							{
							case 6:
								Barcodes.Add(ciBarcode);
								num = 3;
								continue;
							case 2:
								if (a(ciBarcode, A_0))
								{
									num = 6;
									continue;
								}
								goto default;
							default:
								num = 0;
								continue;
							case 0:
								if (enumerator.MoveNext())
								{
									ciBarcode = (CiBarcode)enumerator.Current;
									num = 2;
								}
								else
								{
									num = 1;
								}
								continue;
							case 1:
								num = 4;
								continue;
							case 4:
								break;
							}
							break;
						}
					}
					finally
					{
						while (true)
						{
							IL_017d:
							IDisposable disposable = enumerator as IDisposable;
							num = 2;
							while (true)
							{
								switch (num)
								{
								case 2:
									if (disposable != null)
									{
										num = 1;
										continue;
									}
									goto end_IL_016b;
								case 1:
									disposable.Dispose();
									num = 0;
									continue;
								case 0:
									goto end_IL_016b;
								}
								goto IL_017d;
								continue;
								end_IL_016b:
								break;
							}
							break;
						}
					}
					switch (1 == 1)
					{
					case false:
					case true:
						break;
					default:
						if (false)
						{
						}
						return Barcodes.Count;
					}
					goto case 4;
				case 0:
					num = 10;
					continue;
				case 10:
					num = ((A_1 == 64) ? 15 : 12);
					continue;
				case 6:
					h.Image = A_0;
					h.Find(l);
					enumerator = h.Barcodes.GetEnumerator();
					num = 13;
					continue;
				case 15:
					if (A_0.BitsPerPixel > 1)
					{
						_ = 0;
						num = 2;
						continue;
					}
					goto case 4;
				case 7:
					if (true)
					{
					}
					num = 3;
					continue;
				case 3:
					if (A_0.BitsPerPixel > 1)
					{
						num = 1;
						continue;
					}
					goto case 4;
				case 4:
				case 5:
					num = 14;
					continue;
				case 14:
					if ((A_5 & 1) != 0)
					{
						num = 8;
						continue;
					}
					goto case 6;
				case 1:
					k.Image = A_0;
					k.ConvertToBitonal(EBiTonalConversion.ciBtcLocalThr, A_2, A_3, A_4);
					num = 4;
					continue;
				}
				break;
			}
		}
	}

	private int b(CiImage A_0)
	{
		while (true)
		{
			a(A_0, 32, 0, 0, 0, 0);
			int num = 6;
			while (true)
			{
				switch (num)
				{
				case 6:
					if (l != 0)
					{
						num = 5;
						continue;
					}
					goto IL_0154;
				case 7:
					if (l != 0)
					{
						goto IL_00ae;
					}
					goto IL_0193;
				case 11:
					return Barcodes.Count;
				case 8:
					return Barcodes.Count;
				case 4:
					return Barcodes.Count;
				case 5:
					num = 1;
					continue;
				case 1:
					if (Barcodes.Count >= l)
					{
						num = 8;
						continue;
					}
					goto IL_0154;
				case 2:
					num = 0;
					continue;
				case 0:
					if (Barcodes.Count >= l)
					{
						num = 4;
						continue;
					}
					goto IL_0193;
				case 10:
					num = 3;
					continue;
				case 3:
					if (Barcodes.Count >= l)
					{
						num = 11;
						continue;
					}
					goto IL_0061;
				case 9:
					{
						if (l != 0)
						{
							_ = 0;
							num = 10;
							continue;
						}
						goto IL_0061;
					}
					IL_00ae:
					num = 2;
					continue;
					IL_0193:
					return Barcodes.Count;
					IL_0061:
					a(A_0, 32, 0, 0, 0, 1);
					switch (1 == 1)
					{
					default:
						if (false)
						{
						}
						if (true)
						{
						}
						num = 7;
						continue;
					case false:
					case true:
						break;
					}
					goto IL_00ae;
					IL_0154:
					a(A_0, 128, 0, 50, 0, 0);
					num = 9;
					continue;
				}
				break;
			}
		}
	}

	private int a(CiImage A_0)
	{
		int count = default(int);
		while (true)
		{
			CiImage ciImage = null;
			Barcodes = Ci.CreateBarcodes();
			try
			{
				while (true)
				{
					IL_004f:
					h.Image = A_0;
					ciImage = A_0.Duplicate();
					k.Image = ciImage;
					k.ConvertToGrayscale();
					b(ciImage);
					int num = 1;
					while (true)
					{
						switch (num)
						{
						case 1:
							if (l != 0)
							{
								num = 9;
								continue;
							}
							goto IL_011b;
						case 0:
							count = Barcodes.Count;
							num = 5;
							continue;
						case 5:
							goto end_IL_001d;
						case 2:
							count = Barcodes.Count;
							num = 10;
							continue;
						case 10:
							goto end_IL_001d;
						case 9:
							num = 7;
							continue;
						case 7:
							if (Barcodes.Count >= l)
							{
								num = 0;
								continue;
							}
							goto IL_011b;
						case 8:
							num = 4;
							continue;
						case 4:
							if (Barcodes.Count >= l)
							{
								num = 2;
								continue;
							}
							goto IL_015e;
						case 6:
							if (l != 0)
							{
								num = 8;
								continue;
							}
							goto IL_015e;
						case 3:
							goto end_IL_001d;
							IL_015e:
							count = Barcodes.Count;
							num = 3;
							continue;
							IL_011b:
							i.Image = ciImage;
							i.Skew(5.0);
							b(ciImage);
							num = 6;
							continue;
						}
						goto IL_004f;
						continue;
						end_IL_001d:
						break;
					}
					break;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				int num = 9;
				while (true)
				{
					switch (num)
					{
					default:
						if (ciImage != null)
						{
							num = 12;
							continue;
						}
						goto case 6;
					case 19:
						num = 4;
						continue;
					case 4:
						if (i.Image.IsValid == EBoolean.ciTrue)
						{
							num = 10;
							continue;
						}
						goto case 16;
					case 6:
						num = 1;
						continue;
					case 1:
						if (h != null)
						{
							num = 8;
							continue;
						}
						goto case 18;
					case 13:
						num = 5;
						continue;
					case 5:
						if (k.Image.IsValid == EBoolean.ciTrue)
						{
							num = 17;
							continue;
						}
						break;
					case 17:
						k.Image.Close();
						num = 0;
						continue;
					case 0:
						break;
					case 3:
						h.Image.Close();
						num = 18;
						continue;
					case 12:
						num = 7;
						continue;
					case 7:
						if (ciImage.IsValid == EBoolean.ciTrue)
						{
							num = 14;
							continue;
						}
						goto case 6;
					case 14:
						if (true)
						{
						}
						ciImage.Close();
						num = 6;
						continue;
					case 8:
						num = 2;
						continue;
					case 2:
						if (h.Image.IsValid == EBoolean.ciTrue)
						{
							num = 3;
							continue;
						}
						goto case 18;
					case 18:
						num = 11;
						continue;
					case 11:
						if (i != null)
						{
							num = 19;
							continue;
						}
						goto case 16;
					case 16:
						num = 15;
						continue;
					case 15:
						if (k != null)
						{
							num = 13;
							continue;
						}
						break;
					case 10:
						i.Image.Close();
						num = 16;
						continue;
					}
					break;
				}
			}
			_ = 0;
			switch (1 == 1)
			{
			case false:
			case true:
				continue;
			}
			if (false)
			{
			}
			return count;
		}
	}
}
