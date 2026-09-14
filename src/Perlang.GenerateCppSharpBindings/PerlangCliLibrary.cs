#pragma warning disable SA1010

using System.Collections.Generic;
using CppSharp;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators;
using CppSharp.Parser;
using CppSharp.Passes;

namespace Perlang.GenerateCppSharpBindings;

internal class PerlangCliLibrary : ILibrary
{
    private readonly List<(Class Class, Property Property)> removedProperties = [];

    public void Preprocess(Driver driver, ASTContext ctx)
    {
        // Cannot be setup in SetupPasses, because the pass needs to run after GetterSetterToPropertyPass has converted
        // methods to properties.
        driver.AddTranslationUnitPass(new RemoveAdvancePropertyPass(removedProperties));

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

        // Disabled since it depends on BigInt, which is also disabled
        ctx.IgnoreClassWithName("BigIntRange");

        // Disabled since they are not needed on the C# side
        ctx.IgnoreClassWithName("Error");
        ctx.IgnoreClassWithName("ArgumentError");

        // CppSharp caches the vtable of the first IToken instance it sees, so calling these through the generated
        // bindings dispatches to the wrong implementation for other token types. get_token_type() and get_token_line()
        // are used instead.
        ctx.IgnoreClassMethodWithName("IToken", "type");
        ctx.IgnoreClassMethodWithName("IToken", "line");

        ctx.IgnoreFunctionWithPattern("mp_*");
        ctx.IgnoreFunctionWithPattern("Mp*");
    }

    public void Postprocess(Driver driver, ASTContext ctx)
    {
        foreach ((Class @class, Property property) in removedProperties) {
            @class.Properties.Remove(property);
        }
    }

    public void Setup(Driver driver)
    {
        var options = driver.Options;
        options.GeneratorKind = GeneratorKind.CSharp;
        options.OutputDir = "src/Perlang.Common";

        // Defaults to CPP14_GNU, where std::optional is not available
        driver.ParserOptions.LanguageVersion = LanguageVersion.CPP17_GNU;

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
        driver.AddTranslationUnitPass(new IgnoreOptionalFields());
    }

    // The layout structs CppSharp generates for std::optional refer to a _Storage union which it never emits, so we
    // leave out fields of a 'T | null' type from the generated bindings. We access them using manual C++ wrappers
    // instead.
    private class IgnoreOptionalFields : TranslationUnitPass
    {
        public override bool VisitClassDecl(Class @class)
        {
            if (@class.Name is "optional" or "_Optional_payload" or "_Optional_payload_base") {
                @class.ExplicitlyIgnore();
            }

            // Removes the layout field for ignored classes.
            @class.Layout?.Fields.RemoveAll(f => IsOptional(f.QualifiedType.Type));

            return base.VisitClassDecl(@class);
        }

        public override bool VisitFieldDecl(Field field)
        {
            if (IsOptional(field.Type)) {
                field.ExplicitlyIgnore();
            }

            return base.VisitFieldDecl(field);
        }

        private static bool IsOptional(CppSharp.AST.Type type) =>
            type.Desugar().TryGetClass(out Class @class) && @class.Name == "optional";
    }

    private class FixEnumsNamespace : TranslationUnitPass
    {
        public override bool VisitEnumDecl(Enumeration @enum)
        {
            @enum.Namespace.Name = "Perlang";
            return base.VisitEnumDecl(@enum);
        }
    }

    private class RemoveAdvancePropertyPass : TranslationUnitPass
    {
        private readonly List<(Class, Property)> removedProperties;

        public RemoveAdvancePropertyPass(List<(Class Class, Property Property)> removedProperties)
        {
            this.removedProperties = removedProperties;
        }

        public override bool VisitProperty(Property property)
        {
            if (property.Name == "Advance") {
                property.GetMethod.GenerationKind = GenerationKind.Generate;

                // Cannot remove at this stage since it will cause "Collection was modified" exceptions
                removedProperties.Add(((Class)property.Namespace, property));
            }

            return base.VisitProperty(property);
        }
    }
}
