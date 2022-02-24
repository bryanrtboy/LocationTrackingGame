//Bryan Leister, Feb. 2022
//Script to use a current GPS position to move a player in the Unity world and place objects
//either randomly or using GPS coordinates. As the player gets close to the objects,
//A shadergraph shader changes the color of the object

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ProximityManager : MonoBehaviour
{
    [Tooltip("Player should be in the Scene at location 0,0,0")]
    public GameObject m_player;
    [Tooltip("Prefab to spawn markers into the world")]
    public GameObject m_marker;
    public int m_markerCount = 4;
    [Tooltip("A plane to spawn the markers on")]
    public GameObject m_playingField;
    public TextMeshProUGUI m_coordinates;
    public TextMeshProUGUI m_offsetInfo;
    public int m_offsetTimer = 20;
    public float m_moveMultiplier = 50846;
    public bool m_useHeading = false;
    public bool m_usePresetCoordinates = false;

    private float _latitudeOffset = 111194.90f;
    private float _longitudeOffset = 85459.51f;

    [HideInInspector] public List<GameObject> _markerList;
    private int _offsetTimerCount;
    private Vector3 _offset;
    private bool _updatePlayerPosition = false;
    private void Awake()
    {
        MeshRenderer mr = m_playingField.GetComponent<MeshRenderer>();
        MeshRenderer markermr = m_marker.GetComponent<MeshRenderer>();
        _markerList = new List<GameObject>();
        if (mr && markermr)
        {
            //_markers = new GameObject[m_markerCount];
            for (int i = 0; i < m_markerCount; i++)
            {
                float x = UnityEngine.Random.Range(mr.bounds.min.x,mr.bounds.max.x);
                float z = UnityEngine.Random.Range(mr.bounds.min.z,mr.bounds.max.z);
                GameObject m = Instantiate(m_marker, new Vector3(x, mr.bounds.max.y + (markermr.bounds.max.y / 2), z),
                    Quaternion.identity);
                m.name = "m_" + i;
               //_markers[i] = m;
               _markerList.Add(m);
            }
        }

        //Set this string to the key you are using in the Slider label update script
        if (PlayerPrefs.HasKey("Multiplier"))
        {
            m_moveMultiplier = PlayerPrefs.GetFloat("Multiplier");
        }
    }

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
            yield break;

        Input.location.Start(1,1);
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
        else
        {
            Input.compass.enabled = m_useHeading;
            _offset = new Vector3(Input.location.lastData.latitude, 0, Input.location.lastData.longitude);
            InvokeRepeating("SetStartPosition",1f,.2f);
            InvokeRepeating("SetLongitudeAndLatitudeOffset", 0f, 10f);

        }
    }

    private void Update()
    {
        if(m_useHeading && Input.location.status == LocationServiceStatus.Running)
            m_player.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
    }

    private void LateUpdate()
    {

        if (_updatePlayerPosition && Input.location.status == LocationServiceStatus.Running)
        {
            Vector3 pos = new Vector3((Input.location.lastData.latitude - _offset.x) * _latitudeOffset, 0, (Input.location.lastData.longitude - _offset.z) * _longitudeOffset);
            m_player.transform.position = Vector3.Lerp(m_player.transform.position, pos, Time.deltaTime);
            if (m_offsetInfo)
            {
                float distanceTraveled = CalculateDistance(_offset.x, Input.location.lastData.latitude, _offset.z,
                    Input.location.lastData.longitude);
                m_offsetInfo.text = "Distance from start is " + distanceTraveled + " meters";
            }
        }

    }

    void SetStartPosition()
    {
        //Testing data locations
        // Start Position   42nd Avenue = 39.77528f, -105.06448f
        // Location 1       Kendall St. = 39.77488f, 0, -105.06622f
        // Location 2       Ingalls St. = 39.77544f, -105.06391f
        //
        //Distance calculator - http://edwilliams.org/gccalc.htm

        if (m_usePresetCoordinates)
        {
            _offset = new Vector3(39.77528f, 0, -105.06448f);
            _updatePlayerPosition = true;
            MakeMarkerAtLocation(new Vector3(39.77488f, 0, -105.06622f), "Kendall", true);
            MakeMarkerAtLocation(new Vector3(39.77544f, 0, -105.06391f), "Ingalls", true);
            MakeMarkerAtLocation(_offset, "Home", false);
            MakeMarkerAtLocation(new Vector3(39.77537f, 0, _offset.z), "10m North", true);
            Debug.Log("Offset is set to " + _offset.ToString("F4"));
            CancelInvoke();
        }

        //Give the location data time to settle down and become more accurate...
        _offsetTimerCount++;
        if (m_offsetInfo)
            m_offsetInfo.text = _offsetTimerCount.ToString() + " _offset is " + _offset.ToString() ;
        if (_offsetTimerCount >= m_offsetTimer)
        {
            if(!m_usePresetCoordinates && Input.location.isEnabledByUser)
                _offset = new Vector3(Input.location.lastData.latitude, 0f, Input.location.lastData.longitude);
            _updatePlayerPosition = true;
            Debug.Log("Offset is set to " + _offset);
            if (m_offsetInfo)
                m_offsetInfo.text = _offsetTimerCount.ToString() + " _offset is " + _offset.ToString("F6") ;
            CancelInvoke();
        }
    }

    GameObject MakeMarkerAtLocation(Vector3 coordinates, string label, bool addToList)
    {
        Vector3 distanceFromPlayerOrigin = coordinates - _offset;
        Debug.Log( label + " is " + distanceFromPlayerOrigin.ToString("F6"));
        GameObject g = Instantiate(m_marker, new Vector3(distanceFromPlayerOrigin.x * _latitudeOffset, 0, distanceFromPlayerOrigin.z * _longitudeOffset), Quaternion.identity);
        g.name = label;
        if(addToList)
            _markerList.Add(g);

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
            else
            {
                Input.compass.enabled = m_useHeading;
                InvokeRepeating("SetStartPosition",1f,.2f);
                InvokeRepeating("SetLongitudeAndLatitudeOffset", 0f, 10f);
            }
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
        Vector3 currentPosition = new Vector3(Input.location.lastData.latitude, 0, Input.location.lastData.longitude);

        if(Input.location.isEnabledByUser)
            currentPosition = new Vector3(39.77527f, 0, -105.0645f);

        float roundedLongitude = Mathf.RoundToInt(currentPosition.z);
        //W longitude distance should be a negative value
        _longitudeOffset = -CalculateDistance(currentPosition.x, currentPosition.x, roundedLongitude, roundedLongitude + 1);

        float roundedLatitude = Mathf.RoundToInt(currentPosition.x);
        _latitudeOffset = CalculateDistance(roundedLatitude, roundedLatitude + 1, currentPosition.z, currentPosition.z);
        string message = "1 degree of longitude is " + _longitudeOffset.ToString("F6") + " meters at this latitude" +
                         "\n1 degree of latitude is " + _latitudeOffset.ToString("F6") + " meters at this longitude" +
                         "\ncurrent location is lat: "+ Input.location.lastData.latitude  + " lon: " +  Input.location.lastData.longitude + " alt: " + Input.location.lastData.altitude +
                         "\nhorizontal accuracy is " + Input.location.lastData.horizontalAccuracy;
        if (m_coordinates)
            m_coordinates.text = message;
    }

}
