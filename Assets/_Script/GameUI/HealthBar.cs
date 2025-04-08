using UnityEngine;
using UnityEngine.UI;

// Script này sẽ được gắn vào một GameObject có chứa UI Slider
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient; // Gradient màu sắc cho thanh máu
    public Image fill; // Phần fill image của slider
    
    void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        if (fill == null && slider != null)
            fill = slider.fillRect.GetComponent<Image>();
    }
    
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(1f);
    }
    
    public void SetHealth(int health)
    {
        slider.value = health;
        
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}