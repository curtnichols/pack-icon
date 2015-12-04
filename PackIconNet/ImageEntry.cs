using System;
using System.Runtime.InteropServices;

namespace PackIconNet
{
    // Note that field values are little-endian.

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ImageEntry
    {
        public byte imageWidth;  // 0 => 256
        public byte imageHeight; // 0 => 256
        public byte palleteSize; // 0 => no palette
        public byte reserved;    // zero
        public UInt16 colorPlanes;
        public UInt16 bitsPerPixel;
        public UInt32 imageLength;
        public UInt32 offsetFromBoF;
    }
}
