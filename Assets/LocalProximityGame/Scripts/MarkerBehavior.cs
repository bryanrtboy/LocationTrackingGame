
using UnityEngine;

public class MarkerBehavior : MonoBehaviour
{
 public MeshRenderer m_mesh;

 private DistanceManager _distanceManager;
 private Material _material;
 private void OnEnable()
 {
  _distanceManager = FindObjectOfType<DistanceManager>();
  _material = m_mesh.material;
  float f = Random.Range(.1f, .4f);
  InvokeRepeating("ProximityCheck", 1f,f);
 }

 void ProximityCheck()
 {
  float normalizedDistance = Mathf.InverseLerp(0, _distanceManager.m_triggerDistance,
   Vector3.Distance(_distanceManager.m_manager.m_player.transform.position, this.transform.position));
  _material.SetFloat("_Proximity", normalizedDistance);
 }
}
