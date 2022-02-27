//Bryan Leister, Feb. 2022
//Script to use a current GPS position to move a player in the Unity world and place objects
//either randomly or using GPS coordinates. As the player gets close to the objects,
//A shadergraph shader changes the color of the object
// My Testing data locations
// Start Position   Home =          39.77528, -105.06448
// Location 1       Kendall St. =   39.77488, -105.06622
// Location 2       Ingalls St. =   39.77544, -105.06391
//              10 Meters north =                39.77537, -105.06448
//
//Distance calculator - http://edwilliams.org/gccalc.htm

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LocalProximityGame.Scripts
{
    public class ProximityManager : MonoBehaviour
    {


        [Tooltip("Player starts in the Scene at location 0,0,0")]
        public GameObject m_player;

        public float m_inputAccuracy = 5f;
        public float m_distanceAccuracy = 1f;
        [Tooltip("Prefab to spawn markers into the world")]
        public GameObject m_marker;
        public int m_markerCount = 4;
        [Tooltip("Maximum distance in meters that the marker is considered in range")]
        public float m_triggerDistance = 10f;
        [Tooltip("A plane to spawn the markers on")]
        public GameObject m_playingField;
        public TextMeshProUGUI m_coordinates;
        public TextMeshProUGUI m_offsetInfo;
        public TextMeshProUGUI m_nearestMarker;
        public bool m_useHeading;
        [Tooltip("The first coordinate should be the start position, else we will use the device location.")]
        public bool m_useFirstPresetAsStartPosition = true;
        [Tooltip("If empty, preset coordinates are not used")]
        public CoordinateManagerScriptableObject m_presets;

        private float _latitudeOffset = 111194.90f;
        private float _longitudeOffset = 85459.51f;
        //[HideInInspector] public List<GameObject> _markerList;
        private int _offsetTimerCount;
        private Vector3 _offset;
        private bool _updatePlayerPosition = false;
        private List<MarkerBehavior> _markers;
        private List<GameObject> _inRangeMarkers;
        private readonly int _offsetTimer = 20;

        private void Awake()
        {
            MeshRenderer mr = m_playingField.GetComponent<MeshRenderer>();
            MeshRenderer markermr = m_marker.GetComponent<MeshRenderer>();

            _markers = new List<MarkerBehavior>();
            _inRangeMarkers = new List<GameObject>();
            if (mr && markermr)
            {
                for (int i = 0; i < m_markerCount; i++)
                {
                    float x = Random.Range(mr.bounds.min.x,mr.bounds.max.x);
                    float z = Random.Range(mr.bounds.min.z,mr.bounds.max.z);
                    GameObject m = Instantiate(m_marker, new Vector3(x, mr.bounds.max.y + (markermr.bounds.max.y / 2), z),
                        Quaternion.identity);
                    m.name = "m_" + i;
                    MarkerBehavior mb = m.GetComponent<MarkerBehavior>();
                    _markers.Add(mb);
                }
            }
            if(m_presets.presetCoordinates.Length > 0)
                _offset = new Vector3((float)m_presets.presetCoordinates[0].latitude, (float)m_presets.presetCoordinates[0].altitude,
                (float)m_presets.presetCoordinates[0].longitude);

        }

        IEnumerator Start()
        {
            if (!Input.location.isEnabledByUser)
                yield break;

            Input.location.Start(m_inputAccuracy,m_distanceAccuracy);
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait < 1)
            {
                Debug.Log("Timed out");
                yield break;
            }
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Unable to determine device location");
                yield break;
            }

            Input.compass.enabled = m_useHeading;
            if (!m_useFirstPresetAsStartPosition)
                _offset = new Vector3(Input.location.lastData.latitude, 0, Input.location.lastData.longitude);

            InvokeRepeating(nameof(SetStartPosition),1f,.2f);
            InvokeRepeating(nameof(SetLongitudeAndLatitudeOffset), 0f, 10f);
        }


        private void LateUpdate()
        {


            if (_updatePlayerPosition && Input.location.status == LocationServiceStatus.Running)
            {

                if(m_useHeading)
                    m_player.transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);

                Vector3 pos = new Vector3((Input.location.lastData.latitude - _offset.x) * _latitudeOffset, 0, (Input.location.lastData.longitude - _offset.z) * _longitudeOffset);
                m_player.transform.position = Vector3.Lerp(m_player.transform.position, pos, Time.deltaTime);
                if (m_offsetInfo)
                {
                    float distanceTraveled = CalculateDistance(_offset.x, Input.location.lastData.latitude, _offset.z,
                        Input.location.lastData.longitude);
                    m_offsetInfo.text = "Distance from start is " + distanceTraveled + " meters";
                }

                if (m_nearestMarker)
                {
                    int i = 0;
                    string msg = "";
                    foreach (var marker in _markers)
                    {
                        if (marker.m_isInRange)
                        {
                            msg += marker.name + " is in range.\n";
                            if (!_inRangeMarkers.Contains(marker.gameObject))
                            {
                                _inRangeMarkers.Add(marker.gameObject);
                                Handheld.Vibrate();
                            }

                            i++;
                        }
                        else
                        {
                            if (_inRangeMarkers.Contains(marker.gameObject))
                            {
                                _inRangeMarkers.Remove(marker.gameObject);
                            }
                        }
                    }
                    m_nearestMarker.text = msg + "\n" + i + " total";
                }
            }

        }

        void SetStartPosition()
        {
            //We have a list of coordinates, use them to make markers!
            if(m_presets.presetCoordinates.Length > 0)
            {
                _updatePlayerPosition = true;
                foreach (var mark in m_presets.presetCoordinates)
                    MakeMarkerAtLocation(mark);

                Debug.Log("Offset is set to " + _offset.ToString("F4"));
                CancelInvoke(nameof(SetStartPosition));
            }

            //Give the location data time to settle down and become more accurate...
            _offsetTimerCount++;
            if (m_offsetInfo)
                m_offsetInfo.text = _offsetTimerCount + " _offset is " + _offset ;
            if (_offsetTimerCount >= _offsetTimer)
            {
                if(m_presets.presetCoordinates.Length > 0)
                    _offset = new Vector3(Input.location.lastData.latitude, 0f, Input.location.lastData.longitude);
                _updatePlayerPosition = true;
                if (m_offsetInfo)
                    m_offsetInfo.text = _offsetTimerCount + " _offset is " + _offset.ToString("F6") ;
                CancelInvoke(nameof(SetStartPosition));
            }
        }

        GameObject MakeMarkerAtLocation(PresetCoordinates mark)
        {
            Vector3 coordinates = new Vector3((float)mark.latitude, (float)mark.altitude, (float)mark.longitude);
            Vector3 distanceFromPlayerOrigin = coordinates - _offset;
            //Debug.Log( label + " is " + distanceFromPlayerOrigin.ToString("F6"));
            GameObject g = Instantiate(m_marker, new Vector3(distanceFromPlayerOrigin.x * _latitudeOffset, 0, distanceFromPlayerOrigin.z * _longitudeOffset), Quaternion.identity);
            g.name = mark.label;
            if (mark.includeInMarkerList)
            {
                MarkerBehavior mb = g.GetComponent<MarkerBehavior>();
                _markers.Add(mb);
            }

            return g;
        }

        private IEnumerator OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus)
            {
                Input.location.Stop();
                CancelInvoke();
            }
            else
            {
                Input.location.Start();
                int maxWait = 20;
                while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
                {
                    yield return new WaitForSeconds(1);
                    maxWait--;
                }
                if (maxWait < 1)
                {
                    print("Timed out");
                    yield break;
                }
                if (Input.location.status == LocationServiceStatus.Failed)
                {
                    print("Unable to determine device location");
                    yield break;
                }

                Input.compass.enabled = m_useHeading;
                InvokeRepeating("SetStartPosition",1f,.2f);
                InvokeRepeating("SetLongitudeAndLatitudeOffset", 0f, 10f);
            }
        }

        private float CalculateDistance(float lat_1, float lat_2, float long_1, float long_2)
        {
            int R = 6371;
            var lat_rad_1 = Mathf.Deg2Rad * lat_1;
            var lat_rad_2 = Mathf.Deg2Rad * lat_2;
            var d_lat_rad = Mathf.Deg2Rad * (lat_2 - lat_1);
            var d_long_rad = Mathf.Deg2Rad * (long_2 - long_1);
            var a = Mathf.Pow(Mathf.Sin(d_lat_rad / 2), 2) + (Mathf.Pow(Mathf.Sin(d_long_rad / 2), 2) * Mathf.Cos(lat_rad_1) * Mathf.Cos(lat_rad_2));
            var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            var total_dist = R * c * 1000; // convert to meters
            return total_dist;
        }

        void SetLongitudeAndLatitudeOffset()
        {
            float horizontalAccuracy = 0;
            Vector3 currentPosition = _offset;

            if (!Input.location.isEnabledByUser)
                Debug.Log("Location is not enabled!");
            else
                horizontalAccuracy = Input.location.lastData.horizontalAccuracy;

            if (!m_useFirstPresetAsStartPosition)
                currentPosition = new Vector3(Input.location.lastData.latitude, 0, Input.location.lastData.longitude);

            //W longitude distance should be a negative value in Colorado
            float roundedLongitude = Mathf.RoundToInt(currentPosition.z);
            _longitudeOffset = -CalculateDistance(currentPosition.x, currentPosition.x, roundedLongitude, roundedLongitude + 1);

            float roundedLatitude = Mathf.RoundToInt(currentPosition.x);
            _latitudeOffset = CalculateDistance(roundedLatitude, roundedLatitude + 1, currentPosition.z, currentPosition.z);

            string message = "1 degree of longitude is " + _longitudeOffset.ToString("F6") + " meters at this latitude" +
                             "\n1 degree of latitude is " + _latitudeOffset.ToString("F6") + " meters at this longitude" +
                             "\ncurrent location is lat: "+ currentPosition.x + " lon: " +  currentPosition.z + " alt: " + currentPosition.y +
                             "\nhorizontal accuracy is " + horizontalAccuracy.ToString("F4");
            if (m_coordinates)
                m_coordinates.text = message;
        }

        public void SetTriggerDistance(float distance)
        {
            m_triggerDistance = distance;
        }

    }

    [Serializable]
    public class PresetCoordinates
    {
        public double latitude;
        public double longitude;
        public double altitude;
        public string label = "Marker";
        public bool includeInMarkerList = true;
    }
}
