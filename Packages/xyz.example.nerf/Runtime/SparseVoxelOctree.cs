using System;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf.WellKnownTypes;

namespace UnityNeRF
{
    public class SparseVoxelOctree
    {
        public static SparseVoxelOctree Load(string path)
        {
            using var stream = File.OpenRead(path);
            return Load(stream);
        }

        public static SparseVoxelOctree Load(Stream stream)
        {
            var parser = Protobuf.SparseVoxelOctree.Parser;
            var message = parser.ParseFrom(stream);

            if (Any.GetTypeName(message.TypeUrl) != FloatValue.Descriptor.FullName)
                throw new NotSupportedException();
            
            var nodeCount = message.NodeChildren.Count / 8;
            int dataDim = message.NodeData.Length / (sizeof (float) * nodeCount);
            
            var octree = new SparseVoxelOctree<float[]>(message.Width, message.Height, message.Depth, dataDim);
            octree._nodeChildren.Clear();
            octree._nodeChildren.AddRange(message.NodeChildren);
            octree._nodeData.Clear();

            for (int i = 0; i < nodeCount; ++i)
            {
                // for (int k = 0; k < length; ++k)
                // {
                //     int offset = sizeof(float) * (length * i + k);
                //     var span = message.NodeData.Span.Slice(offset, sizeof (float));
                //     float value = BitConverter.ToSingle(span);
                //     data[k] = value;
                // }
                
                var data = new float[dataDim];
                int offset = sizeof (float) * dataDim * i;
                var span = message.NodeData.Span.Slice(offset, sizeof (float) * dataDim);
                span.CopyTo(MemoryMarshal.Cast<float, byte>(data.AsSpan()));
                octree._nodeData.Add(data);
            }

            return octree;
        }

        public static SparseVoxelOctree<T> Load<T>(string path)
        {
            return (SparseVoxelOctree<T>) Load(path);
        }

        public static SparseVoxelOctree<T> Load<T>(Stream stream)
        {
            return (SparseVoxelOctree<T>) Load(stream);
        }

        // private static System.Type ParseTypeUrl(string typeUrl, out int size)
        // {
        //     throw new NotImplementedException();
        // }
    }
}
