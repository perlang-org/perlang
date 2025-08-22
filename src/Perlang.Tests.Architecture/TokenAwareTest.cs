using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Perlang.Tests.Architecture;

/// <summary>
/// Ensures that all <see cref="Expr"/> and <see cref="Stmt"/> classes implement <see cref="ITokenAware"/>.
/// </summary>
public class TokenAwareTest
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(typeof(TokenAwareTest).Assembly, typeof(Stmt).Assembly)
            .Build();

    private static readonly IObjectProvider<Class> WhitelistedExprClasses =
        Classes()
            .That()
            .HaveFullNameContaining("Perlang.Expr+Empty")
            .Or()
            .HaveFullNameContaining("Perlang.Expr+Literal");

    [Fact]
    public void all_Expr_classes_implement_ITokenAware()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(Expr))
            .And()
            .AreNotAbstract()
            .And()
            .AreNot(WhitelistedExprClasses)
            .Should()
            .ImplementInterface(typeof(ITokenAware))
            .Check(Architecture);
    }
}