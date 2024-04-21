using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller : MonoBehaviour {

    const float skinWidth = .015f;
    public LayerMask collisionMask;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float exclusionAngleMin = 80;
    float exclusionAngleMax = 100;

    float horizontalRaySpacing;
    float verticalRaySpacing;
    new BoxCollider2D collider;
    RaycastOrigins raycastOrigins;

    public CollisionInfo collisionInfo;

    void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();              // maybe put back in update if needed
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        collisionInfo.Reset();
        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }

        transform.Translate(velocity);
    }

    void HorizontalCollisions(ref Vector3 velocity) {
        
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionX * rayLength * Vector2.right, Color.red);
 
            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                if (i == 0 && slopeAngle <= exclusionAngleMin) {
                    collisionInfo.below = true;
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    Slope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }
                if (i == (horizontalRayCount - 1) && slopeAngle >= exclusionAngleMax) {
                    collisionInfo.above = true;
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    Slope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                if (!collisionInfo.onSlope || (slopeAngle > exclusionAngleMin && slopeAngle < exclusionAngleMax)) {
                    if (collisionInfo.onSlope) {
                        velocity.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }
                }
                
                collisionInfo.left = directionX == -1; // if hit something going left then collisionInfo.left is true
                collisionInfo.right = directionX == 1; // if hit something going right then collisionInfo.right is true
            }
        } 
    }

    void VerticalCollisions(ref Vector3 velocity) {
        
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.red);

            if (hit) {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisionInfo.below = directionY == -1; // if hit something going down then collisionInfo.below is true
                collisionInfo.above = directionY == 1; // if hit something going up then collisionInfo.above is true
            }
        }
        
    }

    void Slope(ref Vector3 velocity, float slopeAngle) {
        collisionInfo.onSlope = true;
        float moveDistance = Mathf.Abs(velocity.x);
        float slopeVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        

        if (collisionInfo.below) {
            if(velocity.y <= slopeVelocityY) {
                velocity.y = slopeVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisionInfo.slopeAngle = slopeAngle;
            }
            
        }
        else if (collisionInfo.above) {
            if(velocity.y <= slopeVelocityY) {
                velocity.y = slopeVelocityY * -1;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisionInfo.slopeAngle = slopeAngle;
            }
            
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    void CalculateRaySpacing() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public struct CollisionInfo {
        public bool above, below, left, right;
        public bool onSlope;
        public float slopeAngle ,slopeAngleOld;

        public void Reset() {
            above = below = false;
            left = right = false;
            onSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
