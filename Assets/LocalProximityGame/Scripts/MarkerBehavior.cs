using TMPro;
using UnityEngine;

namespace LocalProximityGame.Scripts
{
 public class MarkerBehavior : MonoBehaviour
 {
  public MeshRenderer m_mesh;
  public bool m_isInRange = false;
  public TextMeshPro m_label;

  private ProximityManager _proximityManager;
  private Material _material;
  [HideInInspector]
  public float m_distanceToPlayer = Mathf.Infinity;

  private static readonly int Proximity = Shader.PropertyToID("_Proximity");

  private void OnEnable()
  {
   _proximityManager = FindObjectOfType<ProximityManager>();
   _material = m_mesh.material;
   float f = Random.Range(.1f, .4f);
   InvokeRepeating("ProximityCheck", 1f,f);
   if (m_label)
   {
    m_label.gameObject.SetActive(false);
    Invoke("UpdateLabel", 2f);
   }
  }

  void UpdateLabel()
  {
   m_label.text = name;
  }

  void ProximityCheck()
  {
   m_distanceToPlayer = Vector3.Distance(_proximityManager.m_player.transform.position, this.transform.position);
   float normalizedDistance = Mathf.InverseLerp(0, _proximityManager.m_triggerDistance,
    m_distanceToPlayer);
   _material.SetFloat(Proximity, normalizedDistance);
   SetInRangeFlag(m_distanceToPlayer);
  }

 void SetInRangeFlag(float distance)
  {
   m_isInRange = distance <= _proximityManager.m_triggerDistance ? true : false;



   if(m_label && m_isInRange)
    m_label.gameObject.SetActive(true);
   else if(m_label)
    m_label.gameObject.SetActive(false);
  }

 }
}
