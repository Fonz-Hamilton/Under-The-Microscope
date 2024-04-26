using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarManager : MonoBehaviour {

    
    public Slider slider;

    public void SetMaxEnergy(float energy) {
       
        slider.maxValue = energy;
        slider.value = energy;
    }
    public void SetEnergy(float energy) {
        
        slider.value = energy;
    }

}

/*energyAmount = Mathf.Clamp(energyAmount, 0, 100);
        energyBar.fillAmount = energyAmount / 100;
*/
