// Manual definitions of methods which CppSharp cannot generate interop signatures for automatically

#pragma warning disable S101
#pragma warning disable SA1300
#pragma warning disable SA1307
#pragma warning disable SA1313
#pragma warning disable SA1403
#pragma warning disable SA1601
#pragma warning disable SA1649

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Perlang
{
    namespace Collections
    {
        public partial class StringHashSet
        {
            public IEnumerable<string> Values
            {
                get
                {
                    StringArray.__Internal stringArray = default;

                    try {
                        stringArray = __Internal.ValuesWrapper(__Instance);

                        for (ulong i = 0; i < stringArray.size; i++) {
                            IntPtr ptr = Marshal.ReadIntPtr(stringArray.items, (int)i * IntPtr.Size);
                            yield return Marshal.PtrToStringUTF8(ptr);
                        }
                    }
                    finally {
                        if (stringArray.items != default) {
                            __Internal.DeleteValuesWrapperResult(__Instance, stringArray);
                        }
                    }
                }
            }
        }
    }
}
