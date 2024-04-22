using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller : RaycastController {

    // The minimum and max angle in the exclusion angle (angle where you cant move on slope)
    float exclusionAngleMin = 80;
    float exclusionAngleMax = 100;

    
    public CollisionInfo collisionInfo;

    public override void Start() {
        base.Start();       // call the Start() method for RaycastController
    }

    // Handles moving objects. Called in Player.Update()
    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();     // update raycast based on position
        collisionInfo.Reset();      // reset the info every frame

        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }
        
        transform.Translate(velocity);
    }

    // this is kind of a fucking mess and I want to consolidate
    void HorizontalCollisions(ref Vector3 velocity) {
        
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        bool bottom = false;
        bool top = false;

        // Loop through horizontal rays looking for collision detection
        for (int i = 0; i < horizontalRayCount; i++) {
            // Determine origin of raycast
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);   // adjust origin based on spacing
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionX * rayLength * Vector2.right, Color.red);
 
            if (hit) {
                // Calculate the slope angle of the surface
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                if (i == 0 && slopeAngle <= exclusionAngleMin) {
                    collisionInfo.below = true;
                    bottom = true;

                    // Handles if there is a new slope angle while moviing
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    // Adjust velocity on slope
                    Slope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }
               else if (i == (horizontalRayCount - 1) && slopeAngle >= exclusionAngleMax) {
                    collisionInfo.above = true;
                    top = true;

                    // Smooths out / brings the hitbox closer to the slope
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    // Adjust velocity on slope
                    Slope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // If rays are detecting slopes above and below, stop
                if(top == true && bottom == true) {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    velocity.y = (hit.distance - skinWidth) * Mathf.Sign(velocity.y);
                }
                // Update velocity and ray length based on collision
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                // Handle if on slope but hitting wall sticking out
                if (!collisionInfo.onSlope || (slopeAngle > exclusionAngleMin && slopeAngle < exclusionAngleMax)) {
                    // Somehow this gets called
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

        // Determine origin of raycast
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.red);

            if (hit) {
                // Update velocity and ray length based on collision
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisionInfo.below = directionY == -1; // if hit something going down then collisionInfo.below is true
                collisionInfo.above = directionY == 1; // if hit something going up then collisionInfo.above is true
            }
        }

        // Handles if there is a new slope angle while moviing
        if (collisionInfo.onSlope) {
            float directionX = Mathf.Sign(velocity.x);      // Get direction of x movement
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            // Determine the origin point of the ray based on direction of movement
            // if left: origin bottemLeft; if right: origin bottomright
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;

            // cast a ray to detect the slope
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if(hit) {
                // Calculate the angle of the slope based on the surface normal
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // If the detected slope angle is different from the previous one
                if(slopeAngle != collisionInfo.slopeAngle) {
                    // adjust the horizontal velocity to follow the slope
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    // update angle
                    collisionInfo.slopeAngle = slopeAngle;
                }
            }
        }
    }

    // Method to adjust velocity on slope
    void Slope(ref Vector3 velocity, float slopeAngle) {
        // set object as on slope
        collisionInfo.onSlope = true;
        // Calculate the distance of movement along the slope
        float moveDistance = Mathf.Abs(velocity.x);
        // Calculate the vertical velocity component along the slope
        float slopeVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // This is to check if jumping
        if (velocity.y <= slopeVelocityY) {
            // Calculate the vertical velocity component along the slope
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionInfo.slopeAngle = slopeAngle;

            // Adjust velocity based on the objects position relative to the slope
            if (collisionInfo.below) {
                velocity.y = slopeVelocityY;
            }
            else if (collisionInfo.above) {
                velocity.y = slopeVelocityY * -1;
            }
        }  
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
