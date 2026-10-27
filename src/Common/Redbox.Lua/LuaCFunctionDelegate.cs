using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int LuaCFunctionDelegate(IntPtr luaState);
