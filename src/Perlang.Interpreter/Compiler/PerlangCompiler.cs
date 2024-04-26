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
using System.Text;
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
using Perlang.Parser;
using Perlang.Stdlib;
using static Perlang.TokenType;
using String = System.String;

namespace Perlang.Interpreter.Compiler;

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
/// methods in this class as expected.</remarks>
/// </summary>
public class PerlangCompiler : Expr.IVisitor<object?>, Stmt.IVisitor<VoidObject>
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
    private readonly IImmutableDictionary<string, Type> superGlobals;

    /// <summary>
    /// A collection of all currently defined global classes (both native/.NET and classes defined in Perlang code.)
    /// </summary>
    private readonly IDictionary<string, object> globalClasses = new Dictionary<string, object>();

    private readonly ImmutableDictionary<string, Type> nativeClasses;
    private readonly IDictionary<string, Method> methods;

    private StringBuilder currentMethod;

    private int indentationLevel = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerlangCompiler"/> class.
    /// </summary>
    /// <param name="runtimeErrorHandler">A callback that will be called on runtime errors. Note that after calling
    ///     this handler, the interpreter will abort the script.</param>
    /// <param name="standardOutputHandler">An callback that will receive output printed to standard output.</param>
    /// <param name="bindingHandler">A binding handler, or `null` to let the interpreter create a new instance.</param>
    /// <param name="arguments">An optional list of runtime arguments.</param>
    public PerlangCompiler(
        Action<RuntimeError> runtimeErrorHandler,
        Action<Lang.String> standardOutputHandler,
        IBindingHandler? bindingHandler = null,
        IEnumerable<string>? arguments = null)
    {
        this.runtimeErrorHandler = runtimeErrorHandler;
        this.standardOutputHandler = standardOutputHandler;
        this.BindingHandler = bindingHandler ?? new BindingHandler();

        this.currentMethod = new StringBuilder();
        this.methods = new Dictionary<string, Method>();

        methods["main"] = new Method("main", ImmutableList.Create<Parameter>(), "int", currentMethod);

        var argumentsList = (arguments ?? Array.Empty<string>()).ToImmutableList();

        superGlobals = CreateSuperGlobals(argumentsList);

        LoadStdlib();
        nativeClasses = RegisterGlobalFunctionsAndClasses();
    }

    private IImmutableDictionary<string, Type> CreateSuperGlobals(ImmutableList<string> argumentsList)
    {
        // Set up the super-global ARGV variable.
        var result = new Dictionary<string, Type>
        {
            { "ARGV", typeof(Argv) }
        }.ToImmutableDictionary();

        // TODO: Returning a value AND modifying the globals like this feels like a code smell. Try to figure out
        // TODO: a more sensible way.
        globals.Define(new Token(VAR, "ARGV", null, -1), new Argv(argumentsList));

        return result;
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
    /// <param name="path">The path to the source file (used for generating error messages).</param>
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
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler)
    {
        string? executablePath = Compile(
            source,
            path,
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

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

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
            runtimeErrorHandler(new RuntimeError(null, $"Process {executablePath} exited with exit code {process.ExitCode}"));
        }

        return executablePath;
    }

    /// <summary>
    /// Compiles and assembles the given Perlang program to a .o file (ELF object file).
    /// </summary>
    /// <param name="source">The Perlang program to compile.</param>
    /// <param name="path">The path to the source file (used for generating error messages).</param>
    /// <param name="compilerFlags">One or more <see cref="CompilerFlags"/> to use.</param>
    /// <param name="scanErrorHandler">A handler for scanner errors.</param>
    /// <param name="parseErrorHandler">A handler for parse errors.</param>
    /// <param name="nameResolutionErrorHandler">A handler for resolve errors.</param>
    /// <param name="typeValidationErrorHandler">A handler for type validation errors.</param>
    /// <param name="immutabilityValidationErrorHandler">A handler for immutability validation errors.</param>
    /// <param name="compilerWarningHandler">A handler for compiler warnings.</param>
    /// <returns>The path to the compiled .o file. Note that this will be non-null even on unsuccessful
    /// compilation.</returns>
    public string? CompileAndAssemble(
        string source,
        string path,
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler)
    {
        string? executablePath = Compile(
            source,
            path,
            compilerFlags,
            scanErrorHandler,
            parseErrorHandler,
            nameResolutionErrorHandler,
            typeValidationErrorHandler,
            immutabilityValidationErrorHandler,
            compilerWarningHandler,
            compileAndAssembleOnly: true
        );

        return executablePath;
    }

    /// <summary>
    /// Compiles the given Perlang program to an executable.
    /// </summary>
    /// <param name="source">The Perlang program to compile.</param>
    /// <param name="path">The path to the source file (used for generating error messages).</param>
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
        string source,
        string path,
        CompilerFlags compilerFlags,
        ScanErrorHandler scanErrorHandler,
        ParseErrorHandler parseErrorHandler,
        NameResolutionErrorHandler nameResolutionErrorHandler,
        ValidationErrorHandler typeValidationErrorHandler,
        ValidationErrorHandler immutabilityValidationErrorHandler,
        CompilerWarningHandler compilerWarningHandler,
        bool compileAndAssembleOnly = false)
    {
        string targetCppFile = Path.ChangeExtension(path, ".cc");

#if _WINDOWS
        // clang is very unlikely to have been available on Windows anyway, but why not...
        string targetExecutable = Path.ChangeExtension(targetCppFile, compileAndAssembleOnly ? "obj" : "exe");
#else
        string targetExecutable = Path.ChangeExtension(targetCppFile, compileAndAssembleOnly ? "o" : null);
#endif

        // TODO: Check the creation time of *all* dependencies here, including the stdlib (both .so/.dll and .h files
        // TODO: ideally). Right now, rebuilding the stdlib doesn't trigger a cache invalidation which is annoying
        // TODO: when developing the stdlib.
        if (!(compilerFlags.HasFlag(CompilerFlags.CacheDisabled) || CompilationCacheDisabled) &&
            File.GetCreationTime(targetCppFile) > File.GetCreationTime(path) &&
            File.GetCreationTime(targetExecutable) > File.GetCreationTime(path))
        {
            // Both the .cc file and the executable are newer than the given Perlang script => no need to
            // compile it. We presume the binary to be already up-to-date.
            return targetExecutable;
        }

        ScanAndParseResult result = PerlangParser.ScanAndParse(
            source,
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
            cppPrototypes = result.CppPrototypes!.ToImmutableList();
            cppMethods = result.CppMethods!.ToImmutableList();
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
            superGlobals,
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
            BindingHandler.GetVariableOrFunctionBinding,
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

            // The AST traverser writes its output to the transpiledSource field.
            Compile(statements);

            using (StreamWriter streamWriter = File.CreateText(targetCppFile))
            {
                // Write standard file header, which includes everything our transpiled code might expect.
                streamWriter.Write($@"// Automatically generated code by Perlang {CommonConstants.InformationalVersion} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include ""bigint.hpp"" // BigInt
#include ""stdlib.hpp""

");

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

                if (methods.Count > 0) {
                    streamWriter.WriteLine("//");
                    streamWriter.WriteLine("// Method definitions");
                    streamWriter.WriteLine("//");

                    foreach (var (key, value) in methods) {
                        streamWriter.WriteLine($"{value.ReturnType} {key}({value.ParametersString});");
                    }

                    streamWriter.WriteLine();
                }

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
                    // implementing warnings for them but we want them on the Perlang level instead of on clang level in
                    // that case.
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

                    // 2147483647 >> 32 is valid in e.g. Java, but generates a warning with clang. The warning is useful
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
    private void Compile(IEnumerable<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            Compile(statement);
        }
    }

    private void Compile(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void AddGlobalClass(string name, PerlangClass perlangClass)
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
        currentMethod.Append($"{expr.Identifier.Name.Lexeme} = {expr.Value.Accept(this)}");
        return VoidObject.Void;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
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
                // TODO: We might need something like CheckNumberOperands() in PerlangInterpreter here, but we can obviously not do any value-based checking since we are a compiler and no
                currentMethod.Append($"{expr.Left.Accept(this)} > {expr.Right.Accept(this)}");
                break;

            case GREATER_EQUAL:
                currentMethod.Append($"{expr.Left.Accept(this)} >= {expr.Right.Accept(this)}");
                break;

            case LESS:
                currentMethod.Append($"{expr.Left.Accept(this)} < {expr.Right.Accept(this)}");
                break;

            case LESS_EQUAL:
                currentMethod.Append($"{expr.Left.Accept(this)} <= {expr.Right.Accept(this)}");
                break;

            case BANG_EQUAL:
                if (expr.Left.TypeReference.IsStringType() && expr.Left.TypeReference.CppWrapInSharedPtr &&
                    expr.Right.TypeReference.IsStringType() && expr.Right.TypeReference.CppWrapInSharedPtr)
                {
                    // Example generated code: *s1 != *s2
                    currentMethod.Append($"*{expr.Left.Accept(this)} != *{expr.Right.Accept(this)}");
                }
                else
                {
                    currentMethod.Append($"{expr.Left.Accept(this)} != {expr.Right.Accept(this)}");
                }

                break;

            case EQUAL_EQUAL:
                if (expr.Left.TypeReference.IsStringType() && expr.Left.TypeReference.CppWrapInSharedPtr &&
                    expr.Right.TypeReference.IsStringType() && expr.Right.TypeReference.CppWrapInSharedPtr)
                {
                    // Example generated code: *s1 == *s2
                    currentMethod.Append($"*{expr.Left.Accept(this)} == *{expr.Right.Accept(this)}");
                }
                else
                {
                    currentMethod.Append($"{expr.Left.Accept(this)} == {expr.Right.Accept(this)}");
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
                    leftCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType)
                {
                    rightCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    currentMethod.Append($"{leftCast}{expr.Left.Accept(this)} - {rightCast}{expr.Right.Accept(this)}");
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
                    currentMethod.Append($"{expr.Left.Accept(this)} -= {expr.Right.Accept(this)}");
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
                if (expr.TypeReference.ClrType != expr.Left.TypeReference.ClrType)
                {
                    leftCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType)
                {
                    rightCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    currentMethod.Append($"{leftCast}{expr.Left.Accept(this)} + {rightCast}{expr.Right.Accept(this)}");
                }
                else if (expr.Left.TypeReference.IsStringType() && expr.Right.TypeReference.IsStringType())
                {
                    throw new NotImplementedInCompiledModeException($"Concatenation between {expr.Left.TypeReference.ClrType.ToTypeKeyword()} and {expr.Right.TypeReference.ClrType.ToTypeKeyword()} is not yet implemented");
                }
                else if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsStringType())
                {
                    throw new NotImplementedInCompiledModeException($"Concatenation between {expr.Left.TypeReference.ClrType.ToTypeKeyword()} and {expr.Right.TypeReference.ClrType.ToTypeKeyword()} is not yet implemented");
                }
                else if (expr.Left.TypeReference.IsStringType() && expr.Right.TypeReference.IsValidNumberType)
                {
                    throw new NotImplementedInCompiledModeException($"Concatenation between {expr.Left.TypeReference.ClrType.ToTypeKeyword()} and {expr.Right.TypeReference.ClrType.ToTypeKeyword()} is not yet implemented");
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
                    currentMethod.Append($"{expr.Left.Accept(this)} += {expr.Right.Accept(this)}");
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
                    currentMethod.Append($"{expr.Left.Accept(this)} / {expr.Right.Accept(this)}");
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
                    leftCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.TypeReference.ClrType != expr.Right.TypeReference.ClrType)
                {
                    rightCast = expr.TypeReference.CppTypeCast;
                }

                if (expr.Left.TypeReference.IsValidNumberType && expr.Right.TypeReference.IsValidNumberType)
                {
                    currentMethod.Append($"{leftCast}{expr.Left.Accept(this)} * {rightCast}{expr.Right.Accept(this)}");
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
                    currentMethod.Append($"{leftCast}perlang::BigInt_pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                }
                else if (new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(expr.Left.TypeReference.ClrType) &&
                         new[] { typeof(float), typeof(double) }.Contains(expr.Right.TypeReference.ClrType))
                {
                    // Normal math.h-based pow(), returning a double
                    currentMethod.Append($"{leftCast}pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                }
                else if (new[] { typeof(float), typeof(double) }.Contains(expr.Left.TypeReference.ClrType) &&
                         new[] { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(float), typeof(double) }.Contains(expr.Right.TypeReference.ClrType))
                {
                    // Normal math.h-based pow(), returning a double
                    currentMethod.Append($"{leftCast}pow({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
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
                        currentMethod.Append($"{leftCast}perlang::BigInt_mod({expr.Left.Accept(this)}, {rightCast}{expr.Right.Accept(this)})");
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(double) ||
                             expr.Right.TypeReference.ClrType == typeof(double))
                    {
                        // C and C++ does not support the % operator for double; we must use `fmod()` instead
                        currentMethod.Append($"fmod({expr.Left.Accept(this)}, {expr.Right.Accept(this)})");
                    }
                    else if (expr.Left.TypeReference.ClrType == typeof(float) ||
                             expr.Right.TypeReference.ClrType == typeof(float))
                    {
                        // Likewise, but with `float` instead of `double` as arguments and return type
                        currentMethod.Append($"fmodf({expr.Left.Accept(this)}, {expr.Right.Accept(this)})");
                    }
                    else
                    {
                        currentMethod.Append($"{expr.Left.Accept(this)} % {expr.Right.Accept(this)}");
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
                    currentMethod.Append($"({expr.Left.Accept(this)} << {expr.Right.Accept(this)})");
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
                    currentMethod.Append($"({expr.Left.Accept(this)} >> ({expr.Right.Accept(this)} {shiftCountBitMask}))");
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

        return VoidObject.Void;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        // TODO: In interpreted mode, information about the callee is not really known at this point; we get information
        // TODO: about it by calling Evaluate(expr.Callee). In compiled mode, it would probably make sense to do method
        // TODO: binding at an earlier stage and e.g. validate the number and type of arguments with the method
        // TODO: definition. Right now, we'll leave this to the C++ compiler to deal with...

        currentMethod.Append(expr.Callee.Accept(this));
        currentMethod.Append('(');

        for (int index = 0; index < expr.Arguments.Count; index++)
        {
            Expr argument = expr.Arguments[index];
            argument.Accept(this);

            if (index < expr.Arguments.Count - 1)
            {
                currentMethod.Append(", ");
            }
        }

        currentMethod.Append(')');

        return VoidObject.Void;
    }

    public object? VisitIndexExpr(Expr.Index expr)
    {
        // TODO: Consider doing range checking at this point. In general, try to figure out a balanced approach in the
        // TODO: "safe" vs "fast" dichotomy. C and C++ are fast (no array bounds checking) but can blow up in your face
        // TODO: if you're not careful. Java and .NET are safe (all array indexing is bounds-checked) but this
        // TODO: inherently *will* have a performance impact. Sometimes this is negligible but sometimes not.
        // TODO:
        // TODO: One way to deal with this: make `foo[bar]` be safe and add an "unsafe" indexing operator `foo[[bar]]`
        // TODO: which can be used when the user believes he knows best. Investigate how Rust is doing this.
        expr.Indexee.Accept(this);

        ITypeReference indexeeTypeReference = expr.Indexee.TypeReference;

        if (indexeeTypeReference.CppWrapInSharedPtr && indexeeTypeReference.IsStringType())
        {
            // This object is wrapped in a std::shared_ptr<T>, which cannot be indexed as a non-wrapped type. We call the
            // char_at() method to help us solve this.
            currentMethod.Append("->char_at(");
        }
        else
        {
            currentMethod.Append('[');
        }

        expr.Argument.Accept(this);

        if (indexeeTypeReference.CppWrapInSharedPtr && indexeeTypeReference.IsStringType())
        {
            currentMethod.Append(')');
        }
        else
        {
            currentMethod.Append(']');
        }

        return VoidObject.Void;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        currentMethod.Append('(');
        expr.Expression.Accept(this);
        currentMethod.Append(')');

        return VoidObject.Void;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        if (expr.Value is AsciiString)
        {
            // The value is an ASCII string literal, so it is safe to use this factory method to wrap it in an AsciiString
            // on the C++ side. (The "static string" is the important part here; because we know that it's a literal, we
            // can assume "shared" ownership of it and assume that it will never be deallocated for the whole lifetime of
            // the program.)
            currentMethod.Append("perlang::ASCIIString::from_static_string(\"");
            currentMethod.Append(expr);
            currentMethod.Append("\")");
        }
        else if (expr.Value is Utf8String)
        {
            // The value is an UTF-8 string literal, so it is safe to use this factory method to wrap it in an UTF8String
            // on the C++ side. (The "static string" is the important part here; because we know that it's a literal, we
            // can assume "shared" ownership of it and assume that it will never be deallocated for the whole lifetime of
            // the program.)
            currentMethod.Append("perlang::UTF8String::from_static_string(\"");
            currentMethod.Append(expr);
            currentMethod.Append("\")");
        }
        else if (expr.Value == null)
        {
            // C++11 defines nullptr as a keyword
            currentMethod.Append("nullptr");
        }
        else if (expr.Value is IntegerLiteral<uint> uintLiteral)
        {
            // TODO: should be UL when targeting 32-bit platforms and U for 64-bit.
            currentMethod.Append(uintLiteral.Value);
            currentMethod.Append('U'); // unsigned
        }
        else if (expr.Value is IntegerLiteral<ulong> ulongLiteral)
        {
            // FIXME: should be ULL for 32-bit platforms
            currentMethod.Append(ulongLiteral.Value);
            currentMethod.Append("UL"); // unsigned long
        }
        else if (expr.Value is IFloatingPointLiteral floatingPointLiteral)
        {
            // We want to be careful to avoid using Value for floating point literals, since we we want to preserve the
            // exact value that the user entered as-is, without any potential loss of precision
            currentMethod.Append(floatingPointLiteral.NumberCharacters);

            if (floatingPointLiteral is FloatingPointLiteral<double>)
            {
                // double is the default in C++, so no need for any suffix in this case
            }
            else if (floatingPointLiteral is FloatingPointLiteral<float>)
            {
                currentMethod.Append('f');
            }
            else
            {
                throw new PerlangCompilerException($"Internal compiler error: unsupported floating point literal type {expr.Value.GetType().ToTypeKeyword()} encountered");
            }
        }
        else if (expr.Value is IntegerLiteral<BigInteger> bigintLiteral)
        {
            currentMethod.Append("BigInt(\"");
            currentMethod.Append(bigintLiteral.Value.ToString());
            currentMethod.Append("\")");
        }
        else if (expr.Value is INumericLiteral numericLiteral)
        {
            // This will work for all other numeric types we support
            currentMethod.Append(numericLiteral.Value);
        }
        else if (expr.Value is bool boolLiteral)
        {
            // true.ToString() == "True", which is why we need this manual logic.
            currentMethod.Append(boolLiteral ? "true" : "false");
        }
        else
        {
            throw new PerlangCompilerException($"Internal compiler error: unsupported type {expr.Value.GetType().ToTypeKeyword()} encountered");
        }

        return VoidObject.Void;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        switch (expr.Operator.Type)
        {
            case PIPE_PIPE:
                expr.Left.Accept(this);
                currentMethod.Append("||");
                expr.Right.Accept(this);
                break;

            case AMPERSAND_AMPERSAND:
                expr.Left.Accept(this);
                currentMethod.Append("&&");
                expr.Right.Accept(this);
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInLogicalExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return VoidObject.Void;
    }

    public object? VisitUnaryPrefixExpr(Expr.UnaryPrefix expr)
    {
        switch (expr.Operator.Type)
        {
            case BANG:
                currentMethod.Append('!');
                expr.Right.Accept(this);
                break;

            case MINUS:
                currentMethod.Append('-');
                expr.Right.Accept(this);
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInUnaryPrefixExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return VoidObject.Void;
    }

    public object? VisitUnaryPostfixExpr(Expr.UnaryPostfix expr)
    {
        switch (expr.Operator.Type)
        {
            case PLUS_PLUS:
                expr.Left.Accept(this);
                currentMethod.Append("++");
                break;

            case MINUS_MINUS:
                expr.Left.Accept(this);
                currentMethod.Append("--");
                break;

            default:
                string message = CompilerMessages.UnsupportedOperatorTypeInUnaryPostfixExpression(expr.Operator.Type);
                throw new RuntimeError(expr.Operator, message);
        }

        return VoidObject.Void;
    }

    public object? VisitIdentifierExpr(Expr.Identifier expr)
    {
        currentMethod.Append(expr.Name.Lexeme);
        return VoidObject.Void;
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        if (expr.Object is Expr.Identifier identifier)
        {
            if (globalClasses.ContainsKey(identifier.Name.Lexeme))
            {
                // These classes are put in the `perlang::stdlib` namespace in the C++ world
                currentMethod.Append($"perlang::stdlib::{identifier.Name.Lexeme}");

                // Static methods are called with `class::method(params)` syntax in C++
                currentMethod.Append("::");
                currentMethod.Append(expr.Name.Lexeme);
            }
            else
            {
                // TODO: Should support global constants here at least! We just need to make them exist in the C++ based
                // TODO: stdlib and define where in the C++ realm the Perlang global variables are to exist.

                // TODO: We need to differentiate here between different kind of identifiers.
                throw new NotImplementedInCompiledModeException($"Calling methods on {identifier} is not yet implemented in compiled mode");
            }
        }
        else if (expr.Object is Expr.Grouping or Expr.Index)
        {
            // TODO: Something like this would do eventually... but until we support user-defined classes, it's kind of
            // TODO: moot anyway. It's pretty much impossible to perform a method call like e.g. 42.get_type() in C or
            // TODO: C++. We would at the very least have to special-case all primitives, to emulate something similar
            // TODO: in the Perlang world.
            expr.Object.Accept(this);

            currentMethod.Append('.');
            currentMethod.Append(expr.Name.Lexeme);
        }
        else
        {
            throw new PerlangCompilerException($"Internal compiler error: unhandled type of get expression: {expr.Object}");
        }

        return VoidObject.Void;
    }

    public VoidObject VisitBlockStmt(Stmt.Block block)
    {
        currentMethod.Append(Indent(indentationLevel));
        currentMethod.AppendLine("{");
        indentationLevel++;

        foreach (Stmt stmt in block.Statements)
        {
            stmt.Accept(this);
        }

        indentationLevel--;
        currentMethod.Append(Indent(indentationLevel));
        currentMethod.AppendLine("}");

        return VoidObject.Void;
    }

    public VoidObject VisitClassStmt(Stmt.Class stmt)
    {
        throw new NotImplementedException();
    }

    public VoidObject VisitExpressionStmt(Stmt.ExpressionStmt stmt)
    {
        // We only emit the "wrapping" (whitespace before + semicolon and newline after expression)  in this case; the
        // rest comes from visiting the expression itself
        currentMethod.Append(Indent(indentationLevel));
        stmt.Expression.Accept(this);
        currentMethod.AppendLine(";");

        return VoidObject.Void;
    }

    public VoidObject VisitFunctionStmt(Stmt.Function functionStmt)
    {
        StringBuilder previousMethod = currentMethod;

        currentMethod = new StringBuilder();

        if (methods.ContainsKey(functionStmt.Name.Lexeme))
        {
            throw new PerlangCompilerException($"Function '{functionStmt.Name.Lexeme}' is already defined");
        }

        methods[functionStmt.Name.Lexeme] = new Method(
            functionStmt.Name.Lexeme,
            functionStmt.Parameters,
            $"{(functionStmt.ReturnTypeReference.CppWrapInSharedPtr ? "std::shared_ptr<const " : "")}" +
            $"{functionStmt.ReturnTypeReference.CppType}" +
            $"{(functionStmt.ReturnTypeReference.CppWrapInSharedPtr ? ">" : "")}",
            currentMethod
        );

        // Loop over the statements in the method body and visit them, recursively
        foreach (Stmt stmt in functionStmt.Body)
        {
            stmt.Accept(this);
        }

        currentMethod = previousMethod;

        return VoidObject.Void;
    }

    public VoidObject VisitIfStmt(Stmt.If stmt)
    {
        currentMethod.Append(Indent(indentationLevel));
        currentMethod.Append($"if ({stmt.Condition.Accept(this)}) ");

        if (!(stmt.ThenBranch is Stmt.Block))
        {
            currentMethod.AppendLine();
        }

        indentationLevel++;
        stmt.ThenBranch.Accept(this);
        indentationLevel--;

        if (stmt.ElseBranch != null)
        {
            currentMethod.Append(Indent(indentationLevel));
            currentMethod.Append("else ");

            if (!(stmt.ElseBranch is Stmt.Block))
            {
                currentMethod.AppendLine();
            }

            indentationLevel++;
            stmt.ElseBranch.Accept(this);
            indentationLevel--;
        }

        return VoidObject.Void;
    }

    public VoidObject VisitPrintStmt(Stmt.Print stmt)
    {
        currentMethod.AppendLine($"{Indent(indentationLevel)}perlang::print({stmt.Expression.Accept(this)});");

        return VoidObject.Void;
    }

    public VoidObject VisitReturnStmt(Stmt.Return stmt)
    {
        if (stmt.Value == null)
        {
            // void return
            currentMethod.AppendLine($"{Indent(indentationLevel)}return;");
        }
        else
        {
            currentMethod.Append($"{Indent(indentationLevel)}return ");
            stmt.Value.Accept(this);
            currentMethod.AppendLine(";");
        }

        return VoidObject.Void;
    }

    public VoidObject VisitVarStmt(Stmt.Var stmt)
    {
        string variableName = stmt.Name.Lexeme;

        currentMethod.Append($"{Indent(indentationLevel)}{stmt.TypeReference.PossiblyWrappedCppType} {variableName}");

        if (stmt.HasInitializer)
        {
            currentMethod.AppendLine($" = {stmt.Initializer.Accept(this)};");
        }
        else
        {
            currentMethod.AppendLine(";");
        }

        return VoidObject.Void;
    }

    public VoidObject VisitWhileStmt(Stmt.While whileStmt)
    {
        currentMethod.Append(Indent(indentationLevel));
        currentMethod.Append($"while ({whileStmt.Condition.Accept(this)}) ");
        whileStmt.Body.Accept(this);
        currentMethod.AppendLine(";");

        return VoidObject.Void;
    }

    // public VoidObject VisitPreprocessorDirective(Stmt.PreprocessorDirective preprocessorDirective)
    // {
    //     if (preprocessorDirective.Type == PreprocessorDirectiveType.Prototypes) {
    //         cppPrototypes.Add(preprocessorDirective.Content);
    //     }
    //     else if (preprocessorDirective.Type == PreprocessorDirectiveType.Methods) {
    //         cppMethods.Add(preprocessorDirective.Content);
    //     }
    //     else {
    //         throw new PerlangCompilerException(
    //             $"Unsupported preprocessor directive {preprocessorDirective.Type} encountered"
    //         );
    //     }
    //
    //     return VoidObject.Void;
    // }

    private static string Indent(int level) => String.Empty.PadLeft(level * 4);

    private record Method(string Name, IImmutableList<Parameter> Parameters, string ReturnType, StringBuilder MethodBody)
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

    public string? Parse(string source, Action<ScanError> scanError, Action<ParseError> parseError)
    {
        throw new NotImplementedException();
    }
}
