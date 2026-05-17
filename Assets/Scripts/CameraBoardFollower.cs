using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBoardFollower : MonoBehaviour
{
    [SerializeField]
    private float Padding = 0.5f;

    [SerializeField]
    // Kept under the existing serialized name so the current scene data still loads cleanly.
    private bool FitBoardToView = true;

    [SerializeField]
    private float FollowSmoothTime = 0.15f;

    [SerializeField]
    private float DeadZone = 0.5f;

    private Camera m_Camera;
    private Vector3 m_Velocity;

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.BoardManager == null || GameManager.Instance.PlayerController == null)
        {
            return;
        }

        BoardManager board = GameManager.Instance.BoardManager;
        Transform target = GameManager.Instance.PlayerController.transform;
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, currentPosition.z);

        if (Mathf.Abs(target.position.x - currentPosition.x) < DeadZone)
        {
            targetPosition.x = currentPosition.x;
        }

        if (Mathf.Abs(target.position.y - currentPosition.y) < DeadZone)
        {
            targetPosition.y = currentPosition.y;
        }

        if (m_Camera != null && m_Camera.orthographic)
        {
            targetPosition = board.ClampCameraPosition(targetPosition, m_Camera.orthographicSize, m_Camera.aspect, Padding);
        }

        if (FitBoardToView)
        {
            transform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref m_Velocity, FollowSmoothTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}
