using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
    public LayerMask collisionMask;

    // (if testing: current = 0.015) Two colliders can penetrate each
    // other as deep as their Skin Width. Larger Skin Widths reduce jitter.
    // Low Skin Width can cause the character to get stuck.
    // A good setting is to make this value 10% of the Radius - Unity docs
    public const float skinWidth = .015f;

    // Ray counts
    const float dstBetweenRays = .25f;
    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    // Ray spacing
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D colliders;             // a reference to the collider component of the object
    [HideInInspector]
    public RaycastOrigins raycastOrigins;

    public virtual void Awake() {
        colliders = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public virtual void Start() {
        CalculateRaySpacing();
    }


    public void UpdateRaycastOrigins() {
        Bounds bounds = colliders.bounds;       // bounding box. Uses colliders bounds
        bounds.Expand(skinWidth * -2);          // reduces the bounds from where the rays originate
                                                // by skinWidth * -2

        // Set values for struct
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing() {
        Bounds bounds = colliders.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        // Takes the spacing bounds.size and divides the area by ray count. -1 cause minumum is 2 ray casts
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    // Vector2 Objects for raycast
    public struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}

*/