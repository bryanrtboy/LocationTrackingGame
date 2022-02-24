using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
[RequireComponent(typeof(Slider))]
public class ShowSliderValue : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI m_sliderLabel;
    public string m_prefix = "Trigger Distance ";
    public string m_suffix = "";
    public string m_key = "Distance";

    private Slider _slider;
    float savedValue = 0f;
    private void Awake()
    {
        _slider = GetComponent<Slider>();
        savedValue = _slider.value;
        if (PlayerPrefs.HasKey(m_key))
        {
            savedValue = PlayerPrefs.GetFloat(m_key);
            UpdateSliderLabel(savedValue);
            _slider.value = savedValue;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(m_key,savedValue);
    }

    public void UpdateSliderLabel(float value)
    {
        m_sliderLabel.text =m_prefix + value.ToString("F2") + m_suffix;
        savedValue = value;
    }
}
