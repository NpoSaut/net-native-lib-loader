//
//  dl.cs
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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace NetNativeLibLoader.Loader;

internal static class dl
{
    public static IntPtr open(string fileName, SymbolFlag flags = SymbolFlag.RTLD_DEFAULT, byte libraryType = 0) =>
        libraryType switch
        {
            0 => BSD.dlopen(fileName, flags),
            1 => Unix.dlopen(fileName, flags),
            _ => UnixArm.dlopen(fileName, flags)
        };

    [Pure]
    public static IntPtr sym(IntPtr handle, string name, byte libraryType = 0) =>
        libraryType switch
        {
            0 => BSD.dlsym(handle, name),
            1 => Unix.dlsym(handle, name),
            _ => UnixArm.dlsym(handle, name)
        };

    public static int close(IntPtr handle, byte libraryType = 0) =>
        libraryType switch
        {
            0 => BSD.dlclose(handle),
            1 => Unix.dlclose(handle),
            _ => UnixArm.dlclose(handle)
        };

    public static IntPtr error(byte libraryType = 0) =>
        libraryType switch
        {
            0 => BSD.dlerror(),
            1 => Unix.dlerror(),
            _ => UnixArm.dlerror()
        };

    public static void ResetError(byte libraryType = 0)
    {
        // Clear any outstanding errors by looping until no error is found
        while (error(libraryType) != IntPtr.Zero)
        {
        }
    }

    private const string LibraryNameArmUnix = "dl";
    private const string LibraryNameUnix    = "dl.so.2";
    private const string LibraryNameBSD     = "c";

    private static class Unix
    {
        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(LibraryNameUnix)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlerror();
    }
    
    private static class UnixArm
    {
        [DllImport(LibraryNameArmUnix)]
        public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

        [DllImport(LibraryNameArmUnix)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(LibraryNameArmUnix)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(LibraryNameArmUnix)]
        public static extern IntPtr dlerror();
    }

    private static class BSD
    {
        [DllImport(LibraryNameBSD)]
        public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

        [DllImport(LibraryNameBSD)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(LibraryNameBSD)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(LibraryNameBSD)]
        public static extern IntPtr dlerror();
    }
}