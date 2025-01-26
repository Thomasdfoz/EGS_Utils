using UnityEngine;


namespace EGS.Utils 
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector3 m_Offset = new Vector3(0f, 1f, 1f);
        [SerializeField] private float m_Speed = 0.05f;

        public float positionLerpSpeed = 5f;  // Speed at which the position lerps towards the target.

        private void OnEnable()
        {
            SetupPosition();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        public void SetParameters(Transform target, Vector3 offset, float speed)
        {
            m_Target = target;
            m_Offset = offset;
            m_Speed = speed;

            SetupPosition();
        }

        private void UpdatePosition()
        {
            if (m_Target == null)
            {
                return;
            }

            Vector3 forward = m_Target.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 targetPosition = m_Target.position + m_Target.forward * m_Offset.z + m_Target.right * m_Offset.x;
            targetPosition.y = Mathf.Lerp(transform.position.y, m_Target.position.y + m_Offset.y, m_Speed * Time.deltaTime);

            // Smoothly interpolate the position
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_Speed * Time.deltaTime);

            Vector3 dir = m_Target.position - transform.position;
            dir.y = 0f;
            transform.forward = -dir;
        }

        private void SetupPosition()
        {
            if (m_Target == null)
            {
                return;
            }

            transform.position = new Vector3(transform.position.x, m_Offset.y, transform.position.z);
            Vector3 forward = m_Target.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            transform.position = m_Target.transform.position + forward * m_Offset.z;

            Vector3 dir = m_Target.position - transform.position;
            dir.y = 0f;
            transform.forward = -dir;
        }
    }
}
