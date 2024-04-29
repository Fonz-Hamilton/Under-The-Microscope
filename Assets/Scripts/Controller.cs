using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller : RaycastController {

    // The minimum and max angle in the exclusion angle (angle where you cant move on slope)
    float exclusionAngleMin = 80;
    float exclusionAngleMax = 100;
    [HideInInspector]
    public Vector2 playerInput;

    
    public Player player;
    public EnergyInfo energyInfo;

    
    public CollisionInfo collisionInfo;

    public override void Start() {
        base.Start();       // call the Start() method for RaycastController
        // Find the player GameObject with the "Player" tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        // Get the Player component attached to the player GameObject
        if (playerObject != null) {
            player = playerObject.GetComponent<Player>();
        }
    }

    // Overload method so objectController doesnt have to worry about player input
    public void Move(Vector2 deltaVelocity) {
        Move(deltaVelocity, Vector2.zero);
    }

    // Handles moving objects. Called in Player.Update()
    public void Move(Vector2 deltaVelocity, Vector2 input) {
        UpdateRaycastOrigins();     // update raycast based on position
        collisionInfo.Reset();      // reset the info every frame
        playerInput = input;

        if (deltaVelocity.x != 0) {
            HorizontalCollisions(ref deltaVelocity);
        }

        if (deltaVelocity.y != 0) {
            VerticalCollisions(ref deltaVelocity);
        }
        
        transform.Translate(deltaVelocity);
    }

    // this is kind of a fucking mess and I want to consolidate
    void HorizontalCollisions(ref Vector2 deltaVelocity) {
        
        float directionX = Mathf.Sign(deltaVelocity.x);
        float rayLength = Mathf.Abs(deltaVelocity.x) + skinWidth;

        bool bottom = false;
        bool top = false;

        // Loop through horizontal rays looking for collision detection
        for (int i = 0; i < horizontalRayCount; i++) {
            // Determine origin of raycast
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);   // adjust origin based on spacing
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionX * Vector2.right, Color.red);
 
            if (hit) {
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Energy")) {
                    CollectEnergy(hit.collider.gameObject);
                }

                // if stuck in a moving object, this allows to skip ahead to the next ray
                // Might need to take out
                if (hit.distance == 0) {
                    continue;
                }
                // Calculate the slope angle of the surface
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                if (i == 0 && slopeAngle <= exclusionAngleMin) {
                    collisionInfo.below = true;
                    bottom = true;

                    // Handles if there is a new slope angle while moviing
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        deltaVelocity.x -= distanceToSlopeStart * directionX;
                    }

                    // Adjust deltaVelocity on slope
                    Slope(ref deltaVelocity, slopeAngle);
                    deltaVelocity.x += distanceToSlopeStart * directionX;
                }
               else if (i == (horizontalRayCount - 1) && slopeAngle >= exclusionAngleMax) {
                    collisionInfo.above = true;
                    top = true;

                    // Smooths out / brings the hitbox closer to the slope
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        deltaVelocity.x -= distanceToSlopeStart * directionX;
                    }
                    // Adjust deltaVelocity on slope
                    Slope(ref deltaVelocity, slopeAngle);
                    deltaVelocity.x += distanceToSlopeStart * directionX;
                }

                // If rays are detecting slopes above and below, stop
                if(top == true && bottom == true) {
                    deltaVelocity.x = (hit.distance - skinWidth) * directionX;
                    deltaVelocity.y = (hit.distance - skinWidth) * Mathf.Sign(deltaVelocity.y);
                }
                // Update deltaVelocity and ray length based on collision
                deltaVelocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                // Handle if on slope but hitting wall sticking out
                if (!collisionInfo.onSlope || (slopeAngle > exclusionAngleMin && slopeAngle < exclusionAngleMax)) {
                    // Somehow this gets called
                    if (collisionInfo.onSlope) {
                        deltaVelocity.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaVelocity.x);
                    }
                }
                collisionInfo.left = directionX == -1; // if hit something going left then collisionInfo.left is true
                collisionInfo.right = directionX == 1; // if hit something going right then collisionInfo.right is true
            }
        } 
    }

    void VerticalCollisions(ref Vector2 deltaVelocity) {
        
        float directionY = Mathf.Sign(deltaVelocity.y);
        float rayLength = Mathf.Abs(deltaVelocity.y) + skinWidth;

        // Determine origin of raycast
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + deltaVelocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionY * Vector2.up, Color.red);

            if (hit) {
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Energy")) {
                    CollectEnergy(hit.collider.gameObject);
                }
                // if stuck in a moving object, this allows to skip ahead to the next ray
                // Might need to take out
                if (hit.distance == 0) {
                    continue;
                }
                // Update deltaVelocity and ray length based on collision
                deltaVelocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisionInfo.below = directionY == -1; // if hit something going down then collisionInfo.below is true
                collisionInfo.above = directionY == 1; // if hit something going up then collisionInfo.above is true
            }
        }

        // Handles if there is a new slope angle while moviing
        if (collisionInfo.onSlope) {
            float directionX = Mathf.Sign(deltaVelocity.x);      // Get direction of x movement
            rayLength = Mathf.Abs(deltaVelocity.x) + skinWidth;
            // Determine the origin point of the ray based on direction of movement
            // if left: origin bottemLeft; if right: origin bottomright
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * deltaVelocity.y;

            // cast a ray to detect the slope
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if(hit) {
                // Calculate the angle of the slope based on the surface normal
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // If the detected slope angle is different from the previous one
                if(slopeAngle != collisionInfo.slopeAngle) {
                    // adjust the horizontal deltaVelocity to follow the slope
                    deltaVelocity.x = (hit.distance - skinWidth) * directionX;
                    // update angle
                    collisionInfo.slopeAngle = slopeAngle;
                }
            }
        }
    }

    // Method to adjust deltaVelocity on slope
    void Slope(ref Vector2 deltaVelocity, float slopeAngle) {
        // set object as on slope
        collisionInfo.onSlope = true;
        // Calculate the distance of movement along the slope
        float moveDistance = Mathf.Abs(deltaVelocity.x);
        // Calculate the vertical deltaVelocity component along the slope
        float slopeDeltaVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // This is to check if jumping
        if (deltaVelocity.y <= slopeDeltaVelocityY) {
            // Calculate the vertical deltaVelocity component along the slope
            deltaVelocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaVelocity.x);
            collisionInfo.slopeAngle = slopeAngle;

            // Adjust deltaVelocity based on the objects position relative to the slope
            if (collisionInfo.below) {
                deltaVelocity.y = slopeDeltaVelocityY;
            }
            else if (collisionInfo.above) {
                deltaVelocity.y = slopeDeltaVelocityY * -1;
            }
        }  
    }

    private void CollectEnergy(GameObject energy) {
        
        if(player != null) {
            // Cache the EnergyInfo component reference
            EnergyInfo energyInfo = energy.GetComponent<EnergyInfo>();
            if (energyInfo != null) {
                // Gain energy based on the energy amount from the EnergyInfo component
                player.GainEnergy(energyInfo.GetEnergyAmount());
            }
            else {
                Debug.Log("Energy: bitch is null");
            }

        }
        else {
            Debug.Log("Player: bitch is null");
        }
        Destroy(energy);
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
