using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]          // references the Controller script
public class Player : MonoBehaviour {

    public float accelerationTime = .3f;
    public float moveSpeed = 3;
    Controller controller;
    Vector3 velocity;
    float velocityXSmoothing;
    float velocityYSmoothing;

    void Start() {
        controller = GetComponent<Controller>();   
       
    }
    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTime);

        float targetVelocityY = input.y * moveSpeed;
        velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, accelerationTime);

        controller.Move(velocity * Time.deltaTime);
        
    }

}
