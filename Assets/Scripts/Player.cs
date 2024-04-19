using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]          // references the Controller script
public class Player : MonoBehaviour {

    float moveSpeed = 6;
    Controller controller;
    Vector3 velocity;

    void Start() {
        controller = GetComponent<Controller>();   
       
    }
    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        velocity.x = input.x * moveSpeed;
        velocity.y = input.y * moveSpeed;
        controller.Move(velocity * Time.deltaTime);
        
    }

}
