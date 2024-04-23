using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]          // references the Controller script
public class Player : MonoBehaviour {

    public float accelerationTime = .3f;
    public float moveSpeed = 3;
    Controller controller;
    Vector2 velocity;
    float velocityXSmoothing;
    float velocityYSmoothing;

    Vector2 directionalInput;

    void Start() {
        controller = GetComponent<Controller>();   
       
    }

    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }

    void Update() {
        
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTime);

        float targetVelocityY = directionalInput.y * moveSpeed;
        velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, accelerationTime);

        controller.Move(velocity * Time.deltaTime, directionalInput);
        
    }

}
