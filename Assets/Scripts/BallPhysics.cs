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

using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject triangleSurfaceRef;

    [Header("Physical properties")] [SerializeField] [Min(1e-6f)]
    private float mass = 1;

    [SerializeField] [Min(0)] private float radius = 1;
    [SerializeField] [Min(0)] private float kineticFrictionCoefficient = 0;
    [SerializeField] [Range(0, 1)] private float bounciness = 1;

    private bool _hasSurfaceRef;
    private TriangleSurface _triangleSurface;

    private Vector3 _velocity = Vector3.zero;

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
        
        if (!_hasSurfaceRef)
        {
            Debug.LogWarning($"{gameObject.name} has no reference to surface!");
        }
    }

    private void FixedUpdate()
    {
        Vector3 netForce = Physics.gravity * mass;
        float frictionCoefficient = _velocity.magnitude > 1e-15f ? kineticFrictionCoefficient : 0;
        
        if (_hasSurfaceRef)
        {
            var hit = _triangleSurface.ProjectOntoSurface(transform.position);
            if (Vector3.Distance(hit.Point, transform.position) <= radius)
            {
                Vector3 parallelUnitVector = Vector3.ProjectOnPlane(_velocity, hit.HitNormal).normalized;
                _velocity = -bounciness*Vector3.Dot(_velocity, hit.HitNormal)*hit.HitNormal + Vector3.ProjectOnPlane(_velocity, hit.HitNormal);
                
                float normalForceMagnitude = Vector3.Dot(netForce, hit.HitNormal);
                
                netForce -= (hit.HitNormal - frictionCoefficient*parallelUnitVector)*normalForceMagnitude;
            }
        }

        _velocity += netForce * Time.fixedDeltaTime / mass;
        transform.Translate(_velocity*Time.fixedDeltaTime);
    }
}