using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Perlang.Tests.Common;

public class CppTypeTests
{
    [Fact]
    public void two_CppTypes_with_same_name_are_compared_as_equal()
    {
        var name1 = new CppType("Name", "Name", wrapInSharedPtr: true);
        var name2 = new CppType("Name", "Name", wrapInSharedPtr: true);

        name1.Equals(name2).Should()
            .BeTrue();
    }

    [Fact]
    public void two_CppTypes_with_same_name_are_consider_equal_by_Linq_Distinct()
    {
        var name1 = new CppType("Name", "Name", wrapInSharedPtr: true);
        var name2 = new CppType("Name", "Name", wrapInSharedPtr: true);

        var list = new List<CppType> { name1, name2 };

        list.Distinct()
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(name1);
    }
}
