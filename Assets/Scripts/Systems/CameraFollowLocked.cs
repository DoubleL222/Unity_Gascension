using Systems;
using UnityEngine;

public class CameraFollowLocked : MonoBehaviour
{
    public Transform target;
    public float damping = 1;
    [Range(0,1)]
    public float screenSpaceUsage = 1;
    public float lookAheadFactor = 3;
    public float lookAheadReturnSpeed = 0.5f;
    public float lookAheadMoveThreshold = 0.1f;
    public bool NeverMoveDownwards = true;

    private float m_OffsetZ;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    void Start(){
        if (!target)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if(go)
                target = go.transform;
        }

        if(target)
            SetTarget(target.gameObject, true);
    }
    public void SetTarget(GameObject newTarget, bool moveToTargetInstantly, float zValue = -10)
    {
        if (newTarget == null)
        {
            target = null;
            return;
        }

        target = newTarget.transform;
        if (moveToTargetInstantly)
            transform.position = new Vector3(newTarget.transform.position.x, newTarget.transform.position.y, zValue);
        m_LastTargetPosition = target.position;
        m_OffsetZ = (transform.position - target.position).z;
    }

    // Update is called once per frame
    private void Update()
    {
        if (target != null)
        {
            // only update lookahead pos if accelerating or changed direction
            float yMoveDelta = (target.position - m_LastTargetPosition).y;

            bool updateLookAheadTarget = Mathf.Abs(yMoveDelta) > lookAheadMoveThreshold;

            if (updateLookAheadTarget)
            {
                m_LookAheadPos = lookAheadFactor * Vector3.up * Mathf.Sign(yMoveDelta);
            }
            else
            {
                m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);
            newPos = new Vector3(0, newPos.y, newPos.z);

            if (NeverMoveDownwards && newPos.y < transform.position.y){
                newPos.y = transform.position.y;
            }

            transform.position = newPos;
            m_LastTargetPosition = target.position;
        }
    }
}