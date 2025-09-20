using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;

namespace Perlang.GenerateCppSharpBindings;

internal class PerlangCliLibrary : ILibrary
{
    public void Preprocess(Driver driver, ASTContext ctx)
    {
        ctx.IgnoreTranslationUnits(["tommath.h"]);
        ctx.IgnoreClassWithName("BigInt");
        ctx.IgnoreClassWithName("String");

        // Disabled since they depend on Delegates class which doesn't seem to be included in generated C# binding code
        // (CppSharp bug?)
        ctx.IgnoreClassWithName("ASCIIString");
        ctx.IgnoreClassWithName("NullPointerException");
        ctx.IgnoreClassWithName("UTF8String");
        ctx.IgnoreClassWithName("UTF16String");

        // Disabled since PerlangValueTypes exist on both the C++ and C# side
        ctx.IgnoreClassWithName("PerlangValueTypes");

        ctx.IgnoreFunctionWithPattern("mp_*");
        ctx.IgnoreFunctionWithPattern("Mp*");
    }

    public void Postprocess(Driver driver, ASTContext ctx)
    {
        // no-op
    }

    public void Setup(Driver driver)
    {
        var options = driver.Options;
        options.GeneratorKind = GeneratorKind.CSharp;
        options.OutputDir = "src/Perlang.Common";

        // Useful when debugging, but generates loads of output.
        //options.Verbose = true;

        var module = options.AddModule("PerlangCli");
        module.SharedLibraryName = "perlang_cli";
        module.OutputNamespace = "";
        module.IncludeDirs.Add("src/perlang_cli/src");
        module.IncludeDirs.Add("src/stdlib/src");
        module.Headers.Add("perlang_cli.h");
        module.Headers.Add("string_token_type_dictionary.h");
    }

    public void SetupPasses(Driver driver)
    {
        // This avoids the *-symbols.cpp files from being generated
        var symbolsPass = driver.Context.TranslationUnitPasses.FindPass<GenerateSymbolsPass>();
        driver.Context.TranslationUnitPasses.RemovePass(symbolsPass);

        driver.AddTranslationUnitPass(new FixEnumsNamespace());
    }

    private class FixEnumsNamespace : TranslationUnitPass
    {
        public override bool VisitEnumDecl(Enumeration @enum)
        {
            @enum.Namespace.Name = "Perlang";
            return base.VisitEnumDecl(@enum);
        }
    }
}
