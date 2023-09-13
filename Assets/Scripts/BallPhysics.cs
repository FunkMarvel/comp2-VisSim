// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////
// //FileName: BallPhysics.cs
// //FileType: Visual C# Source file
// //Author : Anders P. Åsbø
// //Created On : 13/09/2023
// //Last Modified On : 13/09/2023
// //Copy Rights : Anders P. Åsbø
// //Description :
// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////

using System;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [SerializeField] private TriangleSurface triangleSurface;
    private Vector3 _velocity = Vector3.zero;
    private TriangleData _currentTriangle;
    private bool _hasSurfaceRef;

    private void Awake()
    {
        _hasSurfaceRef = triangleSurface != null;

        if (_hasSurfaceRef)
        {
            
        }
    }
}