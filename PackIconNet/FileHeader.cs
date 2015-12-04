using System;
using System.Runtime.InteropServices;

namespace PackIconNet
{
    // Note that field values are little-endian.

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileHeader
    {
        public UInt16 reserved; // must be zero
        public UInt16 imageType;
        public UInt16 imageCount;
    }
}
