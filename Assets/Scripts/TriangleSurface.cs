// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////
// //FileName: TriangleSurface.cs
// //FileType: Visual C# Source file
// //Author : Anders P. Åsbø
// //Created On : 12/09/2023
// //Last Modified On : 14/09/2023
// //Copy Rights : Anders P. Åsbø
// //Description :
// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
///     Struct containing read-only triangulation data for single triangle.
/// </summary>
public struct TriangleData
{
    public TriangleData(int v0, int v1, int v2, int triangle0, int triangle1, int triangle2)
    {
        Indices = new[] { v0, v1, v2 };
        Neighbours = new[] { triangle0, triangle1, triangle2 };
    }

    /// <summary>
    ///     Vertex indices of triangle.
    /// </summary>
    public int[] Indices { get; }

    /// <summary>
    ///     Indices of neighbouring triangles.
    /// </summary>
    public int[] Neighbours { get; }
}

/// <summary>
///     Struct containing read-only data for collision contacts.
/// </summary>
public struct Contact
{
    public Contact(Vector3 point, Vector3 hitNormal)
    {
        Point = point;
        HitNormal = hitNormal;
    }

    /// <summary>
    ///     Location of collision contact in world-space.
    /// </summary>
    public Vector3 Point { get; }

    /// <summary>
    ///     Unit normal at contact-point.
    /// </summary>
    public Vector3 HitNormal { get; }
}


/// <summary>
///     Class for creating triangle-surface from data files.
/// </summary>
public class TriangleSurface : MonoBehaviour
{
    // properties that are set in editor:
    [SerializeField] private Vector3 offset; // object mesh offset for proper centering in world.
    [SerializeField] private TextAsset vertexFile; // reference to text-file with vertices.
    [SerializeField] private TextAsset indexFile; // reference to text-file with triangulation-data.
    [SerializeField] private Material material; // reference to Unity-material to color mesh with.
    private TriangleData _currentTriangle; // for keeping track of ball.

    // for checking if mesh has been generated.
    private bool _hasMesh;

    /// <summary>
    ///     Array of vertices.
    /// </summary>
    public Vector3[] Vertices { get; private set; } // property with public getter and private setter.

    /// <summary>
    ///     Dynamic array with triangulation data.
    /// </summary>
    public List<TriangleData> Triangles { get; private set; } // property with public getter and private setter.

    private void Awake()
    {
        // run after every object in scene is created, but before first frame.

        if (!_hasMesh)
        {
            CreateSurface(); // create mesh if necessary.
            _hasMesh = true;
        }

        _currentTriangle = Triangles[0];
    }

    private void OnDrawGizmos()
    {

        if (!_hasMesh)
        {
            ReadVertexData();
            ReadIndexData();
        }

        foreach (var triangle in Triangles)
        {
            Gizmos.DrawLine(Vertices[triangle.Indices[0]], Vertices[triangle.Indices[1]]);
            Gizmos.DrawLine(Vertices[triangle.Indices[2]], Vertices[triangle.Indices[1]]);
            Gizmos.DrawLine(Vertices[triangle.Indices[0]], Vertices[triangle.Indices[2]]);
        }
    }

    /// <summary>
    ///     Project position onto surface along vertical axis (y-axis).
    /// </summary>
    /// <param name="position">Position to find contact of.</param>
    /// <returns></returns>
    public Contact ProjectOntoSurface(Vector3 position)
    {
        while (true)
        {
            Vector3 p = Vertices[_currentTriangle.Indices[0]],
                q = Vertices[_currentTriangle.Indices[1]],
                r = Vertices[_currentTriangle.Indices[2]];

            var uvw = GetBarycentricCoordinates(position, p, q, r);
            if (uvw is { x: >= 0, y: >= 0, z: >= 0 })
            {
                var hit = uvw.x * p + uvw.y * q + uvw.z * r;

                return new Contact(hit, GetNormalFromTri(_currentTriangle));
            }

            int opposingIndex;

            if (uvw.x <= uvw.y && uvw.x <= uvw.z)
                opposingIndex = 0;
            else if (uvw.y <= uvw.z)
                opposingIndex = 1;
            else
                opposingIndex = 2;

            if (_currentTriangle.Neighbours[opposingIndex] >= 0)
            {
                _currentTriangle = Triangles[_currentTriangle.Neighbours[opposingIndex]];
                
                continue;
            }

            Debug.LogWarning("Warning, contact point out of bounds!");
            return new Contact(Vector3.zero, Vector3.zero);
        }
    }

    private Vector3 GetNormalFromTri(TriangleData currentTriangle)
    {
        return Vector3.Cross(Vertices[currentTriangle.Indices[1]] - Vertices[currentTriangle.Indices[0]],
            Vertices[currentTriangle.Indices[2]] - Vertices[currentTriangle.Indices[0]]).normalized;
    }

    private static Vector3 GetBarycentricCoordinates(Vector3 x, Vector3 p, Vector3 q, Vector3 r)
    {
        var uvw = Vector3.zero;

        Vector3 pq = q - p, pr = r - p, px = x - p;

        var determinant = pq.x * pr.z - pr.x * pq.z;
        uvw.y = (px.x * pr.z - pr.x * px.z) / determinant;
        uvw.z = (pq.x * px.z - px.x * pq.z) / determinant;
        uvw.x = 1.0f - uvw.y - uvw.z;

        return uvw;
    }

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
        meshRenderer.sharedMaterial = material != null
            ? material
            : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

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

            vertices[i - 1] = new Vector3(
                float.Parse(elements[0], CultureInfo.InvariantCulture),
                float.Parse(elements[1], CultureInfo.InvariantCulture),
                float.Parse(elements[2], CultureInfo.InvariantCulture)
            );
        }

        for (var i = 0; i < vertices.Length; i++) vertices[i] -= offset;

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

        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        return newMesh;
    }
}