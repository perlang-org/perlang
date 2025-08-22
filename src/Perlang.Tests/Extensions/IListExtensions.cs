using System;
using System.Collections.Generic;

namespace Perlang.Tests.Extensions;

/// <summary>
/// Extension methods for <see cref="IList{T}"/>.
/// </summary>
public static class IListExtensions
{
    public static T Second<T>(this IList<T> list)
    {
        return list.Count switch
        {
            0 => throw new InvalidOperationException("List does not contain any elements"),
            1 => throw new InvalidOperationException("List contains only one element"),
            _ => list[1]
        };
    }
}