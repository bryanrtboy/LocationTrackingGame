using UnityEngine;

namespace LocalProximityGame.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform m_target;
        public float m_smoothSpeed = .125f;
        public bool m_lookAt = false;
        public bool m_matchTargetDirection = false;
        private Vector3 _offset;
        private void Awake()
        {
            _offset = transform.position;
        }
        private void FixedUpdate()
        {
            Vector3 desiredPosition = m_target.position + _offset;
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, m_smoothSpeed);

            transform.position = smoothPosition;
            if(m_lookAt)
                transform.LookAt(m_target);

            if (m_matchTargetDirection)
                transform.rotation = m_target.rotation;


        }


        public void MatchTargetDirection(bool value)
        {
            if(!value)
                transform.eulerAngles = Vector3.zero;
            m_matchTargetDirection = value;
        }
    }
}
