using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("VISUAL")]
    public Slider m_HealthSlider; //Health feedback is represented on a slider

    public void MaxHealth(float health)
    {
        m_HealthSlider.maxValue = m_HealthSlider.value = health; //Set the initial life values
    }

    public void HealtH(float health)
    { 
        m_HealthSlider.value = health; //Upgrade the life value on the slider value
    }
}