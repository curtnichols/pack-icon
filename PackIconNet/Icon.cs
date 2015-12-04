using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PackIconNet
{
    static class Icon
    {
        public static void PackIcon(string outputPath, string[] inputs)
        {
            bool success = false;
            var inputStreams = inputs.Select(path => File.OpenRead(path)).ToArray();
            var outputStream = File.OpenWrite(outputPath);

            try
            {
                WriteFileHeader(outputStream, inputStreams.Length);

                // Calculate image entries
                var imageEntries = inputStreams.Select(stream => MakeImageEntry(stream)).ToArray();

                var firstImageOffset = Marshal.SizeOf(typeof(FileHeader))
                                        + (imageEntries.Length * Marshal.SizeOf(typeof(ImageEntry)));
                var imageLengths = imageEntries.Select(entry => (int)entry.imageLength).ToArray();
                var imageOffsets = imageLengths.Aggregate(new List<int> { firstImageOffset },
                                            (acc, length) =>
                                                {
                                                    acc.Add(acc.Last() + length);
                                                    return acc;
                                                })
                                            .ToArray();

                // Update image entries with image offsets
                imageEntries = imageEntries.Zip(imageOffsets,
                                        (entry, offset) =>
                                        {
                                            var newEntry = entry;
                                            newEntry.offsetFromBoF = (UInt16)offset;
                                            return newEntry;
                                        })
                                    .ToArray();

                foreach (var entry in imageEntries)
                {
                    WriteImageEntry(outputStream, entry);
                }

                for (int i = 0; i < inputStreams.Length; ++i)
                {
                    var input = inputStreams[i];
                    input.Position = 0;
                    input.CopyTo(outputStream);
                }

                success = true;
            }
            finally
            {
                outputStream.Close();

                foreach (var input in inputStreams)
                {
                    input.Close();
                }

                if (!success)
                {
                    File.Delete(outputPath);
                }
            }
        }

        private static Stream[] OpenInputs(string[] inputs)
        {
            return inputs.Select(path => File.OpenRead(path)).ToArray();
        }

        private static void WriteFileHeader(Stream output, int imageCount)
        {
            var bufSize = Marshal.SizeOf(typeof(FileHeader));
            var buf = new byte[bufSize];
            var native = Marshal.AllocHGlobal(bufSize);
            try
            {
                var hdr = new FileHeader();
                hdr.reserved = 0;
                hdr.imageType = 1;
                hdr.imageCount = (UInt16)imageCount;

                Marshal.StructureToPtr(hdr, native, false);
                Marshal.Copy(native, buf, 0, buf.Length);

                output.Write(buf, 0, buf.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(native);
            }
        }

        private static ImageEntry MakeImageEntry(Stream input)
        {
            using (var bitmap = (Bitmap)Bitmap.FromStream(input))
            {
                var entry = new ImageEntry();
                entry.imageWidth = (byte)(bitmap.Width == 256 ? 0 : bitmap.Width);
                entry.imageHeight = (byte)(bitmap.Height == 256 ? 0 : bitmap.Height);
                entry.palleteSize = 0;
                entry.reserved = 0;
                entry.colorPlanes = 1;
                entry.bitsPerPixel = (UInt16)Image.GetPixelFormatSize(bitmap.PixelFormat);
                entry.imageLength = (UInt32)input.Length;
                entry.offsetFromBoF = 0; // to be set elsewhere

                return entry;
            }
        }

        private static void WriteImageEntry(Stream output, ImageEntry entry)
        {
            var bufSize = Marshal.SizeOf(typeof(ImageEntry));
            var buf = new byte[bufSize];
            var native = Marshal.AllocHGlobal(bufSize);
            try
            {
                Marshal.StructureToPtr(entry, native, false);
                Marshal.Copy(native, buf, 0, buf.Length);

                output.Write(buf, 0, buf.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(native);
            }
        }
    }
}
