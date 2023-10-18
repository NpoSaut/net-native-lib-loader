//
//  DynamicLinkLibraryPathResolver.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NetNativeLibLoader.PathResolver;

public class DynamicLinkLibraryPathResolver : IPathResolver
{
    public ResolvePathResult Resolve(string library) => ResolveAbsolutePath(library, SearchLocalFirst);

    static DynamicLinkLibraryPathResolver()
    {
        _localPathResolver = new LocalPathResolver();
        _pathResolver      = SelectPathResolver();
    }

    public DynamicLinkLibraryPathResolver(bool searchLocalFirst = true) => SearchLocalFirst = searchLocalFirst;

    private static readonly IPathResolver _localPathResolver;
    private static readonly IPathResolver _pathResolver;

    private bool SearchLocalFirst { get; }

    private static IPathResolver SelectPathResolver()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsPathResolver();

        /*
            Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
            "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
        */
        var isBsd = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBsd)
            return new LinuxPathResolver();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsPathResolver();

        throw new PlatformNotSupportedException($"Cannot resolve linker paths on this platform: {RuntimeInformation.OSDescription}");
    }

    private ResolvePathResult ResolveAbsolutePath(string library, bool localFirst)
    {
        var candidates = GenerateLibraryCandidates(library).ToList();

        if (library.IsValidPath())
            foreach (var candidate in candidates.Where(File.Exists))
                return ResolvePathResult.FromSuccess(Path.GetFullPath(candidate));

        // Check the native probing paths (.NET Core defines this, Mono doesn't. Users can set this at runtime, too)
        if (AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is string directories)
            foreach (var path in directories.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
            foreach (var candidatePath in candidates.Select(candidate => Path.Combine(path, candidate)).Where(File.Exists))
                return ResolvePathResult.FromSuccess(Path.GetFullPath(candidatePath));

        if (localFirst)
            foreach (var result in candidates.Select(candidate => _localPathResolver.Resolve(candidate)).Where(result => result.IsSuccess))
                return result;

        foreach (var result in candidates.Select(candidate => _pathResolver.Resolve(candidate)).Where(result => result.IsSuccess))
            return result;

        return library == "__Internal"
                   ? ResolvePathResult.FromSuccess(null)
                   : // Mono extension: Search the main program. Allowed for all runtimes
                   ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
    }

    private static IEnumerable<string> GenerateLibraryCandidates(string library)
    {
        var doesLibraryContainPath = false;

        if (library.IsValidPath())
        {
            library                = Path.GetFileName(library);
            doesLibraryContainPath = true;
        }

        var candidates = new List<string> { library };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !library.EndsWith(".dll"))
            candidates.AddRange(GenerateWindowsCandidates(library));

        var isBsd = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBsd || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            candidates.AddRange(GenerateUnixCandidates(library));

        // If we have a parent path we're looking at, mutate the candidate list to include the parent path
        if (doesLibraryContainPath)
            candidates = candidates.Select(c => Path.Combine(Path.GetDirectoryName(library) ?? string.Empty, c)).ToList();

        return candidates;
    }

    private static IEnumerable<string> GenerateWindowsCandidates(string library)
    {
        yield return $"{library}.dll";
    }

    private static IEnumerable<string> GenerateUnixCandidates(string library)
    {
        const string prefix = "lib";

        var suffix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

        var noSuffix = !library.EndsWith(suffix);
        var noPrefix = !Path.GetFileName(library).StartsWith(prefix);

        if (noSuffix)
            yield return $"{library}{suffix}";

        if (noPrefix)
            yield return $"{prefix}{library}";

        if (noPrefix && noSuffix)
            yield return $"{prefix}{library}{suffix}";
    }
}