using System;
using System.Collections;
using System.Collections.Generic;

namespace SVO.Protobuf {

public static class SparseVoxelOctreeExtensions
{
    public static Type GetType(this SparseVoxelOctree message)
    {
        switch (message.TypeUrl)
        {
            case "type.googleapis.com/google.protobuf.FloatValue":
                return typeof(float);
            case "type.googleapis.com/google.protobuf.DoubleValue":
                return typeof(double);
            default:
                throw new Exception("unknown type");
        }
    }
}

} // namespace SVO.Protobuf