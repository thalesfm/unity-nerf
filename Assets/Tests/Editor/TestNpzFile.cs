using System;
using NUnit.Framework;
using NumSharp;
using UnityNeRF.Editor.IO;

namespace UnityNeRF.Editor.Tests
{
    public class TestNpzFile
    {
        [TestCase(@"Assets/Resources/oct_lego.npz")]
        public static void OpenRead_NoThrow(string path)
        {
            using NpzFile npz = NpzFile.OpenRead(path);

            foreach (var entry in npz.Entries)
            {
                Array array = entry.Value.ReadArray();
                Assert.NotNull(array);
            }
        }
    }
} // namespace UnityNeRF.Editor.Tests