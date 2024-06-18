using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Perlang.Internal;

/// <summary>
/// Very simple memory allocator.
/// </summary>
internal static class MemoryAllocator
{
    private static readonly ConcurrentBag<IntPtr> AllocatedChunks = new();

    static MemoryAllocator()
    {
        AppDomain.CurrentDomain.ProcessExit += FreeAllocatedMemory;
    }

    public static unsafe void* Allocate(nuint count)
    {
        // TODO: Use jemalloc instead. Requires creating a NuGet package of it for easiest consumption from this project.
        // TODO: See https://github.com/dotnet/runtime/issues/11404 and https://github.com/allisterb/jemalloc.NET for
        // TODO: some prior art.

        // We keep a local track of all memory allocated using this allocator, for a poor-man's "garbage collection"
        // when the process exits. In the future we may attempt to do better: #378
        void* result = NativeMemory.Alloc(count);
        AllocatedChunks.Add((IntPtr)result);
        return result;
    }

    private static unsafe void FreeAllocatedMemory(object sender, EventArgs e)
    {
        foreach (IntPtr allocatedChunk in AllocatedChunks)
        {
            var allocatedChunkPtr = (void*)allocatedChunk;
            NativeMemory.Free(allocatedChunkPtr);
        }
    }
}
