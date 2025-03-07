using System.Collections.Generic;
using Perlang.Collections;

namespace Perlang;

public static class IEnumerableExtensions
{
    public static StringHashSet ToPerlangStringHashSet(this IEnumerable<string> list)
    {
        using var mutableStringHashSet = new MutableStringHashSet();

        foreach (string value in list)
        {
            mutableStringHashSet.Add(value);
        }

        var result = new StringHashSet(mutableStringHashSet);

        return result;
    }
}
