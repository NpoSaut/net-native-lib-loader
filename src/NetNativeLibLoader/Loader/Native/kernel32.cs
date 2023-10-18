//
//  kernel32.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace NetNativeLibLoader.Loader;

internal static class kernel32
{
    [DllImport("kernel32", SetLastError = true)]
    public static extern HMODULE LoadLibrary(string fileName);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    [Pure]
    public static extern FARPROC GetProcAddress(HMODULE module, string procName);

    [DllImport("kernel32", SetLastError = true)]
    public static extern int FreeLibrary(HMODULE module);
}