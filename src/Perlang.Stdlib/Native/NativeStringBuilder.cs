#pragma warning disable SA1300
#pragma warning disable SA1601
#pragma warning disable S1117
#pragma warning disable S3877
#nullable enable

using System;
using System.Runtime.InteropServices;

namespace Perlang.Native;

public partial class NativeStringBuilder : IDisposable
{
    public int Length => _StringBuilder_length(nativeStringBuilder);

    private readonly IntPtr nativeStringBuilder;

    private bool disposed;

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_new")]
    private static partial IntPtr _StringBuilder_new();

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_delete")]
    private static partial void _StringBuilder_delete(IntPtr nativeStringBuilder);

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_append", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void _StringBuilder_append(IntPtr nativeStringBuilder, string s);

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_append_line", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void _StringBuilder_append_line(IntPtr nativeStringBuilder, string? s);

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_length")]
    private static partial int _StringBuilder_length(IntPtr nativeStringBuilder);

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_to_string")]
    private static partial IntPtr _StringBuilder_to_string(IntPtr nativeStringBuilder);

    [LibraryImport("perlang_cli", EntryPoint = "StringBuilder_delete_to_string_result")]
    private static partial void _StringBuilder_delete_to_string_result(IntPtr result);

    public static NativeStringBuilder Create()
    {
        IntPtr nativeStringBuilder = _StringBuilder_new();

        try {
            return new NativeStringBuilder(nativeStringBuilder);
        }
        catch {
            _StringBuilder_delete(nativeStringBuilder);
            throw;
        }
    }

    private NativeStringBuilder(IntPtr nativeStringBuilder)
    {
        this.nativeStringBuilder = nativeStringBuilder;
    }

    public void Dispose()
    {
        if (!disposed) {
            _StringBuilder_delete(nativeStringBuilder);
            disposed = true;
        }
    }

    public override string ToString()
    {
        IntPtr result = _StringBuilder_to_string(nativeStringBuilder);

        try {
            if (result == IntPtr.Zero) {
                throw new InvalidOperationException("StringBuilder_to_string returned a null pointer");
            }

            return Marshal.PtrToStringUTF8(result)!;
        }
        finally {
            _StringBuilder_delete_to_string_result(result);
        }
    }

    public void Append(char c)
    {
        // TODO: Add a char-based overload to the C++ implementation. The problem is with non-ASCII data, which requires
        // TODO: some more fiddling to be handled correctly.
        _StringBuilder_append(nativeStringBuilder, c.ToString());
    }

    public void Append(object? o)
    {
        if (o == null) {
            return;
        }

        // TODO: Think this through when we drop this .NET wrapper. We'll have to handle this on the Perlang/C++ side
        // TODO: when we are no longer able to rely on ToString() doing the work for us.
        _StringBuilder_append(nativeStringBuilder, o.ToString()!);
    }

    public void Append(string s)
    {
        _StringBuilder_append(nativeStringBuilder, s);
    }

    public void AppendLine(string? s = null)
    {
        _StringBuilder_append_line(nativeStringBuilder, s);
    }
}
