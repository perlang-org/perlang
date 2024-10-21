#nullable enable
#pragma warning disable SA1300
#pragma warning disable SA1601
using System;
using System.Runtime.InteropServices;

namespace Perlang.Native;

internal static partial class NativeFile
{
    [LibraryImport("perlang_cli", EntryPoint = "File_read_all_text", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr _File_read_all_text(string path);

    [LibraryImport("perlang_cli", EntryPoint = "File_read_all_text_free")]
    private static partial void _File_read_all_text_free(IntPtr file_contents);

    public static string read_all_text(string path)
    {
        IntPtr file_contents = IntPtr.Zero;

        try {
            file_contents = _File_read_all_text(path);
            return Marshal.PtrToStringUTF8(file_contents)!;
        }
        finally {
            if (file_contents != IntPtr.Zero) {
                _File_read_all_text_free(file_contents);
            }
        }
    }
}
