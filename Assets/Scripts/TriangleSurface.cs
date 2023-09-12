// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////
// //FileName: TriangleSurface.cs
// //FileType: Visual C# Source file
// //Author : Anders P. Åsbø
// //Created On : 12/09/2023
// //Last Modified On : 12/09/2023
// //Copy Rights : Anders P. Åsbø
// //Description :
// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////

using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class TriangleSurface : MonoBehaviour
{
    [SerializeField] private TextAsset vertexFile;
    [SerializeField] private TextAsset indexFile;

    private Vector3[] ReadVertexData()
    {
        // defines which characters to split file into lines on:
        var splitLines = new string[] { "\r\n", "\r", "\n" };
        
        // defines which characters to split each line on:
        var splitLine = new char[] { '(', ')', ',' };

        // split file into array of non-empty lines:
        string[] lines = vertexFile.text.Split(splitLines, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 1) return new Vector3[] {};
        
        var numVertices = int.Parse(lines[0]);
        
        if (numVertices < 1) return new Vector3[] {};

        var vertices = new Vector3[] { };

        foreach (var line in lines)
        {
            var elements = line.Split(splitLine, StringSplitOptions.RemoveEmptyEntries);
            Debug.Log(elements);
        }

        return new Vector3[] { };
    }
    
    private void Awake()
    {
        if (vertexFile != null) ReadVertexData();
    }
}