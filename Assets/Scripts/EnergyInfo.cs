using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyInfo : MonoBehaviour {
    public float energyAmount;

    public void SetEnergyAmount(float amount) {
        energyAmount = amount;
    }
    public float GetEnergyAmount() {
        return energyAmount;
    }
}
