using UnityEngine;

[ExecuteAlways]
public class CameraFollowing : MonoBehaviour
{
    [SerializeField] private Transform m_FollowTarget;
    [SerializeField] private Vector3 _offset = new(0, 0, -10);

    [Range(0, 1f)][SerializeField] private float smoothTime = 0.2f;

    private Vector3 _currentVelocity = Vector3.zero;

    private void OnValidate()
    {
        if (m_FollowTarget == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");

            if (player != null) m_FollowTarget = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (m_FollowTarget == null) return;

        Vector3 targetPosition = m_FollowTarget.position + _offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            smoothTime
        );
    }
}   