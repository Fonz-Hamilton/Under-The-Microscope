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
    public float minEnergy = 0;
    public float currentEnergy;
    public BarManager energyBar;

    Vector2 directionalInput;

    void Start() {


        controller = GetComponent<Controller>();

        currentEnergy = maxEnergy;
        energyBar.SetMinMaxEnergy(minEnergy, maxEnergy);
       
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

        
        LoseEnergy(.5f * Time.deltaTime);
        
        if(Input.GetKeyDown(KeyCode.LeftShift)) {
            GainEnergy(5f);
        }
        
    }

    public void LoseEnergy(float lostEnergy) {
        currentEnergy -= lostEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, minEnergy, maxEnergy);
        energyBar.SetEnergy(currentEnergy);
    }
    public void GainEnergy(float gainEnergy) {
        currentEnergy += gainEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, minEnergy, maxEnergy);
        energyBar.SetEnergy(currentEnergy);
    }

}
