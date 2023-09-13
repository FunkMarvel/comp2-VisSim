// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////
// //FileName: TriangleSurface.cs
// //FileType: Visual C# Source file
// //Author : Anders P. Åsbø
// //Created On : 12/09/2023
// //Last Modified On : 13/09/2023
// //Copy Rights : Anders P. Åsbø
// //Description :
// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public struct TriangleData
{
    public TriangleData(int v0, int v1, int v2, int T0, int T1, int T2)
    {
        Indices = new[] { v0, v1, v2 };
        Neighbours = new[] { T0, T1, T2 };
    }

    public int[] Indices { get; }
    public int[] Neighbours { get; }
}

public struct Contact
{
    public Contact(Vector3 point, Vector3 hitNormal)
    {
        Point = point;
        HitNormal = hitNormal;
    }

    public Vector3 Point { get; }
    public Vector3 HitNormal { get; }
}

// [ExecuteAlways]
public class TriangleSurface : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private TextAsset vertexFile;
    [SerializeField] private TextAsset indexFile;
    [SerializeField] private Material material;

    private bool _hasMesh;

    public Vector3[] Vertices { get; private set; } // property with public getter and private setter.
    public List<TriangleData> Triangles { get; private set; } // property with public getter and private setter.

    private void Awake()
    {
        if(!_hasMesh) CreateSurface();
        _hasMesh = true;
    }

    public Contact GetContact(Vector3 center, TriangleData prevTriangle)
    {
        Vector3 p = Vertices[prevTriangle.Indices[0]],
            q = Vertices[prevTriangle.Indices[1]],
            r = Vertices[prevTriangle.Indices[2]];

        Vector3 uvw = GetBarycentricCoordinates(center, p, q, r);
        if (uvw.x < 0 || uvw.y < 0 || uvw.z < 0)
        {
            int opposingIndex = -1;
            
            if (uvw.x <= uvw.y && uvw.x <= uvw.z)
            {
                opposingIndex = 0;
            }
            else if (uvw.y <= uvw.z)
            {
                opposingIndex = 1;
            }
            else
            {
                opposingIndex = 2;
            }

            if (prevTriangle.Neighbours[opposingIndex] >= 0)
                return GetContact(center, Triangles[prevTriangle.Neighbours[opposingIndex]]);
            
            Debug.LogWarning("Warning, contact point out of bounds!");
            return new Contact(Vector3.zero, Vector3.zero);
        }

        Vector3 hit = uvw.x * p + uvw.y * q + uvw.z * r;

        return new Contact(hit, GetNormalFromTri(prevTriangle));
    }

    private Vector3 GetNormalFromTri(TriangleData currentTriangle)
    {
        return Vector3.Cross(Vertices[currentTriangle.Indices[1]] - Vertices[currentTriangle.Indices[0]],
            Vertices[currentTriangle.Indices[2]] - Vertices[currentTriangle.Indices[0]]).normalized;
    }

    public static Vector3 GetBarycentricCoordinates(Vector3 x, Vector3 p, Vector3 q, Vector3 r)
    {
        var uvw = Vector3.zero;
        
        Vector3 pq = q - p, pr = r - p, px = x - p;
        
        float determinant = pq.x * pr.z - pr.x * pq.z;
        uvw.y = (px.x * pr.z - pr.x * px.z) / determinant;
        uvw.z = (pq.x * px.z - px.x * pq.z) / determinant;
        uvw.x = 1.0f - uvw.y - uvw.z;

        return uvw;
    }

    [ContextMenu("Create Surface")]
    private void CreateSurface()
    {
        if (vertexFile == null || indexFile == null)
        {
            Debug.LogWarning("vertexFile or indexFile is null!");
            return;
        }

        var filter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        filter.sharedMesh = GenerateMesh();
        
        // use chosen material, or default material if nothing is chosen.
        meshRenderer.sharedMaterial = material != null ? material : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        _hasMesh = true;
    }

    private void ReadVertexData()
    {
        // defines which characters to split file into lines on:
        var fileDelimiters = new[] { "\r\n", "\r", "\n" };

        // defines which characters to split each line on:
        var lineDelimiters = new[] { '(', ')', ',' };

        // split file into array of non-empty lines:
        var lines = vertexFile.text.Split(fileDelimiters, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 1)
        {
            Debug.LogWarning($"{vertexFile.name} was empty!");
            return;
        }

        var numVertices = int.Parse(lines[0]);

        if (numVertices < 1)
        {
            Debug.LogWarning($"{vertexFile.name} contains no vertex data!");
            return;
        }

        var vertices = new Vector3[numVertices];

        for (var i = 1; i <= numVertices; i++)
        {
            var elements = lines[i].Split(lineDelimiters, StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length < 3)
            {
                Debug.LogWarning($"{vertexFile.name} is missing data on line {i}");
                continue;
            }

            var position = transform.position;
            
            vertices[i - 1] = new Vector3(
                float.Parse(elements[0], CultureInfo.InvariantCulture),
                float.Parse(elements[1], CultureInfo.InvariantCulture),
                float.Parse(elements[2], CultureInfo.InvariantCulture)
            );
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= offset;
        }

        Vertices = vertices;
    }

    private void ReadIndexData()
    {
        Triangles = new List<TriangleData>();

        // defines which characters to split file into lines on:
        var fileDelimiters = new[] { "\r\n", "\r", "\n" };

        // defines which characters to split each line on:
        var lineDelimiters = new[] { ' ' };

        // split file into array of non-empty lines:
        var lines = indexFile.text.Split(fileDelimiters, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 1)
        {
            Debug.LogWarning($"{indexFile.name} is empty!");
            return;
        }

        var numTriangles = int.Parse(lines[0]);

        if (numTriangles < 1)
        {
            Debug.LogWarning($"{indexFile.name} contains no triangle data");
            return;
        }

        for (var i = 1; i <= numTriangles; i++)
        {
            var elements = lines[i].Split(lineDelimiters, StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length < 6)
            {
                Debug.LogWarning($"{vertexFile.name} is missing data on line {i}");
                continue;
            }

            Triangles.Add(new TriangleData(
                int.Parse(elements[0]),
                int.Parse(elements[1]),
                int.Parse(elements[2]),
                int.Parse(elements[3]),
                int.Parse(elements[4]),
                int.Parse(elements[5])
            ));
        }
    }

    private int[] GenerateIndexArray()
    {
        var indices = new int[Triangles.Count * 3];

        for (var i = 0; i < Triangles.Count; i++)
        for (var j = 0; j < 3; j++)
            indices[3 * i + j] = Triangles[i].Indices[j];

        return indices;
    }

    private Mesh GenerateMesh()
    {
        ReadVertexData();
        ReadIndexData();

        var newMesh = new Mesh
        {
            vertices = Vertices,
            triangles = GenerateIndexArray()
        };

        // newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        // var bounds = newMesh.bounds;
        // transform.position -= bounds.center;

        return newMesh;
    }
}