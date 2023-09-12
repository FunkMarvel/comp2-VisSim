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
using UnityEngine;

public struct TriangleData
{
    public TriangleData(int v0, int v1, int v2, int T0, int T1, int T2)
    {
        Indices = new[] { v0, v1, v2 };
        Neighbours = new[] { T0, T1, T2 };
    }

    public int[] Indices { get; private set; }
    public int[] Neighbours { get; private set; }
}

public class TriangleSurface : MonoBehaviour
{
    [SerializeField] private TextAsset vertexFile;
    [SerializeField] private TextAsset indexFile;
    [SerializeField] private Material material;

    public Vector3[] Vertices { get; private set; } // property with public getter and private setter.
    public List<TriangleData> Triangles { get; private set; } // property with public getter and private setter.

    private void Awake()
    {
        if (vertexFile == null || indexFile == null)
        {
            Debug.LogWarning("vertexFile or indexFile is null!");
            return;
        }

        var filter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        filter.sharedMesh = GenerateMesh();
        if (material != null) meshRenderer.sharedMaterial = material;
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

            vertices[i - 1] = new Vector3(
                float.Parse(elements[0], CultureInfo.InvariantCulture),
                float.Parse(elements[1], CultureInfo.InvariantCulture),
                float.Parse(elements[2], CultureInfo.InvariantCulture)
            );
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

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        return newMesh;
    }
}