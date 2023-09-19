// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////
// //FileName: BallPhysics.cs
// //FileType: Visual C# Source file
// //Author : Anders P. Åsbø
// //Created On : 14/09/2023
// //Last Modified On : 14/09/2023
// //Copy Rights : Anders P. Åsbø
// //Description :
// //////////////////////////////////////////////////////////////////////////
// //////////////////////////////

using System;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject triangleSurfaceRef;

    [Header("Physical properties")] [SerializeField] [Min(1e-6f)]
    private float mass = 1;

    [SerializeField] [Min(0)] private float radius = 1;
    [SerializeField] [Min(0)] private float rollingResistance;
    [SerializeField] [Range(0, 1)] private float bounciness = 1;

    private bool _hasSurfaceRef;
    private bool _outOfBounds;
    private TriangleSurface _triangleSurface;

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _prevContact = Vector3.zero;

    private void Awake()
    {
        _hasSurfaceRef = triangleSurfaceRef != null;

        if (!_hasSurfaceRef)
        {
            Debug.LogWarning($"{gameObject.name} has no reference to surface!");
            return;
        }

        _triangleSurface = triangleSurfaceRef.GetComponent<TriangleSurface>();
        _hasSurfaceRef = _triangleSurface != null;

        if (!_hasSurfaceRef) Debug.LogWarning($"{gameObject.name} has no reference to surface!");
    }

    private void FixedUpdate()
    {
        var transform1 = transform;
        var position = transform1.position;

        var rollingCoefficient = _velocity.magnitude > 1e-15f ? rollingResistance : 0;

        var netForce = Physics.gravity * mass;

        if (_hasSurfaceRef)
        {
            var hit = _triangleSurface.ProjectOntoSurface(position);
            _prevContact = hit.Point;
            
            var distVec = position - hit.Point;
            var dist = distVec.magnitude;

            if (dist <= radius)
            {
                if (Mathf.Abs(dist - radius) > 0.5f * radius)
                    transform.position += (radius - dist) * distVec.normalized;

                var parallelUnitVector = Vector3.ProjectOnPlane(_velocity, hit.HitNormal).normalized;
                _velocity = -bounciness * Vector3.Dot(_velocity, hit.HitNormal) * hit.HitNormal +
                            Vector3.ProjectOnPlane(_velocity, hit.HitNormal);

                var normalForceMagnitude = Vector3.Dot(netForce, hit.HitNormal);

                netForce -= (hit.HitNormal - rollingCoefficient * parallelUnitVector) * normalForceMagnitude;
            }

            if (!_outOfBounds && hit.HitNormal.magnitude < 1e-15f)
            {
                _outOfBounds = true;
                Destroy(this);
            }
        }

        var acceleration = netForce / mass;
        _velocity += acceleration * Time.fixedDeltaTime;

        transform1.Translate(_velocity * Time.fixedDeltaTime);

        Debug.Log($"Position: {position} | " +
                  $"Velocity {_velocity} | " +
                  $"Acceleration {acceleration}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_prevContact, 0.3f);
    }
}