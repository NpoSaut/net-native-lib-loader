//
//  ResolvePathResut.cs
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
using System.Diagnostics.CodeAnalysis;

namespace NetNativeLibLoader.PathResolver;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public struct ResolvePathResult
{
    private readonly bool? _isSuccess;

    public string?    Path        { get; }
    public string?    ErrorReason { get; }
    public Exception? Exception   { get; }

    public readonly bool IsSuccess => _isSuccess.HasValue && _isSuccess.Value;

    private ResolvePathResult(string? path, string? errorReason, bool? isSuccess, Exception? exception)
    {
        Path        = path;
        ErrorReason = errorReason;
        _isSuccess  = isSuccess;
        Exception   = exception;
    }

    public static ResolvePathResult FromSuccess(string? resolvedPath) =>
        new(resolvedPath, null, true, null);

    public static ResolvePathResult FromError(string? errorReason) =>
        new(null, errorReason, false, null);

    public static ResolvePathResult FromError(Exception? exception) =>
        new(null, exception?.Message, false, exception);
}