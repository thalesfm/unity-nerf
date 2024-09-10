using System;
using System.IO;

// namespace NET_7_0_Compat
// {
// public static class StreamExtensions
// {
//     public static void ReadExactly(this Stream stream, Span<byte> buffer)
//     {
//         if (buffer.Length > 0)
//         {
//             stream.ReadExactlyUnchecked(buffer);
//         }
//     }
//     private static void ReadExactlyUnchecked(this Stream stream, Span<byte> buffer)
//     {
//         int read = stream.Read(buffer);
//         if (read == 0) throw new EndOfStreamException();
//         if (read < buffer.Length) stream.ReadExactlyUnchecked(buffer.Slice(read));
//     }
// }
// } // namespace StreamCompatExtensions