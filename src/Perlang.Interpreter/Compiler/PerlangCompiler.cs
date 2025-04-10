#nullable enable
#pragma warning disable S112
#pragma warning disable SA1118

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Perlang.Attributes;
using Perlang.Compiler;
using Perlang.Exceptions;
using Perlang.Internal.Extensions;
using Perlang.Interpreter.CodeAnalysis;
using Perlang.Interpreter.Immutability;
using Perlang.Interpreter.Internals;
using Perlang.Interpreter.NameResolution;
using Perlang.Interpreter.Typing;
using Perlang.Lang;
using Perlang.Native;
using Perlang.Parser;
using static Perlang.TokenType;
using String = System.String;

namespace Perlang.Interpreter.Compiler;

// TODO: This class throws a bunch of RuntimeErrors which should probably be PerlangCompilerException instead. Investigate
// TODO: this; will likely require test changes as well.

/// <summary>
/// Compiles Perlang source to executable form (typically ELF binaries on Linux, Mach-O on macOS and PE32/PE32+ on
/// Windows).
///
/// The compilation takes place in two steps:
///
/// - First, a machine-generated C++ file is generated.
/// - Then, the C++ file is piped to `clang` (version 14). `clang` generates an executable file.
///
/// Note that the compilation is highly experimental at the moment and has only been tested on Linux.
///
/// The generated C++ files follows the following design principles, in order of importance:
///
/// - They are valid C++
/// - They semantically correspond to the Perlang program they have been compiled from
/// - They are properly intended and formatted, as close to the "PL Style" for Perlang as possible.
///
/// Particularly the formatting is a "best-effort" at this stage. Machine-generated code in general is unlikely to be as
/// well-formatted as human-formatted code, unless passed through a semantically aware post-processor for formatting.
/// This is not the case for this simple implementation; it simply knows the level of indentation at each stage in the
/// processing and does its best to try to produce something which is "not garbage", but not necessarily perfectly
/// aesthetically pleasing.
/// <remarks>Note: must **NOT** be a <see cref="VisitorBase"/>, because we use different return types than the base class'
/// methods. Using the `new` keyword for these overloads will not work, since the base class will then not call the
/// methods in this class as expected.
///
/// Also, this class is not thread safe. A single instance of this class cannot safely be used from multiple
/// threads.</remarks>
/// </summary>
public class PerlangCompiler : Expr.IVisitor<object?>, Stmt.IVisitor<object>, IDisposable
{
    /// <summary>
    /// Gets a value indicating whether caching of compiled results should be disabled or not.
    ///
    /// To avoid unnecessary overhead, we cache the compiled output for a program by default. When working on changes to
    /// the actual Perlang codebase, this cache often needs to be disabled to test our changes. This can be achieved by
    /// setting PERLANG_EXPERIMENTAL_COMPILATION_CACHE_DISABLED=true in the environment. For convenience, a file
    /// named `~/.perlang_experimental_compilation_cache_disabled` can also be created to achieve this, in cases where
    /// setting the environment variable is impractical.
    /// </summary>
    private static bool CompilationCacheDisabled
    {
        get
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PERLANG_EXPERIMENTAL_COMPILATION_CACHE_DISABLED") ?? String.Empty;

            // The environment variable takes precedence, if set
            if (Boolean.TryParse(environmentVariable, out bool flag))
            {
                return flag;
            }

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return File.Exists(Path.Combine(homeDirectory, ".perlang_experimental_compilation_cache_disabled"));
        }
    }

    internal IBindingHandler BindingHandler { get; }

    private readonly Action<RuntimeError> runtimeErrorHandler;
    private readonly Action<Lang.String> standardOutputHandler;
    private readonly PerlangEnvironment globals = new();

    /// <summary>
    /// A collection of all currently defined global classes (both native/.NET and classes defined in Perlang code.)
    /// </summary>
    private readonly IDictionary<string, object> globalClasses = new Dictionary<string, object>();

    private readonly ImmutableDictionary<string, Type> nativeClasses;
    private readonly IDictionary<string, Method> methods;
    private readonly IDictionary<string, string> enums;
    private readonly IDictionary<string, string> classDefinitions;
    private readonly IDictionary<string, string> classImplementations;

    /// <summary>
    /// The location in the output we are currently writing to. Can be an enum, function, etc.
    /// </summary>
    private readonly NativeStringBuilder mainMethodContent;

    private int indentationLevel = 1;
    private Stmt.Class? currentClass = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerlangCompiler"/> class.
    /// </summary>
    /// <param name="runtimeErrorHandler">A callback that will be called on runtime errors. Note that after calling
    ///     this handler, the interpreter will abort the script.</param>
    /// <param name="standardOutputHandler">An callback that will receive output printed to standard output.</param>
    /// <param name="bindingHandler">A binding handler, or `null` to let the interpreter create a new instance.</param>
    public PerlangCompiler(
        Action<RuntimeError> runtimeErrorHandler,
        Action<Lang.String> standardOutputHandler,
        IBindingHandler? bindingHandler = null)
    {
        this.runtimeErrorHandler = runtimeErrorHandler;
        this.standardOutputHandler = standardOutputHandler;
        this.BindingHandler = bindingHandler ?? new BindingHandler();

        this.mainMethodContent = NativeStringBuilder.Create();

        this.methods = new Dictionary<string, Method>();
        this.enums = new Dictionary<string, string>();
        this.classDefinitions = new Dictionary<string, string>();
        this.classImplementations = new Dictionary<string, string>();

        methods["main"] = new Method("main", ImmutableList.Create<Parameter>(), "int", mainMethodContent);

        LoadStdlib();
        nativeClasses = RegisterGlobalFunctionsAndClasses();
    }

    public void Dispose()
    {
        mainMethodContent.Dispose();

        foreach (Method method in methods.Values) {
            method.MethodBody.Dispose();
        }
    }

    private static void LoadStdlib()
    {
        // Because of implicit dependencies, this is not loaded automatically; we must manually load this
        // assembly to ensure all Callables within it are registered in the global namespace.
        Assembly.Load("Perlang.StdLib");
    }

    private ImmutableDictionary<string, Type> RegisterGlobalFunctionsAndClasses()
    {
        RegisterGlobalClasses();

        // We need to make a copy of this at this early stage, when it _only_ contains native classes, so that
        // we can feed it to the Resolver class.
        return globalClasses.ToImmutableDictionary(kvp => kvp.Key, kvp => (Type)kvp.Value);
    }

    /// <summary>
    /// Registers global classes defined in native .NET code.
    /// </summary>
    /// <exception cref="PerlangInterpreterException">Multiple classes with the same name was encountered.</exception>
    private void RegisterGlobalClasses()
    {
        var globalClassesQueryable = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => new
            {
                Type = t,
                ClassAttribute = t.GetCustomAttribute<GlobalClassAttribute>()
            })
            .Where(t => t.ClassAttribute != null && (
                !t.ClassAttribute.Platforms.Any() || t.ClassAttribute.Platforms.Contains(Environment.OSVersion.Platform)
            ));

        foreach (var globalClass in globalClassesQueryable)
        {
            string name = globalClass.ClassAttribute!.Name ?? globalClass.Type.Name;

            if (globals.Get(name) != null)
            {
                throw new PerlangCompilerException(
                    $"Attempted to define global class '{name}', but another identifier with the same name already exists"
                );
            }

            globalClasses[name] = globalClass.Type;
        }
    }

    /// <summary>
    /// Compiles the given Perlang program to an executable, and runs the executable afterwards.
    /// </summary>
    /// <param name="source">The Perlang program to compile.</param>
    /// <param name="path">The path to the source file.</param>
    /// <param name="targetPath">The full path to the target file, or <c>null</c> to generate the target file name based
    /// on the source path.</param>
    /// <param name="compilerFlags">One or more <see cref="CompilerFlags"/> to use.</param>
    /// <param name="scanErrorHandler">A handler for scanner errors.</param>
    /// <param name="parseErrorHandler">A handler for parse errors.</param>
    /// <param name="nameResolutionErrorHandler">A handler for resolve errors.</param>
    /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
    /// <param name="immutabilityValidationErrorHandler">A handler for immutability validation errors.</param>
    /// <param name="compilerWarningHandler">A handler for compiler warnings.</param>
    /// <returns>The path to the compiled executable. Note that this will be non-null even on unsuccessful
    /// compilation.</returns>
    public string? CompileAndRun(
        string source,
        string path,
        string? targetPath,
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler)
    {
        string? executablePath = Compile(
            [
                new SourceFile(path, source)
            ],
            path,
            targetPath,
            compilerFlags,
            scanErrorHandler,
            parseErrorHandler,
            nameResolutionErrorHandler,
            typeValidationErrorHandler,
            immutabilityValidationErrorHandler,
            compilerWarningHandler
        );

        if (executablePath == null)
        {
            // These errors have already been propagated to the caller; we can simply return a this point.
            return null;
        }

        var processStartInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        string? valgrindLogFile = null;

        if (compilerFlags.HasFlag(CompilerFlags.RunWithValgrind))
        {
            valgrindLogFile = $"valgrind-{Path.ChangeExtension(Path.GetFileName(path), "")}log";

            processStartInfo.FileName = "valgrind";
            processStartInfo.ArgumentList.Add("--leak-check=full");
            processStartInfo.ArgumentList.Add("--error-exitcode=1");
            processStartInfo.ArgumentList.Add("--track-origins=yes");
            processStartInfo.ArgumentList.Add("--show-leak-kinds=all");
            processStartInfo.ArgumentList.Add("--show-error-list=yes");
            processStartInfo.ArgumentList.Add("--errors-for-leak-kinds=all");
            processStartInfo.ArgumentList.Add($"--log-file={valgrindLogFile}");
            processStartInfo.ArgumentList.Add(executablePath);
        }
        else
        {
            processStartInfo.FileName = executablePath;
        }

        Process? process = Process.Start(processStartInfo);

        if (process == null)
        {
            runtimeErrorHandler(new RuntimeError(null, $"Launching process for {path} failed"));
            return executablePath;
        }

        process.WaitForExit();

        // Note that output is currently not streamed to the caller while the process is running. All stdout and stderr
        // output is read after the process has exited.
        while (process.StandardError.ReadLine() is { } standardOutputLine)
        {
            standardOutputHandler(Lang.String.from(standardOutputLine));
        }

        while (process.StandardOutput.ReadLine() is { } standardErrorLine)
        {
            standardOutputHandler(Lang.String.from(standardErrorLine));
        }

        if (process.ExitCode != 0)
        {
            if (valgrindLogFile == null)
            {
                runtimeErrorHandler(new RuntimeError(null, $"Process {executablePath} exited with exit code {process.ExitCode}"));
            }
            else
            {
                runtimeErrorHandler(new RuntimeError(null, $"Process {executablePath} exited with exit code {process.ExitCode}. Valgrind output:\n\n{File.ReadAllText(valgrindLogFile)}"));
            }
        }

        return executablePath;
    }

    /// <summary>
    /// Compiles and assembles the given Perlang program to an .o file (ELF object file).
    /// </summary>
    /// <param name="source">The Perlang program to compile.</param>
    /// <param name="path">The path to the source file.</param>
    /// <param name="targetPath">The full path to the target file, or <c>null</c> to generate the target file name based
    /// on the source path.</param>
    /// <param name="compilerFlags">One or more <see cref="CompilerFlags"/> to use.</param>
    /// <param name="scanErrorHandler">A handler for scanner errors.</param>
    /// <param name="parseErrorHandler">A handler for parse errors.</param>
    /// <param name="nameResolutionErrorHandler">A handler for resolve errors.</param>
    /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
    /// <param name="immutabilityValidationErrorHandler">A handler for immutability validation errors.</param>
    /// <param name="compilerWarningHandler">A handler for compiler warnings.</param>
    public void CompileAndAssemble(
        string source,
        string path,
        string? targetPath,
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler)
    {
        Compile(
            [
                new SourceFile(path, source)
            ],
            path,
            targetPath,
            compilerFlags,
            scanErrorHandler,
            parseErrorHandler,
            nameResolutionErrorHandler,
            typeValidationErrorHandler,
            immutabilityValidationErrorHandler,
            compilerWarningHandler,
            compileAndAssembleOnly: true
        );
    }

    /// <summary>
    /// Compiles the given Perlang program to an executable.
    /// </summary>
    /// <param name="sourceFiles">A list of one or more Perlang source files to compile.</param>
    /// <param name="path">The path and file name of the source file.</param>
    /// <param name="targetPath">The full path to the target file, or <c>null</c> to generate the target file name based
    /// on the source file name.</param>
    /// <param name="compilerFlags">One or more <see cref="CompilerFlags"/> to use.</param>
    /// <param name="scanErrorHandler">A handler for scanner errors.</param>
    /// <param name="parseErrorHandler">A handler for parse errors.</param>
    /// <param name="nameResolutionErrorHandler">A handler for resolve errors.</param>
    /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
    /// <param name="immutabilityValidationErrorHandler">A handler for immutability validation errors.</param>
    /// <param name="compilerWarningHandler">A handler for compiler warnings.</param>
    /// <param name="compileAndAssembleOnly">A flag which enables functionality similar to `gcc -c`; compile and assemble
    /// the given program, but do not generate an executable (and inherently do not execute it).</param>
    /// <returns>The path to the generated executable file, or `null` if compilation failed.</returns>
    private string? Compile(
        ImmutableList<SourceFile> sourceFiles,
        string path,
        string? targetPath,
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler,
        bool compileAndAssembleOnly = false)
    {
        string targetCppFile = Path.ChangeExtension(targetPath ?? path, ".cc");
        string targetHeaderFile = Path.ChangeExtension(targetPath ?? path, ".h");

#if _WINDOWS
        // clang is very unlikely to have been available on Windows anyway, but why not...
        string targetExecutable = targetPath ?? Path.ChangeExtension(targetCppFile, compileAndAssembleOnly ? "obj" : "exe");
#else
        string targetExecutable = targetPath ?? Path.ChangeExtension(targetCppFile, compileAndAssembleOnly ? "o" : null);
#endif

        // TODO: Check the creation time of *all* dependencies here, including the stdlib (both .so/.dll and .h files
        // TODO: ideally). Right now, rebuilding the stdlib doesn't trigger a cache invalidation which is annoying
        // TODO: when developing the stdlib.
        if (!(compilerFlags.HasFlag(CompilerFlags.CacheDisabled) || CompilationCacheDisabled) &&
            File.GetCreationTime(targetCppFile) > File.GetCreationTime(path) &&
            File.GetCreationTime(targetHeaderFile) > File.GetCreationTime(path) &&
            File.GetCreationTime(targetExecutable) > File.GetCreationTime(path))
        {
            // Both the .cc/.h files and the executable are newer than the given Perlang program => no need to
            // compile it. We presume the binary to be already up-to-date.
            return targetExecutable;
        }

        ScanAndParseResult result = PerlangParser.ScanAndParse(
            sourceFiles,
            scanErrorHandler,
            parseErrorHandler,
            replMode: false
        );

        if (result == ScanAndParseResult.ScanErrorOccurred ||
            result == ScanAndParseResult.ParseErrorEncountered)
        {
            // These errors have already been propagated to the caller; we can simply return a this point.
            return null;
        }

        ImmutableList<Stmt> statements;
        ImmutableList<Token> cppPrototypes;
        ImmutableList<Token> cppMethods;

        if (result.HasStatements)
        {
            statements = result.Statements!.ToImmutableList();
            cppPrototypes = result.CppPrototypes.ToImmutableList();
            cppMethods = result.CppMethods.ToImmutableList();
        }
        else if (result.HasExpr)
        {
            // In interpreted mode, we handle "single expressions" specially (so we can return their value to the
            // caller). In compiled mode, single expressions typically don't make _sense_ but they are used by some unit
            // test(s). We wrap them as statements to make the compiler be able to deal with them.
            statements = ImmutableList.Create<Stmt>(new Stmt.ExpressionStmt(result.Expr!));
            cppPrototypes = ImmutableList<Token>.Empty;
            cppMethods = ImmutableList<Token>.Empty;
        }
        else
        {
            throw new IllegalStateException("syntax was neither Expr nor list of Stmt");
        }

        //
        // Resolving names phase
        //

        bool hasNameResolutionErrors = false;

        var nameResolver = new NameResolver(
            nativeClasses,
            BindingHandler,
            AddGlobalClass,
            nameResolutionError =>
            {
                hasNameResolutionErrors = true;
                nameResolutionErrorHandler(nameResolutionError);
            }
        );

        nameResolver.Resolve(statements);

        if (hasNameResolutionErrors)
        {
            // Resolution errors has been reported back to the provided error handler. Nothing more remains
            // to be done than aborting the evaluation.
            return null;
        }

        // The name resolver must run twice, to resolve forward references. The alternative would have been something like
        // C++-style header files, which are incredibly obnoxious and something we want to avoid like the plague.
        nameResolver.StartSecondPass();
        nameResolver.Resolve(statements);

        //
        // Type validation
        //

        bool typeValidationFailed = false;

        TypeValidator.Validate(
            statements,
            typeValidationError =>
            {
                typeValidationFailed = true;
                typeValidationErrorHandler(typeValidationError);
            },
            BindingHandler,
            compilerWarning =>
            {
                bool result = compilerWarningHandler(compilerWarning);

                if (result)
                {
                    typeValidationFailed = true;
                }
            }
        );

        if (typeValidationFailed)
        {
            return null;
        }

        //
        // Immutability validation
        //

        bool immutabilityValidationFailed = false;

        ImmutabilityValidator.Validate(
            statements,
            immutabilityValidationError =>
            {
                immutabilityValidationFailed = true;
                immutabilityValidationErrorHandler(immutabilityValidationError);
            },
            BindingHandler.GetVariableOrFunctionBinding
        );

        if (immutabilityValidationFailed)
        {
            return null;
        }

        //
        // "Code analysis" validation
        //

        bool codeAnalysisValidationFailed = false;

        CodeAnalysisValidator.Validate(
            statements,
            compilerWarning =>
            {
                bool result = compilerWarningHandler(compilerWarning);

                if (result)
                {
                    codeAnalysisValidationFailed = true;
                }
            }
        );

        if (codeAnalysisValidationFailed)
        {
            return null;
        }

        try
        {
            if ((compileAndAssembleOnly || compilerFlags.HasFlag(CompilerFlags.RemoveEmptyMainMethod)) && methods["main"].MethodBody.Length == 0)
            {
                // We generate a default "main" method elsewhere. In -c mode, we want to remove this method if it's empty,
                // to avoid conflicts with the "real" main method of the program the .o file is being linked into.
                methods.Remove("main");
            }

            // The AST traverser returns its output. We put it in the main method content, which is written along with
            // all other methods in the code further below.
            mainMethodContent.Append(Compile(statements));

            using (StreamWriter streamWriter = File.CreateText(targetHeaderFile)) {
                string headerLine;

                if (compilerFlags.HasFlag(CompilerFlags.Idempotent)) {
                    headerLine = "Automatically generated code by Perlang";
                }
                else {
                    headerLine = $"Automatically generated code by Perlang {CommonConstants.InformationalVersion} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}";
                }

                // Write standard file header, which includes everything our transpiled code might expect.
                streamWriter.Write($"""
// {headerLine}
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#pragma once

#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"


""");

                if (cppPrototypes.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// C++ prototypes");
                    streamWriter.WriteLine("//");

                    foreach (Token prototype in cppPrototypes)
                    {
                        streamWriter.WriteLine(prototype.Literal);
                    }

                    streamWriter.WriteLine();
                }

                if (classDefinitions.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Class definitions");
                    streamWriter.WriteLine("//");

                    foreach ((string _, string definition) in classDefinitions) {
                        streamWriter.WriteLine(definition);
                    }
                }

                if (enums.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Enum definitions");
                    streamWriter.WriteLine("//");

                    foreach ((string _, string definition) in enums) {
                        streamWriter.WriteLine(definition);
                    }
                }

                if (methods.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Method definitions");
                    streamWriter.WriteLine("//");

                    foreach (var (key, value) in methods) {
                        streamWriter.WriteLine($"{value.ReturnType} {key}({value.ParametersString});");
                    }
                }
            }

            using (StreamWriter streamWriter = File.CreateText(targetCppFile)) {
                string headerLine;

                if (compilerFlags.HasFlag(CompilerFlags.Idempotent)) {
                    headerLine = "Automatically generated code by Perlang";
                }
                else {
                    headerLine = $"Automatically generated code by Perlang {CommonConstants.InformationalVersion} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}";
                }

                // Write standard file header, which includes everything our transpiled code might expect.
                streamWriter.Write($"""
// {headerLine}
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"

#include "{Path.GetFileName(targetHeaderFile)}"


""");

                if (cppMethods.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// C++ methods");
                    streamWriter.WriteLine("//");

                    foreach (Token method in cppMethods)
                    {
                        streamWriter.WriteLine(method.Literal);
                    }

                    // Try to avoid an extra newline at the end of the file
                    if (methods.Count > 0) {
                        streamWriter.WriteLine();
                    }
                }

                if (methods.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Method declarations");
                    streamWriter.WriteLine("//");

                    foreach (var (key, value) in methods) {
                        streamWriter.WriteLine($"{value.ReturnType} {key}({value.ParametersString}) {{");
                        streamWriter.Write(value.MethodBody);
                        streamWriter.WriteLine('}');
                        streamWriter.WriteLine();
                    }
                }

                if (classImplementations.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Class implementations");
                    streamWriter.WriteLine("//");

                    foreach (var (_, classImplementation) in classImplementations) {
                        streamWriter.Write(classImplementation);
                        streamWriter.WriteLine();
                    }
                }
            }

            // We attempt to detect the stdlib location by looking in the path to the `perlang` executable first. If it
            // doesn't exist there, the PERLANG_ROOT environment variable is checked next.
            string? executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string? stdlibPath;

            string perlangExecutableStdlibPath = Path.Join(executablePath, "lib", "stdlib");

            if (Directory.Exists(perlangExecutableStdlibPath))
            {
                // This seems to be a Perlang distribution, either a nightly snapshot or a release version. Use the
                // stdlib provided with it.
                stdlibPath = perlangExecutableStdlibPath;
            }
            else
            {
                string? perlangRoot = Environment.GetEnvironmentVariable("PERLANG_ROOT");

                if (perlangRoot == null)
                {
                    throw new PerlangCompilerException(
                        $"The {perlangExecutableStdlibPath} directory does not exist, and PERLANG_ROOT is not set. One " +
                        "of these conditions must be met for experimental compilation to succeed. If setting " +
                        "PERLANG_ROOT, it should point to a directory where the lib/stdlib directory contains a " +
                        "compiled version of the Perlang standard library."
                    );
                }

                stdlibPath = Path.Join(perlangRoot, "lib", "stdlib");
            }

            var processStartInfo = new ProcessStartInfo
            {
                // Make this explicit, since we have only tested this with a very specific clang version. Anything else is
                // completely untested and not expected to work at the moment.
                FileName = "clang++-14",

                ArgumentList =
                {
                    // Required for nested namespaces
                    "--std=c++17",

                    "-I", Path.Combine(stdlibPath, "include"),
                    compileAndAssembleOnly ? "-c" : "",
                    "-o", targetExecutable,

                    // Useful while debugging
                    //"-save-temps",
                    //"-v",

                    // Regarding warnings: we enable everything and also the "strict" -Werror mode, but then also
                    // selectively disable warnings which we know will otherwise be triggered by our code generator.
                    "-Wall",
                    "-Werror",

                    // Enable warnings on e.g. narrowing conversion from `long long` to `unsigned long long`.
                    "-Wconversion",

                    // This could be added, but the problem is that division by zero has undefined behavior in C++. :/ I
                    // think the long-term goal for Perlang in this regard is to use proper hardware exceptions for this
                    // (which is natively supported on x86 and amd64, at least), and propagate it as a Perlang exception.
                    // For platforms without hardware support, we might have to emulate this in software for a coherent
                    // dev experience. For now, let Clang warn about it when dividing with a zero constant.
                    //"-Wno-division-by-zero",

                    // ...but do not warn on implicit conversion from `int` to `float` or `double`. For now, we are
                    // aiming at mimicking the C# semantics in this.
                    "-Wno-implicit-int-float-conversion",

                    // Certain narrowing conversions are problematic; we have seen this causing issues when implementing
                    // the BigInt support. For example, `uint64_t` must not be implicitly converted to call a `long
                    // long` constructor/method.
                    "-Wimplicit-int-conversion",

                    // C# allows cast from e.g. 2147483647 to float without warnings, but here's an interesting thing:
                    // clang emits a very nice warning for this ("implicit conversion from 'int' to 'float' changes
                    // value from 2147483647 to 2147483648"). We might want to consider doing something similar for
                    // Perlang in the end.
                    "-Wno-implicit-const-int-float-conversion",

                    // 1073741824 * 2 causes a warning about this. As with other overflows, we could consider
                    // implementing warnings for them, but we want them on the Perlang level instead of on clang level
                    // in that case.
                    "-Wno-integer-overflow",

                    // Handled on the Perlang side
                    "-Wno-logical-op-parentheses",

                    // Probably enabled by -Wconversion, but this causes issues with valid code like `2147483647 /
                    // 4294967295U`.
                    //
                    // NOTE: disabling this also enables extremely dangerous constructs like being able to call
                    // BigInt::pow(-3), causing the negative integer to be reinterpreted as a very large uint32 instead.
                    // We should re-think this and really try to re-enable this, to prevent such horrible code from
                    // compiling.
                    "-Wno-sign-conversion",

                    // We currently overflow without warnings, but we could consider implementing something like this
                    // warning in Perlang as well. Here's what the warning produces on `9223372036854775807 << 2`:
                    // error: signed shift result (0x1FFFFFFFFFFFFFFFC) requires 66 bits to represent, but 'long' only has 64 bits
                    "-Wno-shift-overflow",

                    // 2147483647 >> 32 is valid in e.g. Java, but generates a warning with clang. The warning is useful,
                    // but we silence it for now.
                    "-Wno-shift-count-overflow",

                    // Doesn't make sense, but some of our unit tests produce such warnings
                    "-Wno-unused-value",
                    "-Wno-unused-variable",

                    // Order here is critical: we must list the C++ source *before* the libstdlib.a reference, since the
                    // linker will otherwise be unable to resolve references from the C++ file to e.g. perlang::*
                    // methods.
                    targetCppFile,

                    // TODO: Support Windows static libraries as well
                    compileAndAssembleOnly ? "" : Path.Combine(stdlibPath, "lib/libstdlib.a"),

                    // Needed by the Perlang stdlib
                    compileAndAssembleOnly ? "" : "-lm"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process? process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new PerlangCompilerException($"Launching process for {path} failed");
                }

                // To avoid deadlocks, always read the output stream first and then wait.
                string stderrOutput = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new PerlangCompilerException($"Internal compiler error: compiling transpiled source {targetCppFile} failed. Detailed error will follow:{Environment.NewLine}" +
                                                       $"{Environment.NewLine}" +
                                                       $"{stderrOutput}");
                }

                return targetExecutable;
            }
        }
        catch (SystemException e)
        {
            throw new ApplicationException(
                "Failed running clang. Experimental compilation is only supported on Linux-based systems. If running a " +
                "Debian/Ubuntu-based distribution, please ensure that the clang-14 package is installed since experimental " +
                "compilation depends on it.",
                e
            );
        }
        catch (RuntimeError e)
        {
            runtimeErrorHandler(e);
            return null;
        }
    }

    /// <summary>
    /// Entry-point for compiling one or more statements.
    /// </summary>
    /// <param name="statements">An enumerator for a collection of statements.</param>
    /// <returns>The C++ source for the given statements.</returns>
    private string Compile(IEnumerable<Stmt> statements)
    {
        using var result = NativeStringBuilder.Create();

        foreach (Stmt statement in statements)
        {
            result.Append(Compile(statement));
        }

        return result.ToString();
    }

    private string Compile(Stmt stmt)
    {
        return (string)stmt.Accept(this);
    }

    private void AddGlobalClass(string name, IPerlangClass perlangClass)
    {
        globalClasses[name] = perlangClass;
    }

    public object VisitEmptyExpr(Expr.Empty expr)
    {
        // No need to emit anything in the generated C++ code for an empty expression.
        return VoidObject.Void;
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        if (expr.Target is Expr.Identifier identifier)
        {
            return $"{identifier.Name.Lexeme} = {expr.Value.Accept(this)}";
        }
        else if (expr.Target is Expr.Get get)
        {
            if (get.Object is Expr.Identifier objectIdentifier)
            {
                // TODO: Should probably not use "->" unconditionally here, since it won't work if/when we introduce
                // TODO: stack-allocated (local) objects.
                return $"{objectIdentifier.Name.Lexeme}->{get.Name.Lexeme} = {expr.Value.Accept(this)}";
            }
            else
            {
                throw new PerlangCompilerException($"Invalid assignment target: {expr.Target}");
            }
        }
        else {
            throw new PerlangCompilerException($"Invalid assignment target: {expr.Target}");
        }
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        using var result = NativeStringBuilder.Create();
        string? leftCast = null;
        string? rightCast = null;

        switch (expr.Operator.Type)
        {
            //
            // Comparison operators
            //
            // IComparable would be useful to reduce code duplication here, but it has one major problem: it only
            // supports same-type comparisons (int+int, long+long etc). We do not want to limit our code like that.
            //

            // Note: do **NOT** add new operators here without adding corresponding tests. (in the future, try to
            // enforce this via e.g. ArchUnit.NET or a unit test)

            case GREATER:
                // TODO: We might need something like CheckNumberOperands() in PerlangInterpreter here, but we can
                // TODO: obviously not do any value-based checking since we are a compiler and not an interpreter
                result.Append($"{expr.Left.Accept(this)} > {expr.Right.Accept(this)}");
                break;

            case GREATER_EQUAL:
                result.Append($"{expr.Left.Accept(this)} >= {expr.Right.Accept(this)}");
                break;

            case LESS:
                result.Append($"{expr.Left.Accept(this)} < {expr.Right.Accept(this)}");
                break;

            case LESS_EQUAL:
                result.Append($"{expr.Left.Accept(this)} <= {expr.Right.Accept(this)}");
                break;

            case BANG_EQUAL:
                if (expr.Left.TypeReference.IsStringType && expr.Left.TypeReference.CppWrapInSharedPtr &&
                    expr.Right.TypeReference.IsStringType && expr.Right.TypeReference.CppWrapInSharedPtr)
                {
                    // Example generated code: *s1 != *s2
                    result.Append($"*{expr.Left.Accept(this)} != *{expr.Right.Accept(this)}");
                }
                else
                {
                    result.Append($"{expr.Left.Accept(this)} != {expr.Right.Accept(this)}");
                }

                break;

            case EQUAL_EQUAL:
                if (expr.Left.TypeReference.IsStringType && expr.Left.TypeReference.CppWrapInSharedPtr &&
                    expr.Right.TypeReference.IsStringType && expr.Right.TypeReference.CppWrapInSharedPtr)
                {
                    // Example generated code: *s1 == *s2
                    result.Append($"*{expr.Left.Accept(this)} == *{expr.Right.Accept(this)}");
                }
                else
                {
                    result.Append($"{expr.Left.Accept(this)} == {expr.Right.Accept(this)}");
                }

                break;

            //
            // Arithmetic operators
            //
            // Note: many of these checks could be done at an earlier stage, to ensure e.g. valid numeric types for the
            // operators which require it. We don't currently have any such checks in place, though.
            //

            case MINUS:
                // If the target of the expression is different than the left- or right-hand operand, we need to cast it
                // to ensure we get the expected operator on the C++ side (example: `int - uint` should produce a
                // `long`, and at least one of the operands need to be widened to ensure expected semantics for an
                // expression like `2 - 4294967295`)
                if (expr.TypeReference.ClrType != expr.Left.TypeReference.ClrType)
                {
                    leftCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType)
                {
                    rightCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{leftCast}{expr.Left.Accept(this)} - {rightCast}{expr.Right.Accept(this)}");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case MINUS_EQUAL:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{expr.Left.Accept(this)} -= {expr.Right.Accept(this)}");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case PLUS:
                // If the target of the expression is different than the left- or right-hand operand, we need to cast it
                // to ensure we get the expected operator on the C++ side (example: `int + uint` should produce a
                // `long`, and at least one of the operands need to be widened to ensure expected semantics for an
                // expression like `2 + 4294967295`)
                //
                // String types are special-cased here, since they are wrapped in std::shared_ptr and cannot be cast to
                // e.g perlang::String directly (because this will make the C++ compiler attempt to perform a copy to an
                // abstract, non-instantiable type).
                if (expr.TypeReference.ClrType != expr.Left.TypeReference.ClrType && !(
                        expr.TypeReference.ClrType == typeof(Lang.String) ||
                        expr.TypeReference.ClrType == typeof(Lang.AsciiString) ||
                        expr.TypeReference.ClrType == typeof(Lang.Utf8String)))
                {
                    leftCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType && !(
                        expr.TypeReference.ClrType == typeof(Lang.String) ||
                        expr.TypeReference.ClrType == typeof(Lang.AsciiString) ||
                        expr.TypeReference.ClrType == typeof(Lang.Utf8String)))
                {
                    rightCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{leftCast}{expr.Left.Accept(this)} + {rightCast}{expr.Right.Accept(this)}");
                }
                else if (expr.Left.TypeReference.IsStringType && expr.Right.TypeReference.IsStringType)
                {
                    // The dereference operator (*) must be used to dereference the std::shared_ptr instances
                    // Parentheses required to workaround "indirection requires pointer operand" errors in the C++ compilation
                    result.Append($"(*{leftCast}{expr.Left.Accept(this)} + *{rightCast}{expr.Right.Accept(this)})");
                }
                else if (expr.Left.TypeReference.IsStringType && expr.Right.TypeReference.IsValidNumberType)
                {
                    // The dereference operator (*) must be used to dereference the std::shared_ptr instance
                    result.Append($"(*{leftCast}{expr.Left.Accept(this)} + {rightCast}{expr.Right.Accept(this)})");
                }
                else if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsStringType)
                {
                    // The dereference operator (*) must be used to dereference the std::shared_ptr instance
                    result.Append($"({leftCast}{expr.Left.Accept(this)} + *{rightCast}{expr.Right.Accept(this)})");
                }
                else
                {
                    // TODO: Strings/other types need different handling, and are more complex since they will
                    // TODO: inherently require memory allocation. We don't have a defined model for when that
                    // TODO: memory would be deallocated, #378
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case PLUS_EQUAL:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{expr.Left.Accept(this)} += {expr.Right.Accept(this)}");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case SLASH:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{expr.Left.Accept(this)} / {expr.Right.Accept(this)}");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case STAR:
                // If the target of the expression is different than the left- or right-hand operand, we need to cast it
                // to ensure we get the expected operator on the C++ side (example: `int * uint` should produce a
                // `long`, and at least one of the operands need to be widened to ensure expected semantics for an
                // expression like `2 * 4294967295`)
                if (expr.TypeReference.ClrType != expr.Left.TypeReference.ClrType)
                {
                    leftCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType)
                {
                    rightCast = expr.TypeReference.CppTypeCast();
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    result.Append($"{leftCast}{expr.Left.Accept(this)} * {rightCast}{expr.Right.Accept(this)}");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case STAR_STAR:
                if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(BigInteger) }.Contains(expr.Left.TypeReference.ClrType) &&
                    expr.Right.TypeReference.ClrType == typeof(int))
                {
                    result.Append($"{leftCast}perlang::BigInt_pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                }
                else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(expr.Left.TypeReference.ClrType) &&
                         new[] { typeof(float), typeof(double) }.Contains(expr.Right.TypeReference.ClrType))
                {
                    // Normal math.h-based pow(), returning a double
                    result.Append($"{leftCast}pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                }
                else if (new[] { typeof(float), typeof(double) }.Contains(expr.Left.TypeReference.ClrType) &&
                         new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(expr.Right.TypeReference.ClrType))
                {
                    // Normal math.h-based pow(), returning a double
                    result.Append($"{leftCast}pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case PERCENT:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    if (expr.Left.TypeReference.ClrType == typeof(BigInteger) ||
                        expr.Right.TypeReference.ClrType == typeof(BigInteger))
                    {
                        result.Append($"{leftCast}perlang::BigInt_mod({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(double) ||
                             expr.Right.TypeReference.ClrType == typeof(double))
                    {
                        // C and C++ does not support the % operator for double; we must use `fmod()` instead
                        result.Append($"fmod({expr.Left.Accept(this)}, {expr.Right.Accept(this)})");
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(float) ||
                             expr.Right.TypeReference.ClrType == typeof(float))
                    {
                        // Likewise, but with `float` instead of `double` as arguments and return type
                        result.Append($"fmodf({expr.Left.Accept(this)}, {expr.Right.Accept(this)})");
                    }
                    else
                    {
                        result.Append($"{expr.Left.Accept(this)} % {expr.Right.Accept(this)}");
                    }
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case LESS_LESS:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    // Additional parentheses emitted since C and C++ have the "wrong" (from a Perlang and K&R POV)
                    // precedence for the shift left operator, which clang is kind enough to warn us about.
                    result.Append($"({expr.Left.Accept(this)} << {expr.Right.Accept(this)})");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            case GREATER_GREATER:
                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    // The bitmask rules here correspond to those of C#, as defined here:
                    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators#shift-count-of-the-shift-operators
                    string shiftCountBitMask;

                    if (expr.Left.TypeReference.ClrType == typeof(int) ||
                        expr.Left.TypeReference.ClrType == typeof(uint))
                    {
                        shiftCountBitMask = "& 0b11111";
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(long) ||
                             expr.Left.TypeReference.ClrType == typeof(ulong))
                    {
                        shiftCountBitMask = "& 0b111111";
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(BigInteger))
                    {
                        // BigInt == don't mask away anything of the result
                        shiftCountBitMask = "";
                    }
                    else
                    {
                        string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                        throw new RuntimeError(expr.Operator, message);
                    }

                    // Additional parentheses emitted since C and C++ have the "wrong" (from a Perlang and K&R POV)
                    // precedence for the shift left operator, which clang is kind enough to warn us about.
                    result.Append($"({expr.Left.Accept(this)} >> ({expr.Right.Accept(this)} {shiftCountBitMask}))");
                }
                else
                {
                    string message = CompilerMessages.UnsupportedOperandsInBinaryExpression(expr.Operator.Type, expr.Left.TypeReference, expr.Right.TypeReference);
                    throw new RuntimeError(expr.Operator, message);
                }

                break;

            default:
            {
                // Other operator types are not supported for binary expressions
                string message = CompilerMessages.UnsupportedOperatorTypeInBinaryExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
            }
        }

        return result.ToString();
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        // TODO: In interpreted mode, information about the callee is not really known at this point; we get information
        // TODO: about it by calling Evaluate(expr.Callee). In compiled mode, it would probably make sense to do method
        // TODO: binding at an earlier stage and e.g. validate the number and type of arguments with the method
        // TODO: definition. Right now, we'll leave this to the C++ compiler to deal with...

        using var result = NativeStringBuilder.Create();

        result.Append(expr.Callee.Accept(this));
        result.Append('(');

        for (int index = 0; index < expr.Arguments.Count; index++)
        {
            Expr argument = expr.Arguments[index];
            result.Append(argument.Accept(this));

            if (index < expr.Arguments.Count - 1)
            {
                result.Append(", ");
            }
        }

        result.Append(')');

        return result.ToString();
    }

    public object VisitIndexExpr(Expr.Index expr)
    {
        // TODO: Consider doing range checking at this point. In general, try to figure out a balanced approach in the
        // TODO: "safe" vs "fast" dichotomy. C and C++ are fast (no array bounds checking) but can blow up in your face
        // TODO: if you're not careful. Java and .NET are safe (all array indexing is bounds-checked) but this
        // TODO: inherently *will* have a performance impact. Sometimes this is negligible but sometimes not.
        // TODO:
        // TODO: One way to deal with this: make `foo[bar]` be safe and add an "unsafe" indexing operator `foo[[bar]]`
        // TODO: which can be used when the user believes he knows best. Investigate how Rust is doing this.

        using var result = NativeStringBuilder.Create();

        ITypeReference indexeeTypeReference = expr.Indexee.TypeReference;

        if (indexeeTypeReference.CppWrapInSharedPtr)
        {
            result.Append("(*");
        }

        result.Append(expr.Indexee.Accept(this));

        if (indexeeTypeReference.CppWrapInSharedPtr)
        {
            result.Append(')');
        }

        result.Append('[');
        result.Append(expr.Argument.Accept(this));
        result.Append(']');

        return result.ToString();
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        using var result = NativeStringBuilder.Create();

        result.Append('(');
        result.Append(expr.Expression.Accept(this));
        result.Append(')');

        return result.ToString();
    }

    public object VisitCollectionInitializerExpr(Expr.CollectionInitializer collectionInitializer)
    {
        using var result = NativeStringBuilder.Create();

        result.Append($"std::make_shared<{collectionInitializer.TypeReference.CppType!.TypeName}>(");

        switch (collectionInitializer.TypeReference.CppType?.TypeName) {
            // Collection initializers must be prepended with a type specifier, since the C++ compiler will not attempt
            // to guess the correct type for us.
            case "perlang::IntArray":
                result.Append("std::initializer_list<int32_t> ");
                break;
            case "perlang::StringArray":
                result.Append("std::initializer_list<std::shared_ptr<const perlang::String>> ");
                break;
            default:
                throw new PerlangCompilerException($"Type {collectionInitializer.TypeReference.CppType!.TypeName} is not supported for collection initializers");
        }

        result.Append("{ ");

        for (int index = 0; index < collectionInitializer.Elements.Count; index++) {
            Expr element = collectionInitializer.Elements[index];
            result.Append(element.Accept(this));

            if (index < collectionInitializer.Elements.Count - 1) {
                result.Append(", ");
            }
        }

        result.Append(" }");
        result.Append(')');

        return result.ToString();
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        using var result = NativeStringBuilder.Create();

        if (expr.Value is AsciiString)
        {
            // The value is an ASCII string literal, so it is safe to use this factory method to wrap it in an AsciiString
            // on the C++ side. (The "static string" is the important part here; because we know that it's a literal, we
            // can assume "shared" ownership of it and assume that it will never be deallocated for the whole lifetime of
            // the program.)
            result.Append("perlang::ASCIIString::from_static_string(\"");
            result.Append(expr);
            result.Append("\")");
        }
        else if (expr.Value is Utf8String)
        {
            // The value is an UTF-8 string literal, so it is safe to use this factory method to wrap it in an UTF8String
            // on the C++ side. (The "static string" is the important part here; because we know that it's a literal, we
            // can assume "shared" ownership of it and assume that it will never be deallocated for the whole lifetime of
            // the program.)
            result.Append("perlang::UTF8String::from_static_string(\"");
            result.Append(expr);
            result.Append("\")");
        }
        else if (expr.Value == null)
        {
            // C++11 defines nullptr as a keyword
            result.Append("nullptr");
        }
        else if (expr.Value is IntegerLiteral<uint> uintLiteral)
        {
            // TODO: should be UL when targeting 32-bit platforms and U for 64-bit.
            result.Append(uintLiteral.Value);
            result.Append('U'); // unsigned
        }
        else if (expr.Value is IntegerLiteral<ulong> ulongLiteral)
        {
            // FIXME: should be ULL for 32-bit platforms
            result.Append(ulongLiteral.Value);
            result.Append("UL"); // unsigned long
        }
        else if (expr.Value is IFloatingPointLiteral floatingPointLiteral)
        {
            // We want to be careful to avoid using Value for floating point literals, since we want to preserve the exact
            // value that the user entered as-is, without any potential loss of precision
            result.Append(floatingPointLiteral.NumberCharacters);

            if (floatingPointLiteral is FloatingPointLiteral<double>)
            {
                // double is the default in C++, so no need for any suffix in this case
            }
            else if (floatingPointLiteral is FloatingPointLiteral<float>)
            {
                result.Append('f');
            }
            else
            {
                throw new PerlangCompilerException($"Internal compiler error: unsupported floating point literal type {expr.Value.GetType().ToTypeKeyword()} encountered");
            }
        }
        else if (expr.Value is IntegerLiteral<BigInteger> bigintLiteral)
        {
            result.Append("BigInt(\"");
            result.Append(bigintLiteral.Value.ToString());
            result.Append("\")");
        }
        else if (expr.Value is INumericLiteral numericLiteral)
        {
            // This will work for all other numeric types we support
            result.Append(numericLiteral.Value);
        }
        else if (expr.Value is bool boolLiteral)
        {
            // true.ToString() == "True", which is why we need this manual logic.
            result.Append(boolLiteral ? "true" : "false");
        }
        else
        {
            throw new PerlangCompilerException($"Internal compiler error: unsupported type {expr.Value.GetType().ToTypeKeyword()} encountered");
        }

        return result.ToString();
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        using var result = NativeStringBuilder.Create();

        switch (expr.Operator.Type)
        {
            case PIPE_PIPE:
                result.Append(expr.Left.Accept(this));
                result.Append("||");
                result.Append(expr.Right.Accept(this));
                break;

            case AMPERSAND_AMPERSAND:
                result.Append(expr.Left.Accept(this));
                result.Append("&&");
                result.Append(expr.Right.Accept(this));
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInLogicalExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return result.ToString();
    }

    public object VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
    {
        using var result = NativeStringBuilder.Create();

        switch (expr.Operator.Type)
        {
            case BANG:
                result.Append('!');
                result.Append(expr.Right.Accept(this));
                break;

            case MINUS:
                result.Append('-');
                result.Append(expr.Right.Accept(this));
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInUnaryPrefixExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return result.ToString();
    }

    public object VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
    {
        using var result = NativeStringBuilder.Create();

        switch (expr.Operator.Type)
        {
            case PLUS_PLUS:
                result.Append(expr.Left.Accept(this));
                result.Append("++");
                break;

            case MINUS_MINUS:
                result.Append(expr.Left.Accept(this));
                result.Append("--");
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInUnaryPostfixExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return result.ToString();
    }

    public object VisitIdentifierExpr(Expr.Identifier expr) =>
        expr.Name.Lexeme;

    public object VisitGetExpr(Expr.Get expr)
    {
        using var result = NativeStringBuilder.Create();

        if (expr.Object is Expr.Identifier identifier)
        {
            if (globalClasses.ContainsKey(identifier.Name.Lexeme))
            {
                // These classes are put in the `perlang::stdlib` namespace in the C++ world
                result.Append($"perlang::stdlib::{identifier.Name.Lexeme}");

                // Static methods are called with `class::method(params)` syntax in C++
                result.Append("::");
                result.Append(expr.Name.Lexeme);
            }
            else if (identifier.TypeReference.IsArray)
            {
                // Arrays support a `length` property in Perlang, which is similar to other languages.
                if (expr.Name.Lexeme == "length")
                {
                    result.Append($"{identifier.Name.Lexeme}->length()");
                }
                else
                {
                    throw new PerlangCompilerException($"Unsupported property {expr.Name.Lexeme} on array {identifier.Name.Lexeme}");
                }
            }
            else if (identifier.TypeReference.ClrType == typeof(PerlangEnum))
            {
                // Enums are represented as C++ enum classes, so we can just use the enum name directly
                result.Append($"{identifier.Name.Lexeme}::{expr.Name.Lexeme}");
            }
            else if (identifier.TypeReference.CppType != null)
            {
                string callOperator;

                if (!identifier.TypeReference.CppType.IsSupported)
                {
                    throw new NotImplementedInCompiledModeException($"Calling methods on type {identifier.TypeReference.CppType.TypeName} is not supported because the type is not yet implemented in C++");
                }

                if (identifier.TypeReference.CppWrapInSharedPtr)
                {
                    callOperator = "->";
                }
                else
                {
                    // TODO: This is the wrong exception type; should rather be something like InvalidOperationException
                    throw new NotImplementedInCompiledModeException($"Calling methods on {identifier} which is of type {identifier.TypeReference.CppType.TypeName} is not supported");
                }

                // This is possibly (an instance of) a Perlang class. Generate the expected C++ code for calling a method
                // on it.
                //
                // Note that this code path is used for 'this' invocations as well, resulting in 'this->method_name()'.
                // Because the name of the identifier is 'this', this works similarly as calling a method on any other
                // variable.
                result.Append($"{identifier.Name.Lexeme}{callOperator}{expr.Name.Lexeme}");
            }
            else
            {
                throw new PerlangCompilerException($"Internal compiler error: identifier.TypeReference.CppType was unexpectedly null for {identifier}");
            }
        }
        else if (expr.Object is Expr.Grouping or Expr.Index)
        {
            // TODO: Something like this would do eventually. It's pretty much impossible to perform a method call like
            // TODO: e.g. 42.get_type() in C or C++. We would at the very least have to special-case all primitives, to
            // TODO: emulate something similar in the Perlang world. It would give a nice, Smalltalk/Ruby:ish feel to the
            // TODO: language I think.
            result.Append(expr.Object.Accept(this));

            result.Append('.');
            result.Append(expr.Name.Lexeme);
        }
        else
        {
            throw new PerlangCompilerException($"Internal compiler error: unhandled type of get expression: {expr.Object}");
        }

        return result.ToString();
    }

    public object VisitNewExpression(Expr.NewExpression expr)
    {
        using var result = NativeStringBuilder.Create();

        // The NewExpression is expected to already have been resolved into an unambiguous constructor here, at which
        // point we merely create the corresponding C++ call and expect it to do the right thing.
        result.Append($"std::make_unique<{expr.TypeReference.CppType!.TypeName}>(");

        for (int i = 0; i < expr.Parameters.Count; i++) {
            result.Append(expr.Parameters[i].Accept(this));

            if (i != expr.Parameters.Count - 1) {
                result.Append(", ");
            }
        }

        result.Append(")");

        return result.ToString();
    }

    public object VisitBlockStmt(Stmt.Block block)
    {
        using var result = NativeStringBuilder.Create();

        result.Append(Indent(indentationLevel));
        result.AppendLine("{");
        indentationLevel++;

        foreach (Stmt stmt in block.Statements)
        {
            result.Append(stmt.Accept(this));
        }

        indentationLevel--;
        result.Append(Indent(indentationLevel));
        result.AppendLine("}");

        return result.ToString();
    }

    public object VisitClassStmt(Stmt.Class stmt)
    {
        using var classDefinitionBuilder = NativeStringBuilder.Create();
        using var classImplementationBuilder = NativeStringBuilder.Create();

        var previousClass = currentClass;
        currentClass = stmt;

        classDefinitionBuilder.AppendLine($$"""class {{stmt.Name}} {""");

        classDefinitionBuilder.AppendLine("private:");

        foreach (Stmt.Field field in stmt.Fields) {
            // All fields are private for now, but we explicitly check here to ensure that we get predictable errors if we
            // attempt to change this at some point to support public fields.
            if (field.Visibility != Visibility.Private) {
                throw new PerlangCompilerException($"Field {field.Name.Lexeme} is of {field.Visibility} visibility, which is not currently supported");
            }

            classDefinitionBuilder.Append(Indent(1));
            classDefinitionBuilder.Append(field.Accept(this));
        }

        // We make all methods `public` on the C++ side for now with this single `public` clause; there is no way to
        // define methods as `private` or `internal` anyway yet.
        classDefinitionBuilder.AppendLine("public:");

        foreach (Stmt.Function method in stmt.Methods) {
            // Likewise, check this to ensure we don't generate inconsistent code with what the Perlang model expects.
            if (method.Visibility != Visibility.Public) {
                throw new PerlangCompilerException($"Method {method.Name.Lexeme} is of {method.Visibility} visibility, which is not currently supported");
            }

            // Definition
            classDefinitionBuilder.Append(Indent(1));

            if (method.IsConstructor) {
                classDefinitionBuilder.Append($"{stmt.Name}(");
            }
            else if (method.IsDestructor) {
                classDefinitionBuilder.Append($"~{stmt.Name}(");
            }
            else {
                classDefinitionBuilder.Append($"{method.ReturnTypeReference.PossiblyWrappedCppType} {method.Name.Lexeme}(");
            }

            for (int i = 0; i < method.Parameters.Count; i++) {
                Parameter parameter = method.Parameters[i];
                classDefinitionBuilder.Append($"{parameter.TypeReference.PossiblyWrappedCppType} {parameter.Name.Lexeme}");

                if (i < method.Parameters.Count - 1) {
                    classDefinitionBuilder.Append(", ");
                }
            }

            classDefinitionBuilder.AppendLine(");");

            // Implementation
            if (!method.IsExtern) {
                if (method.IsConstructor) {
                    classImplementationBuilder.Append($"{stmt.Name}::{stmt.Name}(");
                }
                else if (method.IsDestructor) {
                    classImplementationBuilder.Append($"{stmt.Name}::~{stmt.Name}(");
                }
                else {
                    classImplementationBuilder.Append($"{method.ReturnTypeReference.PossiblyWrappedCppType} {stmt.Name}::{method.Name.Lexeme}(");
                }

                for (int i = 0; i < method.Parameters.Count; i++) {
                    Parameter parameter = method.Parameters[i];
                    classImplementationBuilder.Append($"{parameter.TypeReference.PossiblyWrappedCppType} {parameter.Name.Lexeme}");

                    if (i < method.Parameters.Count - 1) {
                        classImplementationBuilder.Append(", ");
                    }
                }

                classImplementationBuilder.AppendLine(") {");
                classImplementationBuilder.Append(method.Accept(this));
                classImplementationBuilder.AppendLine("};");

                classImplementationBuilder.AppendLine();
            }
        }

        classDefinitionBuilder.AppendLine("};");

        classDefinitions[stmt.Name] = classDefinitionBuilder.ToString();
        classImplementations[stmt.Name] = classImplementationBuilder.ToString();

        currentClass = previousClass;

        // Does not need to return the StringBuilder here, since it's been stored in the enums dictionary already.
        return VoidObject.Void;
    }

    public object VisitEnumStmt(Stmt.Enum stmt)
    {
        using var stringBuilder = NativeStringBuilder.Create();

        int localIndentationLevel = 0;

        // The "enum class" concept as defined in C++11 would be preferable here since it provides better type safety, but
        // it makes it much more awkward to support things like `print MyEnum.Value` (since the enum values are not
        // implicitly convertible to int). We use a hack suggested in https://stackoverflow.com/a/46294875/227779 for now;
        // preventing implicit conversions between int/enum types can be handled on the Perlang level rather than in the
        // C++ transpilation.
        stringBuilder.Append(Indent(localIndentationLevel));
        stringBuilder.AppendLine($"namespace {stmt.Name.Lexeme} {{");
        localIndentationLevel++;

        stringBuilder.Append(Indent(localIndentationLevel));
        stringBuilder.AppendLine($"enum {stmt.Name.Lexeme} {{");
        localIndentationLevel++;

        foreach (KeyValuePair<string, Expr?> enumMember in stmt.Members) {
            stringBuilder.Append(Indent(localIndentationLevel));
            stringBuilder.Append(enumMember.Key);

            if (enumMember.Value != null)
            {
                stringBuilder.Append($" = {enumMember.Value.Accept(this)}");
            }

            stringBuilder.AppendLine(",");
        }

        localIndentationLevel--;
        stringBuilder.Append(Indent(localIndentationLevel));
        stringBuilder.AppendLine("};");

        localIndentationLevel--;
        stringBuilder.Append(Indent(localIndentationLevel));
        stringBuilder.AppendLine("};");

        enums[stmt.Name.Lexeme] = stringBuilder.ToString();

        // Does not need to return the StringBuilder here, since it's been stored in the enums dictionary already.
        return VoidObject.Void;
    }

    public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
    {
        using var result = NativeStringBuilder.Create();

        if (stmt.Expression is Expr.Empty) {
            // Empty expressions are sometimes emitted by the compiler. We ignore them at this point, since they will
            // otherwise make the output look a bit funny (bare semicolons indented to the right level).
            return String.Empty;
        }

        // We only emit the "wrapping" (whitespace before + semicolon and newline after expression) in this case; the
        // rest comes from visiting the expression itself
        result.Append(Indent(indentationLevel));
        result.Append(stmt.Expression.Accept(this));
        result.AppendLine(";"); // detta löser inte problemet

        return result.ToString();
    }

    public object VisitFunctionStmt(Stmt.Function functionStmt)
    {
        // Disposed via Dispose() method for class
#pragma warning disable CA2000
        var functionContent = NativeStringBuilder.Create();
#pragma warning restore CA2000

        if (methods.ContainsKey(functionStmt.Name.Lexeme))
        {
            throw new PerlangCompilerException($"Function '{functionStmt.Name.Lexeme}' is already defined");
        }

        if (currentClass == null) {
            methods[functionStmt.Name.Lexeme] = new Method(
                functionStmt.Name.Lexeme,
                functionStmt.Parameters,
                functionStmt.ReturnTypeReference.PossiblyWrappedCppType ?? throw new PerlangCompilerException($"Internal compiler error: return type of function '{functionStmt.Name.Lexeme}' was unexpectedly null"),
                functionContent
            );
        }

        // Loop over the statements in the method body and visit them, recursively
        foreach (Stmt stmt in functionStmt.Body)
        {
            functionContent.Append(stmt.Accept(this));
        }

        if (currentClass == null) {
            // Does not need to return the StringBuilder here, since it's been stored in the methods dictionary already.
            return VoidObject.Void;
        }
        else {
            return functionContent.ToString();
        }
    }

    public object VisitFieldStmt(Stmt.Field stmt)
    {
        using var result = NativeStringBuilder.Create();

        result.Append($"{stmt.TypeReference.PossiblyWrappedCppType} {stmt.Name.Lexeme}");

        if (stmt.Initializer != null)
        {
            result.AppendLine($" = {stmt.Initializer.Accept(this)};");
        }
        else
        {
            result.AppendLine(";");
        }

        return result.ToString();
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        using var result = NativeStringBuilder.Create();

        result.Append(Indent(indentationLevel));
        result.Append($"if ({stmt.Condition.Accept(this)}) ");

        if (!(stmt.ThenBranch is Stmt.Block))
        {
            result.AppendLine();
        }

        indentationLevel++;
        result.Append(stmt.ThenBranch.Accept(this));
        indentationLevel--;

        if (stmt.ElseBranch != null)
        {
            result.Append(Indent(indentationLevel));
            result.Append("else ");

            if (!(stmt.ElseBranch is Stmt.Block))
            {
                result.AppendLine();
            }

            indentationLevel++;
            result.Append(stmt.ElseBranch.Accept(this));
            indentationLevel--;
        }

        return result.ToString();
    }

    public object VisitPrintStmt(Stmt.Print stmt) =>
        $"{Indent(indentationLevel)}perlang::print({stmt.Expression.Accept(this)});\n";

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        using var result = NativeStringBuilder.Create();

        if (stmt.Value == null)
        {
            // void return
            result.AppendLine($"{Indent(indentationLevel)}return;");
        }
        else
        {
            result.Append($"{Indent(indentationLevel)}return ");
            result.Append(stmt.Value.Accept(this));
            result.AppendLine(";");
        }

        return result.ToString();
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        using var result = NativeStringBuilder.Create();

        string variableName = stmt.Name.Lexeme;

        result.Append($"{Indent(indentationLevel)}{stmt.TypeReference.PossiblyWrappedCppType} {variableName}");

        if (stmt.Initializer != null)
        {
            result.AppendLine($" = {stmt.Initializer.Accept(this)};");
        }
        else
        {
            result.AppendLine(";");
        }

        return result.ToString();
    }

    public object VisitWhileStmt(Stmt.While whileStmt)
    {
        using var result = NativeStringBuilder.Create();

        result.Append(Indent(indentationLevel));
        result.Append($"while ({whileStmt.Condition.Accept(this)}) ");
        result.Append(whileStmt.Body.Accept(this));
        result.AppendLine(";");

        return result.ToString();
    }

    private static string Indent(int level) => String.Empty.PadLeft(level * 4);

    private record Method(string Name, IImmutableList<Parameter> Parameters, string ReturnType, NativeStringBuilder MethodBody)
    {
        /// <summary>
        /// Gets the method parameters as a comma-separated string, in C++ format. Example: `int foo, int bar`.
        /// </summary>
        public string ParametersString { get; } = String.Join(", ", Parameters
            .Select(p =>
                $"{p.TypeReference.PossiblyWrappedCppType} " +
                $"{p.Name.Lexeme}"
            )
        );
    }
}
