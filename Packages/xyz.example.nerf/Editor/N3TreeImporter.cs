using UnityEngine;
using UnityEditor.AssetImporters;
using System;

namespace UnityNeRF.Editor
{
    [ScriptedImporter(version: 1, ext: "npz")]
    public class N3TreeImporter : ScriptedImporter
    {
        private const string MaterialPath = "Packages/xyz.example.nerf/Materials/UnlitSparseVoxelOctree.mat";

        public int MaxLevel = 10;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = System.IO.Path.ChangeExtension(ctx.assetPath, "bin");
            N3Tree tree = N3Tree.Load(ctx.assetPath);
            SparseVoxelOctree<float[]> octree = Convert.ToSparseVoxelOctree(tree, MaxLevel);
            octree.Save(path);

            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // TODO: Scale prefab based on `tree.invradius`
            
            MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
            Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Material materialCopy = new Material(material);
            meshRenderer.material = materialCopy;

            SparseVoxelOctreeRenderer volumeRenderer = prefab.AddComponent<SparseVoxelOctreeRenderer>();
            volumeRenderer._fileName = path;

            ctx.AddObjectToAsset("prefab", prefab);
            ctx.AddObjectToAsset("material", materialCopy);
            ctx.SetMainObject(prefab);

            // DestroyImmediate(material);
        }
    }
}