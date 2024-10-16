using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace UnityNeRF.Editor
{
    [ScriptedImporter(version: 1, ext: "npz")]
    public class PlenOctreeImporter : ScriptedImporter
    {
        private const string MaterialPath = "Packages/xyz.example.nerf/Materials/UnlitSparseVoxelOctree.mat";

        public int MaxLevel = 10;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var tree = N3Tree.Load(ctx.assetPath);

            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // TODO: Scale prefab based on `tree.invradius`
            
            MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Material materialCopy = new Material(material);
            meshRenderer.material = materialCopy;

            var plenoctree = ScriptableObject.CreateInstance<PlenOctree>();
            plenoctree.array = Convert.ToSparseArray3D<float[]>(tree, MaxLevel);
            plenoctree.format = tree.data_format;

            // var path = System.IO.Path.ChangeExtension(ctx.assetPath, "asset");
            // AssetDatabase.CreateAsset(plenoctree, path);
            // AssetDatabase.SaveAssets();

            prefab.SetActive(false); // Prevent `Awake`, `OnEnable` from being invoked
            PlenOctreeRenderer renderer = prefab.AddComponent<PlenOctreeRenderer>();
            renderer._plenoctree = plenoctree;
            prefab.SetActive(true);

            ctx.AddObjectToAsset("prefab", prefab);
            ctx.AddObjectToAsset("plenoctree", plenoctree);
            ctx.AddObjectToAsset("material", materialCopy);
            ctx.SetMainObject(prefab);
        }
    }
}
