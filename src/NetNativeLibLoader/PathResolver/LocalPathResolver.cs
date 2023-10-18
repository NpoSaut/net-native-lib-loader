//
//  LocalPathResolver.cs
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
using System.IO;
using System.Reflection;

namespace NetNativeLibLoader.PathResolver
{
    internal class LocalPathResolver : IPathResolver
    {
        private readonly string _entryAssemblyDirectory;
        private readonly string _executingAssemblyDirectory;
        private readonly string _currentDirectory;

        public LocalPathResolver()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var entryLocation = entryAssembly.Location;
                if (!string.IsNullOrEmpty(entryLocation))
                {
                    var parent = Directory.GetParent(entryLocation);
                    _entryAssemblyDirectory = parent != null ? parent.FullName : entryLocation;
                }
                else
                {
                    var parent = Directory.GetParent(Directory.GetCurrentDirectory());
                    _entryAssemblyDirectory = parent != null ? parent.FullName : Directory.GetCurrentDirectory(); 
                }
            }
            else
            {
                _entryAssemblyDirectory = null;
            }

            var executingLocation = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(executingLocation))
            {
                var parent = Directory.GetParent(executingLocation);
                _executingAssemblyDirectory = parent != null ? parent.FullName : executingLocation;
            }
            else
            {
                var parent = Directory.GetParent(Directory.GetCurrentDirectory());
                _executingAssemblyDirectory = parent != null ? parent.FullName : Directory.GetCurrentDirectory(); 
            }

            _currentDirectory = Directory.GetCurrentDirectory();
        }

        public ResolvePathResult Resolve(string library)
        {
            // First, check next to the entry executable
            if (!(_entryAssemblyDirectory is null))
            {
                var result = ScanPathForLibrary(_entryAssemblyDirectory, library);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            if (!(_executingAssemblyDirectory is null))
            {
                var result = ScanPathForLibrary(_executingAssemblyDirectory, library);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            // Then, check the current directory
            if (!(_currentDirectory is null))
            {
                var result = ScanPathForLibrary(_currentDirectory, library);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return ResolvePathResult.FromError(new FileNotFoundException("No local copy of the given library could be found.", library));
        }

        private ResolvePathResult ScanPathForLibrary(string path, string library)
        {
            var libraryLocation = Path.GetFullPath(Path.Combine(path, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            // Check the local library directory
            libraryLocation = Path.GetFullPath(Path.Combine(path, "lib", library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            // Check platform-specific directory
            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            libraryLocation = Path.GetFullPath(Path.Combine(path, "lib", bitness, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("No local copy of the given library could be found.", library));
        }
    }
}