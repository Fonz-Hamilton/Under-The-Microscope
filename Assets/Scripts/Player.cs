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

    public float maxEnergy = 100f;
    public float minEnergy = 0f;
    public float currentEnergy;

    public float maxHealth = 100f;
    public float minHealth = 0f;
    public float currentHealth;

    public float rateOfEnergyLoss = 2.5f;
    public float rateOfHealthLoss = 2.5f;
    public BarManager VitalsBar;

    Vector2 directionalInput;

    void Start() {


        controller = GetComponent<Controller>();

        currentEnergy = maxEnergy;
        currentHealth = maxHealth;
        VitalsBar.SetMinMaxEnergy(minEnergy, maxEnergy);
        VitalsBar.SetHealth(maxHealth);
       
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

        
        LoseEnergy(rateOfEnergyLoss * Time.deltaTime);

        if (currentEnergy <= 0) {
            LoseHealth(rateOfHealthLoss * Time.deltaTime);
        }
        
        if(Input.GetKeyDown(KeyCode.LeftShift)) {
            GainEnergy(5f);
        }

        
        
    }

    public void LoseEnergy(float lostEnergy) {
        currentEnergy -= lostEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, minEnergy, maxEnergy);
        VitalsBar.SetEnergy(currentEnergy);
    }
    public void GainEnergy(float gainEnergy) {
        currentEnergy += gainEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, minEnergy, maxEnergy);
        VitalsBar.SetEnergy(currentEnergy);
    }
    public void LoseHealth(float lostHealth) {
        currentHealth -= lostHealth;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        VitalsBar.healthBar.fillAmount = currentHealth / 100f;
    }

}
