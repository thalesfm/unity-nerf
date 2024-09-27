using System;
using NUnit.Framework;
using NumSharp;
using UnityNeRF.Editor.IO;

namespace UnityNeRF.Editor.Tests
{
    public class TestN3Tree
    {
        [TestCase(@"Assets/Resources/oct_lego.npz")]
        public static void Load_NoThrow(string path)
        {
            N3Tree tree = N3Tree.Load(path);
        }
    }
} // namespace UnityNeRF.Editor.Tests