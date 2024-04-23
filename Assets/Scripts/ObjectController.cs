using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : RaycastController {
    public Vector3 move;
    public LayerMask pushMask;          // mask for things that can be pushed

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0,3)]
    public float easeAmount;
    
    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    List<PushedEntity> pushedEntity;    // List to store pushed objects
    Dictionary<Transform, Controller> pushedDictionary = new();

    public override void Start() {
        base.Start();                   // call the Start() method for RaycastController

        globalWaypoints = new Vector3[localWaypoints.Length];
        for(int i = 0; i < globalWaypoints.Length; i++) {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }


    void Update() {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculateObstacleMovement();

        CalculatePushAway(velocity);
        // Method for Enities speed
        PushEntity();
        // Movement of the moving object
        transform.Translate(velocity);
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculateObstacleMovement() {

        if(Time.time < nextMoveTime) {
            return Vector3.zero;
        }


        fromWaypointIndex %= globalWaypoints.Length; // resets from waypoint index when done
        // +1 cause we want next one in the array
        // reset when reaches global waypoints
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;    
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        // Lerp Interpolates between the points
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);
        // if you have reached the next waypoint
        if(percentBetweenWaypoints >= 1) {
            // reset percentage
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            // when we get to the end, reverse
            if((fromWaypointIndex >= globalWaypoints.Length - 1) && !cyclic) {
                fromWaypointIndex = 0;
                System.Array.Reverse(globalWaypoints);
            }

            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;

    }
    void PushEntity() {
        // Loop through each pushed entity
        foreach (PushedEntity pushed in pushedEntity) {
            if(!pushedDictionary.ContainsKey(pushed.transform)) {
                pushedDictionary.Add(pushed.transform, pushed.transform.GetComponent<Controller>());
            }
            // Call the move method of the Controller script attached to the pushed entity
            pushedDictionary[pushed.transform].Move(pushed.velocity);
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
        public Transform transform;     // Transform the pushed entity
        public Vector3 velocity;        // Velocity to push the entity
        
        public PushedEntity(Transform _transform, Vector3 _velocity) {
            transform = _transform;
            velocity = _velocity;
        }
    }

    void OnDrawGizmos() {
        if(localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = .3f;

            for(int i = 0; i < localWaypoints.Length; i++) {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }    
    }
}
