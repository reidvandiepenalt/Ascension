#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

public class ParallelogramPrimitive
{
#if UNITY_EDITOR
    private static Mesh CreateMesh()
    {
        Vector3[] vertices = {
             new Vector3(0, 0, 0),
             new Vector3(1f, 0f, 0),
             new Vector3(1f, 1f, 0),
             new Vector3(1f, 1f, 0),
             new Vector3(2f, 1f, 0),
             new Vector3(1f, 0, 0)
         };

        Vector2[] uv = {
             new Vector2(0, 0),
             new Vector2(1, 0),
             new Vector2(1, 1),
             new Vector2(1, 1),
             new Vector2(2, 1),
             new Vector2(1, 0)
         };

        int[] triangles = { 0, 1, 2, 3, 4, 5 };

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.name = "Parallelogram";
        AssetDatabase.CreateAsset(mesh, "Assets/Art/Meshes/" + mesh.name + ".asset");
        AssetDatabase.SaveAssets();
        return mesh;
    }

    private static GameObject CreateObject()
    {
        var obj = new GameObject("Parallelogram");
        var mesh = CreateMesh();
        var filter = obj.AddComponent<MeshFilter>();
        var renderer = obj.AddComponent<MeshRenderer>();
        var collider = obj.AddComponent<MeshCollider>();

        filter.sharedMesh = mesh;
        collider.sharedMesh = mesh;
        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

        return obj;
    }

    [MenuItem("GameObject/3D Object/Parallelogram", false, 0)]
    public static void Create()
    {
        CreateObject();
    }
#endif
}
