using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow;

public static class SystemTools
{
	public unsafe static IntPtr CopyUnmanagedMemory(IntPtr dst, IntPtr src, int count)
	{
		CopyUnmanagedMemory((byte*)dst.ToPointer(), (byte*)src.ToPointer(), count);
		return dst;
	}

	public unsafe static byte* CopyUnmanagedMemory(byte* dst, byte* src, int count)
	{
		return memcpy(dst, src, count);
	}

	public unsafe static IntPtr SetUnmanagedMemory(IntPtr dst, int filler, int count)
	{
		SetUnmanagedMemory((byte*)dst.ToPointer(), filler, count);
		return dst;
	}

	public unsafe static byte* SetUnmanagedMemory(byte* dst, int filler, int count)
	{
		return memset(dst, filler, count);
	}

	[DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
	private unsafe static extern byte* memcpy(byte* dst, byte* src, int count);

	[DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
	private unsafe static extern byte* memset(byte* dst, int filler, int count);
}
