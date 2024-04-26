using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarManager : MonoBehaviour {

    
    public Slider slider;

    public void SetMinMaxEnergy(float energyMin, float energyMax) {
       
        slider.maxValue = energyMax;
        slider.minValue = energyMin;
        slider.value = energyMax;
    }
    public void SetEnergy(float energy) {
        
        slider.value = energy;
    }

}

