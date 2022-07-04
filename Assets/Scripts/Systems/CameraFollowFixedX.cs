using UnityEngine;
using System.Collections;

namespace  PC2D
{
    public class CameraFollowFixedX : MonoBehaviour
    {
        public Transform target;
        private Vector3 m_CurrentVelocity;
        public float damping = 1;
        private float m_OffsetZ;


        // Update is called once per frame
        void Update()
        {
            m_OffsetZ = (transform.position - target.position).z;

            Vector3 pos = transform.position;
            //pos.x = target.position.x;
            pos.x = 0f;
            pos.y = target.position.y;

            Vector3 aheadTargetPos;
            aheadTargetPos = target.position + Vector3.forward * m_OffsetZ;

            Vector3 newPos = Vector3.SmoothDamp(pos, aheadTargetPos, ref m_CurrentVelocity, damping);
            newPos = new Vector3(0, newPos.y, newPos.z);
            transform.position = newPos;
        }
    }
}
