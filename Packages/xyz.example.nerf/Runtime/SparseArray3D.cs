using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;

namespace UnityNeRF
{
    [Serializable]
    public class SparseArray3D
    {
        public static SparseArray3D<T> Load<T>(string path)
        {
            using var stream = File.OpenRead(path);
            return Load<T>(stream);
        }

        public static SparseArray3D<T> Load<T>(Stream stream)
        {
            // WARNING: Unsafe! Replace ASAP
            return (SparseArray3D<T>) new BinaryFormatter().Deserialize(stream);
        }
    }
}
