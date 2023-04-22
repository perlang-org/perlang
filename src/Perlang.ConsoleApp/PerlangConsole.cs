#nullable enable
using System;
using System.CommandLine.IO;
using System.Runtime.InteropServices;
using System.Text;
using Perlang.Exceptions;
using Perlang.Stdlib;
using String = Perlang.Lang.String;

namespace Perlang.ConsoleApp;

// Shamelessly inspired by System.CommandLine.IO.SystemConsole,
// Copyright (c) .NET Foundation and contributors. All rights reserved.
public class PerlangConsole : IPerlangConsole
{
    public PerlangConsole()
    {
        Error = StandardErrorStreamWriter.Instance;
        Out = StandardOutStreamWriter.Instance;
    }

    /// <inheritdoc />
    public IStandardStreamWriter Error { get; }

    /// <inheritdoc />
    // TODO: Can we do this without relying on System.Console?
    public bool IsErrorRedirected => Console.IsErrorRedirected;

    /// <inheritdoc />
    public IStandardStreamWriter Out { get; }

    /// <inheritdoc />
    // TODO: Can we do this without relying on System.Console?
    public bool IsOutputRedirected => Console.IsOutputRedirected;

    /// <inheritdoc />
    public bool IsInputRedirected => Console.IsInputRedirected;

    public void WriteStdoutLine(String s)
    {
        int bytesWritten = Libc.Internal.write(Libc.Internal.STDOUT_FILENO, s.Bytes, (int)s.Length);

        if (bytesWritten < 0)
        {
            // TODO: Might want to silently ignore EPIPE errors and/or handle EAGAIN differently, like
            // TODO: ConsolePal.Unix.cs does it. Some of this is probably easier to handle in C/C++, so it would be a
            // TODO: good fit for a libplatform.so/libsystem.so or similar (written in native code)
            throw new IOException(Marshal.GetLastPInvokeErrorMessage(), Marshal.GetLastPInvokeError());
        }

        bytesWritten = Libc.Internal.write(Libc.Internal.STDOUT_FILENO, String.Newline.Bytes, (int)String.Newline.Length);

        if (bytesWritten < 0)
        {
            // TODO: same as above
            throw new IOException(Marshal.GetLastPInvokeErrorMessage(), Marshal.GetLastPInvokeError());
        }
    }

    private struct StandardOutStreamWriter : IStandardStreamWriter
    {
        public static readonly StandardOutStreamWriter Instance = default;

        public unsafe void Write(string? value)
        {
            if (value == null)
            {
                return;
            }

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(value);

            fixed (byte* ptr = utf8Bytes)
            {
                int bytesWritten = Libc.Internal.write(Libc.Internal.STDOUT_FILENO, (IntPtr)ptr, utf8Bytes.Length);

                if (bytesWritten < 0)
                {
                    // TODO: Might want to silently ignore EPIPE errors and/or handle EAGAIN differently, like
                    // TODO: ConsolePal.Unix.cs does it. Some of this is probably easier to handle in C/C++, so it would be a
                    // TODO: good fit for a libplatform.so/libsystem.so or similar (written in native code)
                    throw new IOException(Marshal.GetLastPInvokeErrorMessage(), Marshal.GetLastPInvokeError());
                }
            }
        }
    }

    private struct StandardErrorStreamWriter : IStandardStreamWriter
    {
        public static readonly StandardErrorStreamWriter Instance = default;

        public unsafe void Write(string? value)
        {
            if (value == null)
            {
                return;
            }

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(value);

            fixed (byte* ptr = utf8Bytes)
            {
                int bytesWritten = Libc.Internal.write(Libc.Internal.STDERR_FILENO, (IntPtr)ptr, utf8Bytes.Length);

                if (bytesWritten < 0)
                {
                    // TODO: Might want to silently ignore EPIPE errors and/or handle EAGAIN differently, like
                    // TODO: ConsolePal.Unix.cs does it. Some of this is probably easier to handle in C/C++, so it would be a
                    // TODO: good fit for a libplatform.so/libsystem.so or similar (written in native code)
                    throw new IOException(Marshal.GetLastPInvokeErrorMessage(), Marshal.GetLastPInvokeError());
                }
            }
        }
    }
}

// Similar to System.CommandLine.IConsole, but takes Perlang strings as parameters.
