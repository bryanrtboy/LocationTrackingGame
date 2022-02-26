using TMPro;
using UnityEngine;

namespace LocalProximityGame.Scripts
{
   public class PlayerPosition : MonoBehaviour
   {
      public TextMeshPro m_label;

      private void Start()
      {
         InvokeRepeating("UpdateText",1,.2f);
      }

      void UpdateText()
      {
         if (m_label)
            m_label.text = "^\n" + transform.position.ToString("F4");
      }
   }
}
