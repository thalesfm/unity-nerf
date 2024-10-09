using System;
using NUnit.Framework;

namespace UnityNeRF.Editor.Tests
{
    public class N3TreeTests
    {
        [TestCase(@"Packages/xyz.example.nerf/Tests/Editor/Data/svo_mgrid_16.zip")]
        public static void Load_Mgrid(string path)
        {
            N3Tree tree = N3Tree.Load(path);
            int width = (int) Math.Pow(2.0, tree.depth_limit + 1.0);

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    for (int k = 0; k < width; ++k)
                    {
                        float x = (i + 0.5f) / width;
                        float y = (j + 0.5f) / width;
                        float z = (k + 0.5f) / width;

                        Assert.That(tree[x, y, z].GetSingle(0), Is.EqualTo((float) i));
                        Assert.That(tree[x, y, z].GetSingle(1), Is.EqualTo((float) j));
                        Assert.That(tree[x, y, z].GetSingle(2), Is.EqualTo((float) k));
                    }
                }
            }
        }
    }
} // namespace UnityNeRF.Editor.Tests
