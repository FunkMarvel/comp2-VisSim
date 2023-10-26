using System;
using System.Globalization;
using UnityEngine;

public class PointCloud : MonoBehaviour
{
    [SerializeField] private TextAsset vertexFile;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 scale = Vector3.one;
    
    private Vector3[] _vertices = {new Vector3(0,0,0), new Vector3(1, 1, 1)};
    private ComputeBuffer _pointBuffer;
    
    private const int CommandCount = 1;
    public Material material;
    public Mesh mesh;

    private GraphicsBuffer _commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
    private static readonly int ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");
    private static readonly int PositionUniform = Shader.PropertyToID("_positions");

    private void Start()
    {
        ReadVertexData();
        Debug.LogWarning(_vertices.Length);
        
        _commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount,
            GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];
        _pointBuffer = new ComputeBuffer(_vertices.Length, 4 * 3);
        _pointBuffer.SetData(_vertices);
    }

    private void Update()
    {
        var rp = new RenderParams(material)
        {
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one), // use tighter bounds for better FOV culling
            matProps = new MaterialPropertyBlock()
        };
        var uniformMat = Matrix4x4.identity;
        uniformMat.SetTRS(-offset, Quaternion.identity, scale);
        
        rp.matProps.SetMatrix(ObjectToWorld, uniformMat);
        rp.matProps.SetBuffer(PositionUniform, _pointBuffer);
        _commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        _commandData[0].instanceCount = (uint)_vertices.Length;
        _commandBuf.SetData(_commandData);
        Graphics.RenderMeshIndirect(rp, mesh, _commandBuf, CommandCount);
    }

    private void OnDestroy()
    {
        _commandBuf?.Release();
        _commandBuf = null;
    }

    private void ReadVertexData()
    {
        // defines which characters to split file into lines on:
        var fileDelimiters = new[] { "\r\n", "\r", "\n" };
    
        // defines which characters to split each line on:
        var lineDelimiters = new[] { '(',')',',' };
    
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
    
        _vertices = vertices;
        Debug.LogWarning(_vertices[0]);
    }

    //
    // public Mesh mesh;
    // public Material material;
    //

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

}