using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PointCloud : MonoBehaviour
{
    public Material material;
        public Mesh mesh;

        GraphicsBuffer commandBuf;
        GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
        const int commandCount = 2;

        void Start()
        {
            commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
        }

        void OnDestroy()
        {
            commandBuf?.Release();
            commandBuf = null;
        }

        void Update()
        {
            RenderParams rp = new RenderParams(material);
            rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds for better FOV culling
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
            commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
            commandData[0].instanceCount = 10;
            commandData[1].indexCountPerInstance = mesh.GetIndexCount(0);
            commandData[1].instanceCount = 10;
            commandBuf.SetData(commandData);
            Graphics.RenderMeshIndirect(rp, mesh, commandBuf, commandCount);
        }
    
    // [SerializeField] private TextAsset vertexFile;
    // [SerializeField] private Vector3 offset;
    //
    // public Mesh mesh;
    // public Material material;
    //
    // private Vector3[] _vertices;
    // private ComputeBuffer _pointBuffer;
    // private List<List<Matrix4x4>> _batches = new List<List<Matrix4x4>>();
    //
    // private void RenderBatches()
    // {
    //     foreach (var batch in _batches)
    //     {
    //         Graphics.DrawMeshInstanced(mesh, 0, material, batch);
    //     }
    // }
    //
    //
    // private void OnEnable()
    // {
    //     ReadVertexData();
    //     _pointBuffer = new ComputeBuffer(_vertices.Length, 3 * 4);
    //     _pointBuffer.SetData(_vertices);
    // }
    //
    // private void OnDisable()
    // {
    //     _pointBuffer.Release();
    //     _pointBuffer = null;
    // }
    //
    // private void Update()
    // {
    //     RenderBatches();
    // }
    //
    // /// <summary>
    // ///     Read vertex-data from file.
    // /// </summary>
    // private void ReadVertexData()
    // {
    //     // defines which characters to split file into lines on:
    //     var fileDelimiters = new[] { "\r\n", "\r", "\n" };
    //
    //     // defines which characters to split each line on:
    //     var lineDelimiters = new[] { ' ' };
    //
    //     // split file into array of non-empty lines:
    //     var lines = vertexFile.text.Split(fileDelimiters, StringSplitOptions.RemoveEmptyEntries);
    //
    //     if (lines.Length < 1)
    //     {
    //         Debug.LogWarning($"{vertexFile.name} was empty!");
    //         return;
    //     }
    //
    //     var numVertices = int.Parse(lines[0]);
    //
    //     if (numVertices < 1)
    //     {
    //         Debug.LogWarning($"{vertexFile.name} contains no vertex data!");
    //         return;
    //     }
    //
    //     var vertices = new Vector3[numVertices];
    //
    //     for (var i = 1; i <= numVertices; i++)
    //     {
    //         // split line and read coordinates:
    //         var elements = lines[i].Split(lineDelimiters, StringSplitOptions.RemoveEmptyEntries);
    //         if (elements.Length < 3)
    //         {
    //             Debug.LogWarning($"{vertexFile.name} is missing data on line {i}");
    //             continue;
    //         }
    //
    //         vertices[i - 1] = new Vector3(
    //             float.Parse(elements[0], CultureInfo.InvariantCulture),
    //             float.Parse(elements[1], CultureInfo.InvariantCulture),
    //             float.Parse(elements[2], CultureInfo.InvariantCulture)
    //         );
    //     }
    //
    //     // center mesh in world-space
    //     for (var i = 0; i < vertices.Length; i++) vertices[i] -= offset;
    //
    //     _vertices = vertices;
    // }
}
