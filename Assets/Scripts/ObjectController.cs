using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : RaycastController {
    public Vector3 move;
    public LayerMask pushMask;          // mask for things that can be pushed

    List<PushedEntity> pushedEntity;    // List to store pushed objects

    public override void Start() {
        base.Start();                   // call the Start() method for RaycastController
    }


    void Update() {
        UpdateRaycastOrigins();

        Vector3 velocity = move * Time.deltaTime;

        CalculatePushAway(velocity);
        // Method for Enities speed
        PushEntity();
        // Movement of the moving object
        transform.Translate(velocity);
    }
    void PushEntity() {
        // Loop through each pushed entity
        foreach (PushedEntity pushed in pushedEntity) {
            // Call the move method of the Controller script attached to the pushed entity
            pushed.transform.GetComponent<Controller>().Move(pushed.velocity);
        }
    }

    void CalculatePushAway(Vector3 velocity) {
        // HashSet to store objects that have been pushed
        HashSet<Transform> pushAway = new HashSet<Transform>();
        pushedEntity = new List<PushedEntity>();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                // Determine origin of raycast
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);      // adjust origin based on spacing
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, pushMask);

                Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.blue);

                if (hit) {
                    // If the hit object has not been pushed yet
                    if (!pushAway.Contains(hit.transform)) {
                        pushAway.Add(hit.transform);

                        // Calculate the push force
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        // Add the pushed entity to the pushed list
                        pushedEntity.Add(new PushedEntity(hit.transform, new Vector3(pushX, pushY)));
                    } 
                }

            }
        }
        // Horizontally moving
        if(velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                // Determine origin of raycast
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);       // adjust origin based on spacing
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, pushMask);

                Debug.DrawRay(rayOrigin, directionX * rayLength * Vector2.right, Color.blue);

                if (hit) {
                    // If the hit object has not been pushed yet
                    if (!pushAway.Contains(hit.transform)) {
                        pushAway.Add(hit.transform);

                        // Calculate the push force
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = 0;

                        pushedEntity.Add(new PushedEntity(hit.transform, new Vector3(pushX, pushY)));
                    }
                }

            }

        }
    }
    // keeps the data of the entity that is veing moved / pushed by the moving object
    struct PushedEntity {
        public Transform transform;
        public Vector3 velocity;
        
        public PushedEntity(Transform _transform, Vector3 _velocity) {
            transform = _transform;
            velocity = _velocity;
        }
    }
}
