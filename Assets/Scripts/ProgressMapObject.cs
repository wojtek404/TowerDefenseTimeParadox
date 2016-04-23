using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProgressMapObject : MonoBehaviour
{  
    public Slider slider;
    public Image image;
    public Sprite objAliveSprite;
    public Sprite objDeadSprite;

    void OnSpawn()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        slider.value = 0f;
        image.sprite = objAliveSprite;
    }

    public void CalculateProgress(float currentProgress)
    {
        slider.value = currentProgress;
    }
}