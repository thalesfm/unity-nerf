using UnityEngine;
using UnityEditor.AssetImporters;
using System;

namespace UnityNeRF.Editor
{
    [ScriptedImporter(version: 1, ext: "npz")]
    public class PlenOctreeImporter : ScriptedImporter
    {
        private const string MaterialPath = "Packages/xyz.example.nerf/Materials/UnlitSparseVoxelOctree.mat";

        public int MaxLevel = 10;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            throw new Exception(); // Currently pretty slow; disabled for now

            var path = System.IO.Path.ChangeExtension(ctx.assetPath, "bin");
            var tree = PlenOctree.N3Tree.Load(ctx.assetPath);
            var octree = Convert.ToSparseArray3D<float[]>(tree, MaxLevel);
            octree.Save(path);

            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // TODO: Scale prefab based on `tree.invradius`
            
            MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
            Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Material materialCopy = new Material(material);
            meshRenderer.material = materialCopy;

            RadianceFieldRenderer volumeRenderer = prefab.AddComponent<RadianceFieldRenderer>();
            volumeRenderer._fileName = path;

            ctx.AddObjectToAsset("prefab", prefab);
            ctx.AddObjectToAsset("material", materialCopy);
            ctx.SetMainObject(prefab);
        }
    }
}
