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

    public float maxEnergy = 100;
    public float currentEnergy;
    public BarManager energyBar;

    Vector2 directionalInput;

    void Start() {
        controller = GetComponent<Controller>();

        currentEnergy = maxEnergy;
        energyBar.SetMaxEnergy(maxEnergy);
       
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

        if(Input.GetKeyDown(KeyCode.Space)) {
            LoseEnergy(10f);
        }
        if(Input.GetKeyDown(KeyCode.LeftShift)) {
            GainEnergy(10f);
        }
        
    }

    public void LoseEnergy(float lostEnergy) {
        currentEnergy -= lostEnergy;
        energyBar.SetEnergy(currentEnergy);
    }
    public void GainEnergy(float gainEnergy) {
        currentEnergy += gainEnergy;
        energyBar.SetEnergy(currentEnergy);
    }

}
