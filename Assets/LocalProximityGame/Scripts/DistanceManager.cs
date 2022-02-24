using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DistanceManager : MonoBehaviour
{
    public float m_triggerDistance = 1f;
    public TextMeshProUGUI m_distance;
    public ProximityManager m_manager;
    private string _nearestMarker = "";

    private void Awake()
    {
        //Set this string to the key you are using in the Slider label update script
        if (PlayerPrefs.HasKey("Distance"))
        {
            m_triggerDistance = PlayerPrefs.GetFloat("Distance");
            SetDistance(m_triggerDistance);
        }
    }

    private void Start()
    {
        if(m_manager == null)
            m_manager = FindObjectOfType<ProximityManager>();

        InvokeRepeating("UpdateMarkers",1,.3f);
    }

    private void UpdateMarkers()
    {
        float _nearestMarkerDistance = 1000000f;

        foreach (var marker in m_manager._markerList)
        {
            float dist = Vector3.Distance(m_manager.m_player.transform.position, marker.transform.position);
            if ( dist < _nearestMarkerDistance)
            {
                _nearestMarkerDistance = dist;
                _nearestMarker = marker.gameObject.name;
            }
        }
        UpdateDistance(_nearestMarkerDistance,_nearestMarker);
    }

    public void SetDistance(float value)
    {
        m_triggerDistance = value;
    }

    public void UpdateDistance(float value, string marker)
    {
        if (m_distance != null)
        {
            m_distance.text = "Distance to " + marker + ": " + value.ToString("F") + "m";
        }

    }
}
