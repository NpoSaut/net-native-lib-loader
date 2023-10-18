//
//  LinuxPathResolver.cs
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

namespace NetNativeLibLoader.PathResolver;

internal class LinuxPathResolver : IPathResolver
{
    public ResolvePathResult Resolve(string library)
    {
        var libraryPaths = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")?.Split(':').Where(p => !string.IsNullOrEmpty(p));

        string libraryLocation;

        if (libraryPaths != null)
            foreach (var path in libraryPaths)
            {
                libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                if (File.Exists(libraryLocation))
                    return ResolvePathResult.FromSuccess(libraryLocation);
            }

        if (File.Exists("/etc/ld.so.cache"))
        {
            var cachedLibraries = File.ReadAllText("/etc/ld.so.cache").Split('\0');
            var cachedMatch     = cachedLibraries.FirstOrDefault(l => l.EndsWith(library) && Path.GetFileName(l) == Path.GetFileName(library));

            if (cachedMatch != null)
                return ResolvePathResult.FromSuccess(cachedMatch);
        }

        libraryLocation = Path.GetFullPath(Path.Combine("/lib", library));
        if (File.Exists(libraryLocation))
            return ResolvePathResult.FromSuccess(libraryLocation);

        libraryLocation = Path.GetFullPath(Path.Combine("/usr/lib", library));
        return File.Exists(libraryLocation)
                   ? ResolvePathResult.FromSuccess(libraryLocation)
                   : ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
    }
}