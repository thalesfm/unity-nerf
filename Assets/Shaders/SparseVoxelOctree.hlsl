#ifndef SPARSE_VOXEL_OCTREE_INCLUDED
#define SPARSE_VOXEL_OCTREE_INCLUDED

struct SparseVoxelOctree {
    int width;
    int height;
    int depth;
    int dataDim;
    int maxLevel;
    StructuredBuffer<int> nodeChildren;
    StructuredBuffer<float> nodeData;
};

int SVOGetNodeIndex(SparseVoxelOctree svo, int x, int y, int z, int level = 0)
{
    int nodeIndex = 0;

    for (int k = svo.maxLevel - 1; k >= level; --k) {
        bool qx = (x & (1 << k)) != 0;
        bool qy = (y & (1 << k)) != 0;
        bool qz = (z & (1 << k)) != 0;

        int quadrant = 0;
        quadrant += qx ? 0 : 1;
        quadrant += qy ? 0 : 2;
        quadrant += qz ? 0 : 4;

        int childIndex = svo.nodeChildren[8 * nodeIndex + quadrant];
        if (childIndex == -1) {
            return -1;
        }

        nodeIndex = childIndex;
    }

    return nodeIndex;
}

int SVOGetNodeIndexAt(SparseVoxelOctree svo, float3 position, int level = 0)
{
    if (position.x <= -1.0 || 1.0 <= position.x ||
        position.y <= -1.0 || 1.0 <= position.y ||
        position.z <= -1.0 || 1.0 <= position.z) {
        return -1;
    }

    int x = svo.width  * (position.x + 1.0) / 2.0;
    int y = svo.height * (position.y + 1.0) / 2.0;
    int z = svo.depth  * (position.z + 1.0) / 2.0;
    int nodeIndex = SVOGetNodeIndex(svo, x, y, z);

    return nodeIndex;
}

float SVOGetNodeData(SparseVoxelOctree svo, int nodeIndex, int offset = 0)
{
    if (nodeIndex == -1) {
        return 0.0;
    }
    return svo.nodeData[nodeIndex*svo.dataDim + offset];
}

#endif