using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LocalProximityGame.Scripts
{
 public class MarkerBehavior : MonoBehaviour
 {
  public MeshRenderer m_mesh;
  public bool m_isInRange = false;
  private ProximityManager _proximityManager;
  private Material _material;
  [HideInInspector]
  public float m_distanceToPlayer = Mathf.Infinity;
  private void OnEnable()
  {
   _proximityManager = FindObjectOfType<ProximityManager>();
   _material = m_mesh.material;
   float f = Random.Range(.1f, .4f);
   InvokeRepeating("ProximityCheck", 1f,f);
  }

  void ProximityCheck()
  {
   m_distanceToPlayer = Vector3.Distance(_proximityManager.m_player.transform.position, this.transform.position);
   float normalizedDistance = Mathf.InverseLerp(0, _proximityManager.m_triggerDistance,
    m_distanceToPlayer);
   _material.SetFloat("_Proximity", normalizedDistance);
   SetInRangeFlag(m_distanceToPlayer);
  }

 void SetInRangeFlag(float distance)
  {
   m_isInRange = distance <= _proximityManager.m_triggerDistance ? true : false;
  }

 }
}
