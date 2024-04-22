using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : RaycastController {
    public Vector3 move;
    public LayerMask pushMask;

    List<PushedEntity> pushedEntity;

    public override void Start() {
        base.Start();
    }


    void Update() {
        UpdateRaycastOrigins();

        Vector3 velocity = move * Time.deltaTime;
        CalculatePushAway(velocity);
        PushEntity();
        transform.Translate(velocity);
    }
    void PushEntity() {
        foreach (PushedEntity pushed in pushedEntity) {
            pushed.transform.GetComponent<Controller>().Move(pushed.velocity);
        }
    }

    void CalculatePushAway(Vector3 velocity) {
        HashSet<Transform> pushAway = new HashSet<Transform>();
        pushedEntity = new List<PushedEntity>();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, pushMask);

                Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.blue);

                if (hit) {
                    if(!pushAway.Contains(hit.transform)) {
                        pushAway.Add(hit.transform);

                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        pushedEntity.Add(new PushedEntity(hit.transform, new Vector3(pushX, pushY)));
                    } 
                }

            }
        }
        // Horizontally moving
        if(velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, pushMask);

                Debug.DrawRay(rayOrigin, directionX * rayLength * Vector2.right, Color.blue);

                if (hit) {
                    if (!pushAway.Contains(hit.transform)) {
                        pushAway.Add(hit.transform);

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
