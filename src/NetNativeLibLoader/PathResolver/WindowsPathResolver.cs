//
//  WindowsPathResolver.cs
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
using System.Linq;
using System.Reflection;

namespace NetNativeLibLoader.PathResolver
{
    internal class WindowsPathResolver : IPathResolver
    {
        public ResolvePathResult Resolve(string library)
        {
            DirectoryInfo parent = null;
            string        libraryLocation;

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var entryLocation = entryAssembly.Location;
                parent = Directory.GetParent(!string.IsNullOrEmpty(entryLocation) ? entryLocation : Directory.GetCurrentDirectory());
            }

            if (parent != null)
            {
                var executingDir = parent.FullName;

                libraryLocation = Path.GetFullPath(Path.Combine(executingDir, library));
                if (File.Exists(libraryLocation))
                {
                    return ResolvePathResult.FromSuccess(libraryLocation);
                }
            }

            var sysDir = Environment.SystemDirectory;
            libraryLocation = Path.GetFullPath(Path.Combine(sysDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var sys16Dir = Path.Combine(windowsDir, "System");
            libraryLocation = Path.GetFullPath(Path.Combine(sys16Dir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            libraryLocation = Path.GetFullPath(Path.Combine(windowsDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var currentDir = Directory.GetCurrentDirectory();
            libraryLocation = Path.GetFullPath(Path.Combine(currentDir, library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (!(pathVar is null))
            {
                var pathDirs = pathVar.Split(';').Where(p => !string.IsNullOrEmpty(p));
                foreach (var path in pathDirs)
                {
                    libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                    if (File.Exists(libraryLocation))
                    {
                        return ResolvePathResult.FromSuccess(libraryLocation);
                    }
                }
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }
    }
}