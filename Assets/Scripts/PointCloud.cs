using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PointCloud : MonoBehaviour
{
    [SerializeField] private TextAsset vertexFile;
    [SerializeField] private Vector3 offset;

    public Mesh mesh;
    public Material material;

    private Vector3[] _vertices;
    private ComputeBuffer _pointBuffer;
    private List<List<Matrix4x4>> _batches = new List<List<Matrix4x4>>();

    private void RenderBatches()
    {
        foreach (var batch in _batches)
        {
            Graphics.DrawMeshInstanced(mesh, 0, material, batch);
        }
    }
    

    private void OnEnable()
    {
        ReadVertexData();
        _pointBuffer = new ComputeBuffer(_vertices.Length, 3 * 4);
        _pointBuffer.SetData(_vertices);
    }

    private void OnDisable()
    {
        _pointBuffer.Release();
        _pointBuffer = null;
    }

    private void Update()
    {
        RenderBatches();
    }

    /// <summary>
    ///     Read vertex-data from file.
    /// </summary>
    private void ReadVertexData()
    {
        // defines which characters to split file into lines on:
        var fileDelimiters = new[] { "\r\n", "\r", "\n" };

        // defines which characters to split each line on:
        var lineDelimiters = new[] { ' ' };

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
            // split line and read coordinates:
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

        // center mesh in world-space
        for (var i = 0; i < vertices.Length; i++) vertices[i] -= offset;

        _vertices = vertices;
    }    
}
