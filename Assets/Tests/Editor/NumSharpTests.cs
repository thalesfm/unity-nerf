
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sprache;
using NumSharp;

public class NumSharpTests
{
    [TestCase(@"Assets/Resources/oct_lego.npz")]
    public static void Test(string npzPath)
    {
        using (NpzDictionary<float[,,,]> npz = np.Load_Npz<float[,,,]>(npzPath))
        {
            float[,,,] data = npz["data.npy"];
            float[,,,] child = npz["child.npy"];
        }

        using (NpzDictionary<Array> npz = np.LoadMatrix_Npz(npzPath))
        {
            var dataDim = npz["data_dim.npy"];
        }
    }
}