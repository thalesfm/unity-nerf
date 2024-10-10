using System;
using Google.Protobuf;
using System.IO;
using System.Runtime.InteropServices;

namespace UnityNeRF
{
    public partial class SparseVoxelOctree<T>
    {
#if UNITY_EDITOR
        public void Save(string path)
        {
            using var stream = File.OpenWrite(path);
            Save(stream);
        }

        public void Save(Stream stream)
        {
            if (!typeof(T).IsArray || typeof(T).GetElementType() != typeof(float))
                throw new NotSupportedException();
            
            // int dataLength = 0;
            // for (int i = 0; i < _nodeData.Count; ++i)
            // {
            //     if (_nodeData[i] != null)
            //     {
            //         dataLength = (_nodeData[i] as float[]).Length;
            //         break;
            //     }
            // }
            
            float[] nodeData = new float[_nodeData.Count * DataDim];

            for (int i = 0; i < _nodeData.Count; ++i)
            {
                if (_nodeData[i] != null)
                {
                    (_nodeData[i] as float[]).CopyTo(nodeData, DataDim * i);
                }
            }

            var message = new Protobuf.SparseVoxelOctree();
            message.TypeUrl = "type.googleapis.com/google.protobuf.FloatValue";
            message.Width = Width;
            message.Height = Height;
            message.Depth = Depth;
            message.NodeChildren.AddRange(_nodeChildren);

            // byte[] bytes = new byte[sizeof(float) * nodeData.Length];
            // Buffer.BlockCopy(nodeData, 0, bytes, 0, bytes.Length);
            // message.NodeData = ByteString.CopyFrom(bytes);

            // message.NodeData = ByteString.CopyFrom(MemoryMarshal.Cast<float, byte>(nodeData.AsSpan()));
            message.NodeData = ByteString.CopyFrom(MemoryMarshal.Cast<float, byte>(nodeData.AsSpan()));

            message.WriteTo(stream);
        }
#endif // UNITY_EDITOR
    }
}
